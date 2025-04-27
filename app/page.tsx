'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'

export default function Home() {
  const router = useRouter();
  
  useEffect(() => {
    // Всегда перенаправляем на страницу авторизации
    router.push('/login');
  }, [router]);

  // Показываем загрузку во время перенаправления
  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-4">
      <h1 className="text-2xl font-bold mb-4">Перенаправление...</h1>
      <p className="mb-4">Перенаправление на страницу авторизации.</p>
    </div>
  );
}
