import { NextResponse } from 'next/server';
import { employeeQueries } from '@/db/queries';
import { clerkClient } from '@clerk/nextjs/server';
import { auth } from '@clerk/nextjs/server';
import fs from 'fs/promises';
import path from 'path';

// Simple file-based mutex to prevent race conditions
async function acquireLock(): Promise<boolean> {
  const lockFile = path.join(process.cwd(), 'user-creation.lock');
  try {
    await fs.writeFile(lockFile, Date.now().toString(), { flag: 'wx' });
    return true;
  } catch (error) {
    // File exists, lock is held
    return false;
  }
}

async function releaseLock(): Promise<void> {
  const lockFile = path.join(process.cwd(), 'user-creation.lock');
  try {
    await fs.unlink(lockFile);
  } catch (error) {
    console.error('Error releasing lock:', error);
  }
}

// Этот эндпоинт проверяет, является ли текущий пользователь первым в системе
// и если да, то назначает ему права администратора
export async function GET() {
  let lockAcquired = false;
  
  try {
    // Получаем ID текущего пользователя
    const { userId } = await auth();
    
    if (!userId) {
      return NextResponse.json({ error: 'Не авторизован' }, { status: 401 });
    }
    
    // Try to acquire lock before proceeding with user creation
    lockAcquired = await acquireLock();
    if (!lockAcquired) {
      // Wait a moment and check if user was created by another process
      await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    // Получаем все записи сотрудников
    const employees = await employeeQueries.getAll();
    
    // Проверяем, существует ли уже сотрудник с таким clerk ID
    const existingEmployee = employees.find(emp => emp.clerkId === userId);
    
    if (existingEmployee) {
      // Пользователь уже существует в базе данных
      if (lockAcquired) await releaseLock();
      
      // Проверим, есть ли у него права администратора в Clerk
      const clerk = await clerkClient();
      const user = await clerk.users.getUser(userId);
      
      if (user && user.publicMetadata && (user.publicMetadata as any).role === 'admin') {
        return NextResponse.json({ 
          success: true, 
          isAdmin: true,
          alreadyExists: true,
          message: 'Вы уже являетесь администратором системы'
        });
      }
      
      // Если пользователь существует, но не имеет прав администратора, и нет других сотрудников,
      // то назначаем ему права администратора
      if (employees.length === 1) {
        await clerk.users.updateUser(userId, {
          publicMetadata: {
            role: "admin"
          }
        });
        
        return NextResponse.json({ 
          success: true, 
          isAdmin: true,
          alreadyExists: true,
          message: 'Вы стали администратором системы'
        });
      }
      
      return NextResponse.json({ 
        success: false, 
        isAdmin: false,
        alreadyExists: true,
        message: 'Вы уже зарегистрированы, но не являетесь администратором'
      });
    }
    
    // Проверяем, есть ли уже сотрудники в базе данных
    const isFirstUser = employees.length === 0;
    
    if (!isFirstUser) {
      if (lockAcquired) await releaseLock();
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
      if (lockAcquired) await releaseLock();
      return NextResponse.json({ error: 'Пользователь не найден' }, { status: 404 });
    }
    
    // Получаем email пользователя
    const email = user.emailAddresses[0]?.emailAddress;
    
    if (!email) {
      if (lockAcquired) await releaseLock();
      return NextResponse.json({ error: 'Email пользователя не найден' }, { status: 400 });
    }
    
    // Проверяем, есть ли уже сотрудник с таким email
    const emailEmployee = employees.find(emp => emp.email === email);
    if (emailEmployee) {
      if (lockAcquired) await releaseLock();
      console.log('User with this email already exists');
      return NextResponse.json({ 
        success: true,
        isAdmin: true,
        alreadyExists: true,
        message: 'Пользователь с таким email уже существует'
      });
    }
    
    // Проверяем еще раз, не был ли пользователь добавлен через webhook
    const updatedEmployees = await employeeQueries.getAll();
    const nowExistingEmployee = updatedEmployees.find(emp => emp.clerkId === userId);
    
    if (nowExistingEmployee) {
      // Если пользователь уже существует (возможно добавлен через webhook),
      // просто убедимся, что у него есть права администратора
      await clerk.users.updateUser(userId, {
        publicMetadata: {
          role: "admin"
        }
      });
      
      if (lockAcquired) await releaseLock();
      return NextResponse.json({ 
        success: true, 
        isAdmin: true,
        alreadyExists: true,
        message: 'Вы уже являетесь администратором системы (добавлен через webhook)'
      });
    }
    
    // Устанавливаем роль администратора в метаданных Clerk
    await clerk.users.updateUser(userId, {
      publicMetadata: {
        role: "admin"
      }
    });
    
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
    
    if (lockAcquired) await releaseLock();
    return NextResponse.json({ 
      success: true, 
      isAdmin: true,
      message: 'Вы стали первым администратором системы'
    });
  } catch (error) {
    if (lockAcquired) await releaseLock();
    console.error('Ошибка при установке прав администратора:', error);
    return NextResponse.json(
      { error: 'Ошибка при установке прав администратора' },
      { status: 500 }
    );
  }
} 