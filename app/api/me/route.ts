import { NextResponse } from 'next/server'
import { currentUser, auth } from '@clerk/nextjs/server'

export async function GET() {
  // Получаем userId из текущей сессии
  const { userId } = await auth()

  // Защищаем маршрут, проверяя, авторизован ли пользователь
  if (!userId) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 })
  }

  try {
    // Получаем данные пользователя через Clerk API
    const user = await currentUser()

    // Форматируем данные для отправки клиенту
    const userData = {
      id: user?.id,
      firstName: user?.firstName,
      lastName: user?.lastName,
      email: user?.emailAddresses[0]?.emailAddress,
      imageUrl: user?.imageUrl,
      publicMetadata: user?.publicMetadata
    }

    return NextResponse.json(userData)
  } catch (error) {
    console.error('Error fetching user data:', error)
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    )
  }
}