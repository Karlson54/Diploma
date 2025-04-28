"use client"
import { WeeklyCalendar } from "@/components/weekly-calendar"
import { DashboardHeader } from "@/components/dashboard-header"
import { SimpleSidebar } from "@/components/simple-sidebar"
import { withAuth } from "@/lib/AuthContext"
import { useEffect, useState } from 'react';
import { useUser } from '@clerk/nextjs';

function DashboardPage() {
  const { user, isLoaded } = useUser();
  const [adminCheckDone, setAdminCheckDone] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    // Проверяем, является ли пользователь первым и назначаем ему права админа если да
    const checkFirstUser = async () => {
      if (isLoaded && user) {
        try {
          const response = await fetch('/api/auth/first-user');
          const data = await response.json();
          
          if (data.success && data.isAdmin) {
            // Первый пользователь, ставший админом
            setMessage('Вы стали первым администратором системы!');
            
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
      }
    };
    
    if (!adminCheckDone) {
      checkFirstUser();
    }
  }, [isLoaded, user, adminCheckDone]);

  return (
    <div className="flex h-screen bg-gray-50">
      <SimpleSidebar />
      <div className="flex-1 flex flex-col overflow-hidden">
        <DashboardHeader isAdmin={false} />
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          <WeeklyCalendar />
        </main>
      </div>
    </div>
  )
}

// Экспортируем компонент с защитой авторизации
export default withAuth(DashboardPage);
