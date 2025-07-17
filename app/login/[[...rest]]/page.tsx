'use client'

import React, { useState, useEffect } from 'react'
import { useSignIn } from '@clerk/nextjs'
import { useAuthContext } from '@/lib/AuthContext'
import { useTranslation } from 'react-i18next'

export default function LoginPage() {
  const { t } = useTranslation()

  const { isAuthenticated, isLoading, redirectToDashboard } = useAuthContext()
  const { isLoaded, signIn, setActive } = useSignIn()
  
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [verificationCode, setVerificationCode] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [needsVerification, setNeedsVerification] = useState(false)
  const [registrationAllowed, setRegistrationAllowed] = useState(false)
  const [registrationStatusLoading, setRegistrationStatusLoading] = useState(true)

  // Перенаправление если пользователь уже авторизован
  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      redirectToDashboard();
    }
  }, [isLoading, isAuthenticated, redirectToDashboard]);

  // Проверяем, разрешена ли регистрация
  useEffect(() => {
    async function checkRegistrationStatus() {
      // TODO: Implement this logic in the Future API Service
      // It should look something like this
      // const response = await Service.checkRegistrationStatus()
      // API SCHEMA:
      // {
      //   registrationAllowed: boolean
      // }
      // setRegistrationAllowed(response.registrationAllowed)
      setRegistrationAllowed(true)
    }

    checkRegistrationStatus();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!isLoaded) return;
    setIsSubmitting(true)
    setError('')
    
    try {
      // Создаем попытку входа по email и паролю
      const signInAttempt = await signIn.create({
        identifier: email,
        password,
      })
      
      // Проверяем статус входа
      if (signInAttempt.status === 'complete') {
        // Если вход успешный, устанавливаем сессию и перенаправляем
        await setActive({ session: signInAttempt.createdSessionId })
        redirectToDashboard();
      } else if (signInAttempt.status === 'needs_second_factor') {
        // Требуется двухфакторная аутентификация
        setNeedsVerification(true)
      } else if (signInAttempt.status === 'needs_identifier') {
        setError('Пожалуйста, введите email')
      } else if (signInAttempt.status === 'needs_first_factor') {
        setError('Пожалуйста, введите пароль')
      } else {
        setError('Произошла ошибка входа. Пожалуйста, проверьте ваши данные.')
      }
    } catch (err: any) {
      setError(err.message || 'Ошибка входа')
    } finally {
      setIsSubmitting(false)
    }
  }
  
  const handleVerification = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!isLoaded) return;
    setIsSubmitting(true)
    setError('')
    
    try {
      // Выполняем проверку кода
      const resp = await signIn.attemptSecondFactor({
        strategy: 'totp',
        code: verificationCode,
      })
      
      if (resp.status === 'complete') {
        // Если вход успешный, устанавливаем сессию и перенаправляем
        await setActive({ session: resp.createdSessionId })
        redirectToDashboard();
      } else {
        setError('Неверный код верификации')
      }
    } catch (err: any) {
      setError(err.message || 'Ошибка верификации')
    } finally {
      setIsSubmitting(false)
    }
  }

  // Показываем загрузку пока проверяем авторизацию
  if (isLoading) {
    return (
      <div className="flex min-h-screen bg-gray-50 items-center justify-center">
        <div className="text-center">
          <h2 className="text-xl font-semibold">Загрузка...</h2>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen bg-gray-50 items-center justify-center">
      <div className="w-full max-w-md px-4 py-8 sm:px-6 sm:py-12 bg-white shadow-lg rounded-lg">
        <div className="flex justify-center">
          <div className="h-12 w-auto">
            <img
              src="https://hebbkx1anhila5yf.public.blob.vercel-storage.com/%D0%91%D0%B5%D0%B7%20%D0%B7%D0%B0%D0%B3%D0%BE%D0%BB%D0%BE%D0%B2%D0%BA%D0%B0-fUH90pbu2g9blr3Tk2CoJfZWlS4CiP.png"
              alt="Mediacom"
              className="h-full"
            />
          </div>
        </div>
        <h2 className="mt-6 text-center text-3xl font-bold tracking-tight text-gray-900">{t('auth.title')}</h2>
        <p className="mt-2 text-center text-sm text-gray-600">{t('auth.systemName')}</p>

        <div className="mt-8">
          {error && (
            <div className="mb-4 p-3 bg-red-50 text-red-500 text-sm rounded-md">
              {error}
            </div>
          )}
          
          {!needsVerification ? (
            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="space-y-4">
                <div className="space-y-2">
                  <label className="block text-sm font-medium text-gray-700">
                    {t('auth.email')}
                  </label>
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
                
                <div className="space-y-2">
                  <label className="block text-sm font-medium text-gray-700">
                    {t('auth.password')}
                  </label>
                  <input
                    type="password"
                    required
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
              </div>
              
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-blue-300"
              >
                {isSubmitting ? t('auth.loginInProgress') : t('auth.login')}
              </button>
              
              {registrationStatusLoading ? (
                <p className="mt-2 text-center text-sm text-gray-600">
                  {t('auth.registrationStatus')}
                </p>
              ) : registrationAllowed ? (
                <p className="mt-2 text-center text-sm text-gray-600">
                  {t('auth.notRegistered')}
                  <a href="/signup" className="font-medium text-blue-600 hover:text-blue-500">
                    {t('auth.createAccount')}
                  </a>
                </p>
              ) : (
                <p className="mt-2 text-center text-sm text-gray-600">
                  {t('auth.forbidden')}
                </p>
              )}
            </form>
          ) : (
            <form onSubmit={handleVerification} className="space-y-6">
              <header className="text-center">
                <h2 className="text-xl font-semibold">Двофакторна аутентифікація</h2>
                <p className="text-sm text-gray-600">Введіть код підтвердження</p>
              </header>
              
              <div className="space-y-2 mt-4">
                <label className="block text-sm font-medium text-gray-700">
                  {t('auth.code')}
                </label>
                <input
                  type="text"
                  required
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value)}
                  className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-blue-300 mt-4"
              >
                {isSubmitting ? t('auth.checking') : t('auth.submit')}
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  )
} 