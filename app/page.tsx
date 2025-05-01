'use client'

import { useEffect } from 'react'
import { useAuthContext } from '@/lib/AuthContext'
import { useTranslation } from 'react-i18next'

export default function Home() {
  const { t } = useTranslation()
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
      <h1 className="text-2xl font-bold mb-4">{t('auth.redirecting')}</h1>
      <p className="mb-4">{t('auth.checkingStatus')}</p>
    </div>
  );
}
