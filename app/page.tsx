'use client'

import { useEffect } from 'react'
import { useAuthContext } from '@/lib/AuthContext'

export default function Home() {
  const { isAuthenticated, isLoading, isAdmin, redirectToLogin } = useAuthContext();

  useEffect(() => {
    if (!isLoading) {
      if (isAuthenticated) {
        if (isAdmin) {
          window.location.replace('/admin/dashboard');
        } else {
          window.location.replace('/dashboard');
        }
      } else {
        redirectToLogin();
      }
    }
  }, [isLoading, isAuthenticated, isAdmin, redirectToLogin]);

  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-4">
      <h1 className="text-2xl font-bold mb-4">Перенаправление...</h1>
      <p className="mb-4">Проверка статуса авторизации...</p>
    </div>
  );
}
