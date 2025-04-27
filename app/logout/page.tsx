'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useClerk } from '@clerk/nextjs'

export default function LogoutPage() {
  const router = useRouter()
  const { signOut } = useClerk()

  useEffect(() => {
    // Автоматически выходим и перенаправляем на страницу логина
    const performSignOut = async () => {
      await signOut()
      router.push('/login')
    }
    
    performSignOut()
  }, [signOut, router])

  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-4">
      <h1 className="text-2xl font-bold mb-4">Выход из системы...</h1>
      <p>Выполняется выход из системы и перенаправление на страницу входа.</p>
    </div>
  )
} 