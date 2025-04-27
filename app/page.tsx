'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useClerk } from '@clerk/nextjs'

export default function Home() {
  const router = useRouter()
  const { signOut } = useClerk()

  useEffect(() => {
    // Перенаправляем на страницу логина
    router.push('/login')
  }, [router])

  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-4">
      <h1 className="text-2xl font-bold mb-4">Перенаправление...</h1>
      <p className="mb-4">Перенаправление на страницу авторизации.</p>
      <button 
        onClick={() => signOut(() => router.push('/login'))}
        className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
      >
        Выйти из аккаунта
      </button>
    </div>
  )
}
