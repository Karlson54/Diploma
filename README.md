# Mediacom - Система обліку робочого часу

Система обліку робочого часу на базі Next.js з аутентифікацією Clerk.

## Вимоги

- Node.js 18+
- npm або yarn

## Налаштування проекту

1. Клонуйте репозиторій
2. Встановіть залежності:

```bash
npm install
# або
yarn install
```

3. Налаштуйте Clerk
   - Створіть аккаунт на [clerk.com](https://clerk.com)
   - Створіть новий додаток
   - Скопіюйте ключі API (publishable key та secret key)
   - Створіть файл `.env.local` в корені проекту з такими змінними:

```
NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY=your_publishable_key
CLERK_SECRET_KEY=your_secret_key
NEXT_PUBLIC_CLERK_SIGN_IN_URL=/login
NEXT_PUBLIC_CLERK_SIGN_UP_URL=/signup
NEXT_PUBLIC_CLERK_AFTER_SIGN_IN_URL=/dashboard
NEXT_PUBLIC_CLERK_AFTER_SIGN_UP_URL=/dashboard
```

4. Запустіть проект:

```bash
npm run dev
# або
yarn dev
```

## Особливості

- **Аутентифікація**: Реалізована за допомогою Clerk
- **Захист маршрутів**: Використовується middleware для захисту маршрутів
- **Ролі користувачів**: Підтримка ролей (адміністратор, користувач)
- **Інтерфейс користувача**: Стилізований з використанням Tailwind CSS

## Налаштування Clerk

Для додавання ролей користувачів в Clerk:
1. Перейдіть до панелі керування Clerk
2. Виберіть вкладку "Users"
3. Виберіть користувача та перейдіть до "Public metadata"
4. Додайте поле `role` зі значенням `admin` для адміністраторів

## Примітки щодо конфігурації

### Middleware

В файлі `middleware.ts` використовується явний список маршрутів для застосування middleware, 
замість складних шаблонів з регулярними виразами, які можуть викликати помилки:

```typescript
export const config = {
  matcher: [
    '/',
    '/dashboard/:path*',
    '/admin/:path*',
    '/api/:path*',
    '/reports/:path*',
    '/profile/:path*',
    '/login/:path*',
    '/signup/:path*',
    '/forgot-password/:path*'
  ],
};
```

При додаванні нових маршрутів, які повинні бути захищені або публічні, не забудьте оновити:
1. Список `matcher` в `middleware.ts` для застосування middleware
2. Функцію `isPublicRoute` для правильного визначення публічних маршрутів

### Сторінки авторизації (catch-all routes)

Для правильної роботи компонентів `SignIn` та `SignUp` від Clerk, сторінки авторизації повинні бути налаштовані як catch-all routes:

- `/login/[[...rest]]/page.tsx` - для входу
- `/signup/[[...rest]]/page.tsx` - для реєстрації
- `/forgot-password/[[...rest]]/page.tsx` - для відновлення паролю

Ця структура дозволяє Clerk використовувати внутрішні маршрути ці маршрути для обробки різних станів автентифікації.