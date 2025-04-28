import { NextResponse } from 'next/server';
import { employeeQueries } from '@/db/queries';
import { clerkClient } from '@clerk/nextjs/server';

export async function GET() {
  try {
    // Проверка существующих пользователей в нашей базе данных
    const employees = await employeeQueries.getAll();
    const hasLocalUsers = employees.length > 0;
    
    // Если у нас уже есть пользователи в базе данных, сразу запрещаем регистрацию
    if (hasLocalUsers) {
      return NextResponse.json({ 
        registrationAllowed: false,
        message: 'Регистрация отключена (администратор уже существует в локальной базе данных)'
      });
    }
    
    // Проверка существующих пользователей в Clerk
    const clerk = await clerkClient();
    const clerkUsers = await clerk.users.getUserList({
      limit: 1, // Нам нужно знать только, есть ли хотя бы один пользователь
    });
    const hasClerkUsers = clerkUsers.data.length > 0;
    
    // Кэширование ответа на 5 минут для уменьшения числа запросов к API
    const headers = {
      'Cache-Control': 'max-age=300, s-maxage=300, stale-while-revalidate=60'
    };
    
    // Регистрация разрешена только если нет пользователей ни в одной из баз данных
    const registrationAllowed = !hasClerkUsers;
    
    return NextResponse.json({ 
      registrationAllowed,
      localUsers: hasLocalUsers,
      clerkUsers: hasClerkUsers,
      message: registrationAllowed 
        ? 'Регистрация разрешена (первый пользователь-администратор)' 
        : 'Регистрация отключена (администратор уже существует)'
    }, { headers });
  } catch (error) {
    console.error('Ошибка при проверке статуса регистрации:', error);
    // В случае ошибки, по умолчанию разрешаем регистрацию, чтобы избежать блокировки
    return NextResponse.json({ 
      registrationAllowed: true,
      message: 'Регистрация разрешена (ошибка при проверке)'
    });
  }
} 