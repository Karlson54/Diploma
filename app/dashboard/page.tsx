"use client"
import { WeeklyCalendar } from "@/components/weekly-calendar"
import { SimpleSidebar } from "@/components/simple-sidebar"
import { withAuth } from "@/lib/AuthContext"
import { useEffect, useState } from 'react';
import { useUser } from '@clerk/nextjs';

function DashboardPage() {
  const { user, isLoaded } = useUser();
  const [adminCheckDone, setAdminCheckDone] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    // Проверка будет выполнена только один раз при загрузке компонента
    const checkFirstUser = async () => {
      if (!isLoaded || !user) return;
      
      try {
        // Добавляем случайное число в URL, чтобы избежать кэширования браузером
        const response = await fetch(`/api/auth/first-user?t=${Date.now()}`);
        
        // Если запрос не успешен, прекращаем выполнение
        if (!response.ok) {
          console.error('Ошибка при проверке первого пользователя:', response.statusText);
          setAdminCheckDone(true);
          return;
        }
        
        const data = await response.json();
        
        // Если пользователь уже существует, просто отмечаем проверку как выполненную
        if (data.alreadyExists) {
          setAdminCheckDone(true);
          
          // Если пользователь является администратором, показываем сообщение
          if (data.isAdmin) {
            setMessage(data.message || 'Вы администратор системы');
          }
          return;
        }
        
        // Если пользователь стал администратором, показываем сообщение
        if (data.success && data.isAdmin) {
          setMessage(data.message || 'Вы стали первым администратором системы!');
          
          // Обновляем страницу через 2 секунды, чтобы обновить данные о пользователе
          setTimeout(() => {
            window.location.reload();
          }, 2000);
        }
        
        setAdminCheckDone(true);
      } catch (error) {
        console.error('Ошибка при проверке первого пользователя:', error);
        setAdminCheckDone(true);
      }
    };
    
    // Вызываем функцию проверки только если она еще не была выполнена
    if (!adminCheckDone && isLoaded && user) {
      checkFirstUser();
    }
  }, [isLoaded, user, adminCheckDone]);

  return (
    <div className="flex h-screen bg-gray-50">
      <SimpleSidebar />
      <div className="flex-1 flex flex-col overflow-hidden">
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          {message && (
            <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
              {message}
            </div>
          )}
          <WeeklyCalendar />
        </main>
      </div>
    </div>
  )
}

// Экспортируем компонент с защитой авторизации
export default withAuth(DashboardPage);
