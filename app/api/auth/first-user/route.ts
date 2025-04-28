import { NextResponse } from 'next/server';
import { employeeQueries } from '@/db/queries';
import { clerkClient } from '@clerk/nextjs/server';
import { auth } from '@clerk/nextjs/server';

// Этот эндпоинт проверяет, является ли текущий пользователь первым в системе
// и если да, то назначает ему права администратора
export async function GET() {
  try {
    // Получаем ID текущего пользователя
    const { userId } = await auth();
    
    if (!userId) {
      return NextResponse.json({ error: 'Не авторизован' }, { status: 401 });
    }
    
    // Проверяем, есть ли уже сотрудники в базе данных
    const employees = await employeeQueries.getAll();
    const isFirstUser = employees.length === 0;
    
    if (!isFirstUser) {
      return NextResponse.json({ 
        success: false, 
        isAdmin: false,
        message: 'Вы не первый пользователь. Администратор уже существует.'
      });
    }
    
    // Получаем информацию о пользователе из Clerk
    const clerk = await clerkClient();
    const user = await clerk.users.getUser(userId);
    
    if (!user) {
      return NextResponse.json({ error: 'Пользователь не найден' }, { status: 404 });
    }
    
    // Устанавливаем роль администратора в метаданных Clerk
    await clerk.users.updateUser(userId, {
      publicMetadata: {
        role: "admin"
      }
    });
    
    // Получаем email пользователя
    const email = user.emailAddresses[0]?.emailAddress;
    
    if (!email) {
      return NextResponse.json({ error: 'Email пользователя не найден' }, { status: 400 });
    }
    
    // Добавляем пользователя в таблицу сотрудников
    const today = new Date().toISOString();
    await employeeQueries.add({
      name: `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'Admin User',
      email: email,
      position: 'Admin',
      department: 'Administration',
      joinDate: today,
      status: 'Active',
      clerkId: userId,
    });
    
    return NextResponse.json({ 
      success: true, 
      isAdmin: true,
      message: 'Вы стали первым администратором системы'
    });
  } catch (error) {
    console.error('Ошибка при установке прав администратора:', error);
    return NextResponse.json(
      { error: 'Ошибка при установке прав администратора' },
      { status: 500 }
    );
  }
} 