'use client'

import * as Clerk from '@clerk/elements/common'
import * as SignUp from '@clerk/elements/sign-up'

export default function SignUpPage() {
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
        <h2 className="mt-6 text-center text-3xl font-bold tracking-tight text-gray-900">Реєстрація</h2>
        <p className="mt-2 text-center text-sm text-gray-600">Система обліку робочого часу</p>

        <div className="mt-8">
          <SignUp.Root>
            <SignUp.Step name="start" className="space-y-6">
              <Clerk.GlobalError className="text-red-500 text-sm" />
              <div className="space-y-4">
                <Clerk.Field name="emailAddress" className="space-y-2">
                  <Clerk.Label className="block text-sm font-medium text-gray-700">
                    Email
                  </Clerk.Label>
                  <Clerk.Input
                    type="email"
                    required
                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                  <Clerk.FieldError className="text-red-500 text-xs" />
                </Clerk.Field>
                
                <Clerk.Field name="password" className="space-y-2">
                  <Clerk.Label className="block text-sm font-medium text-gray-700">
                    Пароль
                  </Clerk.Label>
                  <Clerk.Input
                    type="password"
                    required
                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                  <Clerk.FieldError className="text-red-500 text-xs" />
                </Clerk.Field>
              </div>
              
              <SignUp.Captcha className="empty:hidden" />
              
              <SignUp.Action
                submit
                className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                Зареєструватися
              </SignUp.Action>
              
              <p className="mt-2 text-center text-sm text-gray-600">
                Вже маєте аккаунт?{' '}
                <Clerk.Link
                  navigate="sign-in"
                  className="font-medium text-blue-600 hover:text-blue-500"
                >
                  Увійти
                </Clerk.Link>
              </p>
            </SignUp.Step>
            
            <SignUp.Step name="verifications" className="space-y-6">
              <SignUp.Strategy name="email_code">
                <header className="text-center">
                  <h2 className="text-xl font-semibold">Перевірка email</h2>
                  <p className="text-sm text-gray-600">Введіть код, який ми відправили на вашу пошту</p>
                </header>
                
                <Clerk.Field name="code" className="space-y-2 mt-4">
                  <Clerk.Label className="block text-sm font-medium text-gray-700">
                    Код підтвердження
                  </Clerk.Label>
                  <Clerk.Input
                    required
                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                  <Clerk.FieldError className="text-red-500 text-xs" />
                </Clerk.Field>
                
                <SignUp.Action
                  submit
                  className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 mt-4"
                >
                  Завершити реєстрацію
                </SignUp.Action>
              </SignUp.Strategy>
            </SignUp.Step>
          </SignUp.Root>
        </div>
      </div>
    </div>
  )
} 