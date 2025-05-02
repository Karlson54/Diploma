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
        message: 'Регистрация отключена (в базе данных уже есть пользователи)'
      });
    }
    
    // Проверка существующих пользователей в Clerk
    const clerk = await clerkClient();
    const clerkUsers = await clerk.users.getUserList({
      limit: 10, // Увеличиваем лимит для надежности
    });
    const hasClerkUsers = clerkUsers.data.length > 0;
    
    // Регистрация разрешена только если нет пользователей ни в одной из баз данных
    const registrationAllowed = !hasClerkUsers;
    
    // Явно отключаем кэширование для этого эндпоинта,
    // чтобы всегда получать актуальные данные
    const headers = {
      'Cache-Control': 'no-store, no-cache, must-revalidate, proxy-revalidate',
      'Pragma': 'no-cache',
      'Expires': '0',
      'Surrogate-Control': 'no-store'
    };
    
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
    // В случае ошибки, по умолчанию запрещаем регистрацию для безопасности
    return NextResponse.json({ 
      registrationAllowed: false,
      message: 'Регистрация отключена (ошибка при проверке)'
    });
  }
} 