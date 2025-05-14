import { NextResponse } from 'next/server';
import { clientQueries } from '@/db/queries';
import { auth } from '@clerk/nextjs/server';

// Get all clients
export async function GET(request: Request) {
  try {
    // Проверка авторизации
    const { userId } = await auth();
    
    if (!userId) {
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }
    
    // Получаем всех клиентов из базы данных
    const clients = await clientQueries.getAll();
    
    // Возвращаем данные
    return NextResponse.json({ clients });
  } catch (error) {
    console.error('Error fetching clients:', error);
    return NextResponse.json({ error: 'Failed to fetch clients' }, { status: 500 });
  }
} 