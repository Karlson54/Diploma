"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { ChevronLeft, ChevronRight, Pencil, Trash, Plus, Copy } from "lucide-react"
import { DayEntryForm } from "@/components/day-entry-form"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { useTranslation } from "react-i18next"

// Define interfaces for our data types
interface Client {
  id: string;
  name: string;
}

interface Report {
  id: number;
  date: string;
  market: string;
  contractingAgency: string;
  client: string | number | { id: number | string; name: string };
  clientName?: string;
  projectBrand: string;
  media: string;
  jobType: string;
  comments: string;
  hours: number;
}

export function WeeklyCalendar() {
  const { t, i18n } = useTranslation()
  const [selectedDate, setSelectedDate] = useState<Date | null>(null)
  const [showEntryForm, setShowEntryForm] = useState(false)
  const [editingReport, setEditingReport] = useState<any>(null)
  const [dateRange, setDateRange] = useState({
    from: new Date(2025, 3, 1),
    to: new Date(2025, 3, 16),
  })
  const [currentWeekStart, setCurrentWeekStart] = useState(new Date())

  // Добавьте после других useState
  const [copyingReportId, setCopyingReportId] = useState<number | null>(null)
  const [copyDates, setCopyDates] = useState<Date[]>([])
  const [showCopyDialog, setShowCopyDialog] = useState(false)
  const [currentMonth, setCurrentMonth] = useState(new Date())
  
  // Добавляем состояние для хранения клиентов из API
  const [clients, setClients] = useState<Client[]>([])
  const [isLoadingClients, setIsLoadingClients] = useState(true)

  // Приклад списку ринків
  const markets = [
    { id: "1", name: t('markets.ukraine') },
    { id: "2", name: t('markets.europe') },
    { id: "3", name: t('markets.usa') },
    { id: "4", name: t('markets.asia') },
    { id: "5", name: t('markets.global') },
  ]

  // Список агентств
  const agencies = [
    { id: "1", name: "GroupM" },
    { id: "2", name: "MediaCom" },
    { id: "3", name: "Mindshare" },
    { id: "4", name: "Wavemaker" },
  ]

  // Список типів медіа
  const mediaTypes = [
    { id: "1", name: "OOH" },
    { id: "2", name: "Other" },
    { id: "3", name: "Print" },
    { id: "4", name: "Radio" },
    { id: "5", name: "Research" },
    { id: "6", name: "Trading" },
    { id: "7", name: "TV" },
    { id: "8", name: "TVs" },
    { id: "9", name: "Digital - all" },
    { id: "10", name: "Digital - Paid Social" },
    { id: "11", name: "Digital - Paid Search" },
    { id: "12", name: "Digital - Display" },
    { id: "13", name: "Digital - Video" },
    { id: "14", name: "Digital SP" },
    { id: "15", name: "Digital Commerce" },
    { id: "16", name: "Digital Influencers" },
    { id: "17", name: "Digital other" },
  ]

  // Список типів робіт
  const jobTypes = [
    { id: "1", name: "Strategy planning" },
    { id: "2", name: "Media plans" },
    { id: "3", name: "Campaign running" },
    { id: "4", name: "Reporting" },
    { id: "5", name: "Docs and finances" },
    { id: "6", name: "Research" },
    { id: "7", name: "Self education" },
    { id: "8", name: "Vacation" },
    { id: "9", name: "Other" },
  ]

  // Отримуємо поточний тиждень (починаючи з понеділка)
  const getWeekDays = (date: Date) => {
    const day = date.getDay()
    const diff = date.getDate() - day + (day === 0 ? -6 : 1) // Коригування для тижня, що починається з понеділка
    const monday = new Date(date)
    monday.setDate(diff)

    const weekDays = []
    for (let i = 0; i < 7; i++) {
      const currentDay = new Date(monday)
      currentDay.setDate(monday.getDate() + i)
      weekDays.push(currentDay)
    }
    return weekDays
  }

  const weekDays = getWeekDays(currentWeekStart)

  // Форматування дати
  const formatDate = (date: Date) => {
    const locale = i18n.language === 'uk' ? 'uk-UA' : 'en-US';
    return date.toLocaleDateString(locale, { day: 'numeric', month: 'long' });
  }

  // Форматування дня тижня
  const formatDayOfWeek = (date: Date) => {
    const locale = i18n.language === 'uk' ? 'uk-UA' : 'en-US';
    return date.toLocaleDateString(locale, { weekday: 'short' });
  }

  // Перевірка, чи є день поточним
  const isToday = (date: Date) => {
    const today = new Date()
    return (
      date.getDate() === today.getDate() &&
      date.getMonth() === today.getMonth() &&
      date.getFullYear() === today.getFullYear()
    )
  }

  // Перехід до попереднього тижня
  const goToPreviousWeek = () => {
    const newDate = new Date(currentWeekStart)
    newDate.setDate(newDate.getDate() - 7)
    setCurrentWeekStart(newDate)
  }

  // Перехід до наступного тижня
  const goToNextWeek = () => {
    const newDate = new Date(currentWeekStart)
    newDate.setDate(newDate.getDate() + 7)
    setCurrentWeekStart(newDate)
  }

  // Вибір дня
  const selectDay = (date: Date) => {
    setSelectedDate(date)
    setShowEntryForm(false) // Не відкриваємо форму автоматично при виборі дня
  }

  // Приклад даних звітів поточного співробітника
  const [allReports, setAllReports] = useState<Report[]>([
    {
      id: 1,
      date: "16.04.2025",
      market: t('markets.ukraine'),
      contractingAgency: "Agency One",
      client: "Acme Inc",
      projectBrand: "Ребрендинг 2025",
      media: "Веб-сайт",
      jobType: "Дизайн",
      comments: "Розробка дизайну логотипу та фірмового стилю",
      hours: 4.5,
    },
    {
      id: 2,
      date: "15.04.2025",
      market: t('markets.europe'),
      contractingAgency: "Creative Solutions",
      client: "Globex Corp",
      projectBrand: "Маркетингова кампанія Q2",
      media: "Соціальні мережі",
      jobType: "Стратегія",
      comments: "Аналіз конкурентів та цільової аудиторії",
      hours: 3.5,
    },
    {
      id: 3,
      date: "14.04.2025",
      market: t('markets.ukraine'),
      contractingAgency: "Media Group",
      client: "Tech Solutions",
      projectBrand: "Запуск нового продукту",
      media: "Відео",
      jobType: "Копірайтинг",
      comments: "Написання сценарію для рекламного ролика",
      hours: 2.0,
    },
    {
      id: 4,
      date: "16.04.2025",
      market: t('markets.global'),
      contractingAgency: "Digital Marketing Ltd",
      client: "Stark Industries",
      projectBrand: "Промо-кампанія",
      media: "Соціальні мережі",
      jobType: "Дизайн",
      comments: "Створення банерів для соціальних мереж",
      hours: 3.0,
    },
    {
      id: 5,
      date: "16.04.2025",
      market: t('markets.ukraine'),
      contractingAgency: "Brand Partners",
      client: "Wayne Enterprises",
      projectBrand: "Корпоративний сайт",
      media: "Веб-сайт",
      jobType: "Розробка",
      comments: "Верстка сторінок для корпоративного сайту",
      hours: 5.0,
    },
  ])

  // Fetch reports from the API when component mounts
  const [isLoading, setIsLoading] = useState(true);
  
  useEffect(() => {
    async function fetchReports() {
      try {
        setIsLoading(true);
        console.log('Fetching reports from API...');
        const response = await fetch('/api/reports?currentUserOnly=true');
        
        if (!response.ok) {
          console.error('API response not OK:', response.status, response.statusText);
          throw new Error(`Failed to fetch reports: ${response.status} ${response.statusText}`);
        }
        
        const data = await response.json();
        console.log('Raw API response:', data);
        
        // Transform the reports to match the format expected by the component
        // Проверяем, есть ли свойство reports в ответе
        const reportsData = data.reports || [];
        console.log('Reports data array:', reportsData);
        const formattedReports = reportsData.map((item: any) => {
          // Получаем объект отчета из данных API
          const report = item.report;
          
          // Format date from ISO to DD.MM.YYYY
          const dateObj = new Date(report.date);
          const formattedDate = dateObj.toLocaleDateString('uk-UA', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
          }).replace(/\//g, '.');
          
          return {
            id: report.id,
            date: formattedDate,
            market: report.market || '',
            contractingAgency: report.contractingAgency || '',
            client: report.client || '',
            projectBrand: report.projectBrand || '',
            media: report.media || '',
            jobType: report.jobType || '',
            comments: report.comments || '',
            hours: report.hours || 0,
          };
        });
        
        console.log('Fetched reports:', formattedReports);
        if (formattedReports.length > 0) {
          // Полностью заменяем тестовые данные реальными отчетами из API
          setAllReports(formattedReports);
          console.log('Updated allReports with API data');
        } else {
          console.warn('No reports found in the API response, keeping example data');
        }
      } catch (error) {
        console.error('Error fetching reports:', error);
        // Keep the example data if there's an error
      } finally {
        setIsLoading(false);
      }
    }
    
    fetchReports();
  }, []);

  // Загрузка клиентов из API
  useEffect(() => {
    async function fetchClients() {
      try {
        setIsLoadingClients(true)
        console.log('Fetching clients from API...')
        const response = await fetch('/api/clients')
        
        if (!response.ok) {
          console.error('API response not OK:', response.status, response.statusText)
          throw new Error(`Failed to fetch clients: ${response.status} ${response.statusText}`)
        }
        
        const data = await response.json()
        console.log('Clients data:', data)
        
        // Преобразуем данные из БД в формат, ожидаемый компонентом
        const formattedClients = data.clients.map((client: { id: number | string; name: string }) => ({
          id: client.id.toString(),
          name: client.name
        }))
        
        // Добавляем опцию "All clients", если её нет
        if (!formattedClients.some((client: Client) => client.name === "All clients")) {
          formattedClients.unshift({ id: "0", name: "All clients" })
        }
        
        setClients(formattedClients)
        setIsLoadingClients(false)
      } catch (error) {
        console.error('Error fetching clients:', error)
        // Если загрузка не удалась, используем резервный список
        const fallbackClients = [
          { id: "1", name: "All clients" },
          { id: "2", name: "NewBiz" },
          { id: "3", name: "Adidas/Reebook" },
        ]
        setClients(fallbackClients)
        setIsLoadingClients(false)
      }
    }
    
    fetchClients()
  }, []) // Пустой массив зависимостей означает, что эффект выполнится один раз при монтировании

  // Фільтрація звітів за вибраною датою
  const getFilteredReports = () => {
    if (!selectedDate) return []

    const selectedDateStr = selectedDate
      .toLocaleDateString("uk-UA", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      })
      .replace(/\//g, ".")

    console.log(`Filtering reports for selected date: ${selectedDateStr}`);
    
    // Ensure allReports is an array before filtering
    if (!Array.isArray(allReports)) {
      console.warn('allReports is not an array:', allReports);
      return [];
    }

    const filtered = allReports.filter((report) => {
      if (!report || !report.date) return false;
      
      // Преобразуем формат даты отчета для сравнения
      let reportDate;
      if (report.date.includes('.')) {
        reportDate = report.date;
      } else {
        const [day, month, year] = report.date.split(".")
        reportDate = `${day}.${month}.${year}`
      }
      
      const match = reportDate === selectedDateStr;
      return match;
    });
    
    console.log(`Found ${filtered.length} reports for ${selectedDateStr}:`, filtered);
    return filtered;
  }

  // Проверка наличия записей на конкретный день
  const hasDayRecords = (date: Date) => {
    const dateStr = date
      .toLocaleDateString("uk-UA", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      })
      .replace(/\//g, ".")

    console.log(`Checking records for date: ${dateStr}, allReports:`, allReports);
    
    // Check if allReports is defined and is an array before calling .some()
    return Array.isArray(allReports) && allReports.some((report) => {
      if (!report || !report.date) return false;
      
      // Преобразуем формат даты отчета для сравнения
      let reportDate;
      if (report.date.includes('.')) {
        reportDate = report.date;
      } else {
        const [day, month, year] = report.date.split(".")
        reportDate = `${day}.${month}.${year}`
      }
      
      const match = reportDate === dateStr;
      if (match) {
        console.log(`Found report for ${dateStr}:`, report);
      }
      return match;
    })
  }

  const filteredReports = getFilteredReports()

  // Розрахунок статистики для вибраної дати
  const calculateStats = () => {
    if (filteredReports.length === 0) {
      return {
        totalHours: 0,
        reportsCount: 0,
        averageHours: 0,
      }
    }

    const totalHours = filteredReports.reduce((sum, report) => sum + report.hours, 0)
    return {
      totalHours,
      reportsCount: filteredReports.length,
      averageHours: totalHours / filteredReports.length,
    }
  }

  const stats = calculateStats()

  // Функция для копирования записи на другую дату
  const handleCopyReport = (reportId: number) => {
    setCopyingReportId(reportId)
    setCopyDates([])
    setShowCopyDialog(true)
    setCurrentMonth(new Date()) // Reset to current month when opening dialog
  }

  // Функция для выполнения копирования
  const executeCopy = () => {
    if (!copyingReportId || !copyDates.length) return

    const reportToCopy = allReports.find((report) => report.id === copyingReportId)
    if (!reportToCopy) return

    // Создаем новые отчеты для каждой выбранной даты
    const newReports = copyDates.map((date, index) => {
      // Форматируем дату для нового отчета
      const formattedDate = date
        .toLocaleDateString("uk-UA", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        })
        .replace(/\//g, ".")

      // Создаем новый отчет на основе копируемого, сохраняя все поля
      return {
        ...reportToCopy,
        id: allReports.length + index + 1,
        date: formattedDate,
        market: reportToCopy.market,
        contractingAgency: reportToCopy.contractingAgency,
        client: reportToCopy.client,
        projectBrand: reportToCopy.projectBrand,
        media: reportToCopy.media,
        jobType: reportToCopy.jobType,
        comments: reportToCopy.comments,
        hours: reportToCopy.hours,
      }
    })

    // Сохраняем новые отчеты в базу данных
    const saveReportsToDB = async () => {
      try {
        setIsLoading(true);
        
        // Сохраняем каждый новый отчет в базу данных
        for (const newReport of newReports) {
          // Получаем компании для ассоциации с отчетом
          const companiesToAssociate = [];
          if (newReport.client) companiesToAssociate.push(newReport.client);
          if (newReport.contractingAgency && newReport.contractingAgency !== newReport.client) {
            companiesToAssociate.push(newReport.contractingAgency);
          }
          
          // Преобразуем дату в формат YYYY-MM-DD для БД
          const [day, month, year] = newReport.date.split('.');
          const dateForDB = `${year}-${month}-${day}`;
          
          const response = await fetch('/api/reports', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              ...newReport,
              date: dateForDB,
              companies: companiesToAssociate,
            }),
          });
          
          if (!response.ok) {
            throw new Error(`Failed to save copied report: ${response.statusText}`);
          }
        }
        
        // После успешного сохранения обновляем локальное состояние
        setAllReports([...allReports, ...newReports]);
        
      } catch (error) {
        console.error('Error saving copied reports:', error);
        alert('Не удалось сохранить скопированные записи. Пожалуйста, попробуйте снова.');
      } finally {
        setIsLoading(false);
      }
    };
    
    // Запустить сохранение в базу данных
    saveReportsToDB();

    // Закрываем диалог
    setShowCopyDialog(false)
    setCopyingReportId(null)
    setCopyDates([])

    // Уведомляем пользователя
    alert(`Запись скопирована на ${copyDates.length} ${copyDates.length === 1 ? "дату" : "дат"}`)
  }

  // Функція для завантаження звіту
  const downloadReport = (reportId: number) => {
    console.log(`Завантаження звіту ID: ${reportId}`)
    alert(`Звіт ID: ${reportId} завантажується у форматі Excel...`)
  }

  // Функция добавления новой записи с очисткой формы
  const handleAddNewEntry = () => {
    // Сначала закрываем форму редактирования если она открыта
    if (showEntryForm) {
      // Закрываем форму и очищаем данные редактирования одновременно
      setShowEntryForm(false);
      setEditingReport(null);
      
      // Затем, в следующем цикле рендеринга, открываем форму снова
      // Используем requestAnimationFrame вместо setTimeout
      requestAnimationFrame(() => {
        setShowEntryForm(true);
      });
    } else {
      // Если форма не открыта, просто очищаем старые данные и открываем форму
      setEditingReport(null);
      setShowEntryForm(true);
    }
  };

  // Функция редактирования записи
  const handleEditReport = (report: any) => {
    // Parse the date from the report
    const [day, month, year] = report.date.split(".")
    const reportDate = new Date(`${year}-${month}-${day}`)
    setSelectedDate(reportDate)
    
    // Преобразуем названия в ID для селекторов
    const findIdByName = (items: Array<{id: string, name: string}>, name: string) => {
      const item = items.find(item => item.name === name)
      return item ? item.id : name
    }
    
    // Convert hours to minutes for the form
    const hoursToMinutes = Math.round(report.hours * 60);
    
    const reportForEdit = {
      ...report,
      // Convert values to IDs for the form dropdowns
      market: findIdByName(markets, report.market),
      contractingAgency: findIdByName(agencies, report.contractingAgency),
      client: findIdByName(clients, report.client),
      media: findIdByName(mediaTypes, report.media),
      jobType: findIdByName(jobTypes, report.jobType),
      // Update hours to minutes for the form
      hours: hoursToMinutes.toString(),
    }
    
    setEditingReport(reportForEdit)
    setShowEntryForm(true)
  }

  // Delete a report
  const handleDeleteReport = (reportId: number) => {
    if (confirm('Ви впевнені, що хочете видалити цей запис?')) {
      // Delete the report from the API
      const deleteReport = async () => {
        try {
          console.log(`Attempting to delete report with ID: ${reportId}`);
          const response = await fetch(`/api/reports?id=${reportId}`, {
            method: 'DELETE',
          });
          
          const data = await response.json();
          
          if (!response.ok) {
            console.error('Error response from server:', data);
            throw new Error(data.error || 'Failed to delete report');
          }
          
          console.log('Report successfully deleted', data);
          
          // Update local state after successful deletion
          setAllReports(allReports.filter((report) => report.id !== reportId));
          
          // Recalculate stats
          calculateStats();
        } catch (error) {
          console.error('Error deleting report:', error);
          alert(`Failed to delete the report: ${error instanceof Error ? error.message : 'Unknown error'}`);
        }
      };
      
      deleteReport();
    }
  }

  // Функція для завантаження всіх звітів
  const downloadAllReports = () => {
    console.log("Завантаження всіх звітів")
    alert("Всі звіти завантажуються у форматі Excel...")
  }

  // Calendar functions for copy dialog
  const getDaysInMonth = (year: number, month: number) => {
    return new Date(year, month + 1, 0).getDate()
  }

  const getFirstDayOfMonth = (year: number, month: number) => {
    return new Date(year, month, 1).getDay()
  }

  const generateCalendarDays = (year: number, month: number) => {
    const daysInMonth = getDaysInMonth(year, month)
    const firstDay = getFirstDayOfMonth(year, month)

    // Get days from previous month to fill the first week
    const daysFromPrevMonth = firstDay === 0 ? 6 : firstDay - 1
    const prevMonth = month === 0 ? 11 : month - 1
    const prevMonthYear = month === 0 ? year - 1 : year
    const daysInPrevMonth = getDaysInMonth(prevMonthYear, prevMonth)

    const prevMonthDays = []
    for (let i = daysInPrevMonth - daysFromPrevMonth + 1; i <= daysInPrevMonth; i++) {
      prevMonthDays.push({
        day: i,
        month: prevMonth,
        year: prevMonthYear,
        isCurrentMonth: false,
      })
    }

    // Current month days
    const currentMonthDays = []
    for (let i = 1; i <= daysInMonth; i++) {
      currentMonthDays.push({
        day: i,
        month,
        year,
        isCurrentMonth: true,
      })
    }

    // Next month days to fill the last week
    const totalDaysShown = Math.ceil((daysFromPrevMonth + daysInMonth) / 7) * 7
    const daysFromNextMonth = totalDaysShown - (daysFromPrevMonth + daysInMonth)
    const nextMonth = month === 11 ? 0 : month + 1
    const nextMonthYear = month === 11 ? year + 1 : year

    const nextMonthDays = []
    for (let i = 1; i <= daysFromNextMonth; i++) {
      nextMonthDays.push({
        day: i,
        month: nextMonth,
        year: nextMonthYear,
        isCurrentMonth: false,
      })
    }

    return [...prevMonthDays, ...currentMonthDays, ...nextMonthDays]
  }

  const isDateSelected = (day: number, month: number, year: number) => {
    return copyDates.some((date) => date.getDate() === day && date.getMonth() === month && date.getFullYear() === year)
  }

  const toggleDateSelection = (day: number, month: number, year: number) => {
    const date = new Date(year, month, day)

    if (isDateSelected(day, month, year)) {
      setCopyDates(
        copyDates.filter((d) => !(d.getDate() === day && d.getMonth() === month && d.getFullYear() === year)),
      )
    } else {
      setCopyDates([...copyDates, date])
    }
  }

  const goToPreviousMonth = () => {
    const newMonth = new Date(currentMonth)
    newMonth.setMonth(newMonth.getMonth() - 1)
    setCurrentMonth(newMonth)
  }

  const goToNextMonth = () => {
    const newMonth = new Date(currentMonth)
    newMonth.setMonth(newMonth.getMonth() + 1)
    setCurrentMonth(newMonth)
  }

  const formattedMonthName = currentMonth.toLocaleDateString(
    i18n.language === 'uk' ? 'uk-UA' : 'en-US',
    { month: 'long', year: 'numeric' }
  )
  const calendarDays = generateCalendarDays(currentMonth.getFullYear(), currentMonth.getMonth())
  const weekDayNames = ["Нд", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"]

  // Add a helper function to get client name from client ID
  const getClientName = (client: Report['client']): string => {
    // Handle client object with name property
    if (client && typeof client === 'object' && 'name' in client) {
      return client.name;
    }
    
    // Handle client ID (string or number)
    if (typeof client === 'number' || typeof client === 'string') {
      const clientId = client.toString();
      const foundClient = clients.find((c) => c.id === clientId);
      return foundClient ? foundClient.name : clientId;
    }
    
    // Fallback for other cases
    return client ? String(client) : '';
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('calendar.title')}</h1>
        <p className="text-gray-500">{t('calendar.description')}</p>
      </div>

      <Card>
        <CardHeader className="pb-0">
          <div className="flex justify-between items-center">
            <CardTitle>
              {t('calendar.week', { from: weekDays[0].toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long' }), to: weekDays[6].toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long' }) })}
            </CardTitle>
            <div className="flex gap-2">
              <Button variant="outline" size="icon" onClick={goToPreviousWeek}>
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <Button variant="outline" size="icon" onClick={goToNextWeek}>
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-7 gap-1 mt-4">
            {weekDays.map((day, index) => (
              <Button
                key={index}
                variant={
                  isToday(day)
                    ? "outline"
                    : selectedDate &&
                        selectedDate.getDate() === day.getDate() &&
                        selectedDate.getMonth() === day.getMonth() &&
                        selectedDate.getFullYear() === day.getFullYear()
                      ? "secondary"
                      : "outline"
                }
                className={`h-auto flex flex-col py-2 ${
                  isToday(day)
                    ? "border-primary bg-[rgb(15,40,84)] text-white hover:bg-[rgb(15,40,84)] hover:text-white"
                    : hasDayRecords(day)
                      ? "bg-gray-100 hover:bg-gray-200"
                      : "bg-gray-200 hover:bg-gray-300"
                }`}
                onClick={() => selectDay(day)}
              >
                <span className="text-xs font-medium">{t(`calendar.weekdayShort.${index}`)}</span>
                <span className="text-lg font-bold">{day.getDate()}</span>
                <span className="text-xs">{day.toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { month: 'short' })}</span>
              </Button>
            ))}
          </div>
        </CardContent>
      </Card>

      {selectedDate && (
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-bold">
            {t('calendar.entriesForDate', { date: selectedDate.toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long', year: 'numeric' }) })}
          </h2>
          <Button onClick={handleAddNewEntry} className="gap-2">
            <Plus className="h-4 w-4" />
            {t('calendar.addEntry')}
          </Button>
        </div>
      )}

      {showEntryForm && selectedDate && (
        <Card>
          <CardHeader>
            <CardTitle>
              {editingReport ? t('calendar.editEntry') : t('calendar.newEntry')} {t('calendar.onDate', { date: formatDate(selectedDate) })}
            </CardTitle>
            <CardDescription>{t('calendar.specifyDetails')}</CardDescription>
          </CardHeader>
          <CardContent>
            <DayEntryForm
              key={editingReport ? `edit-${editingReport.id}` : 'new-entry'}
              date={selectedDate}
              fields={{
                market: true,
                contractingAgency: true,
                client: true,
                projectBrand: true,
                media: true,
                jobType: true,
                comments: true,
                hours: true,
              }}
              compact={true}
              initialValues={editingReport}
              filterStartsWith={true}
              showInputInField={true}
              onClose={() => {
                // Закрываем форму и очищаем данные одновременно
                setShowEntryForm(false);
                setEditingReport(null);
              }}
              onSave={(data) => {
                console.log("Збережено:", data)

                // Convert IDs back to text values
                const marketName = markets.find((m) => m.id === data.market)?.name || data.market
                const agencyName = agencies.find((a) => a.id === data.contractingAgency)?.name || data.contractingAgency
                const clientName = clients.find((c) => c.id === data.client)?.name || data.client
                const mediaName = mediaTypes.find((m) => m.id === data.media)?.name || data.media
                const jobTypeName = jobTypes.find((j) => j.id === data.jobType)?.name || data.jobType

                // Подготовка данных отчета
                const updatedData = {
                  ...data,
                  id: editingReport?.id || Date.now(), // Сохраняем ID или создаем новый
                  date: selectedDate.toLocaleDateString("uk-UA", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric",
                  }).replace(/\//g, "."),
                  market: marketName,
                  contractingAgency: agencyName,
                  client: clientName,
                  media: mediaName,
                  jobType: jobTypeName,
                  hours: Number(data.hours) / 60 || 0, // Convert minutes to hours for the database
                }

                // Save the report to the server
                const saveReport = async () => {
                  try {
                    const apiUrl = '/api/reports';
                    const method = editingReport ? 'PUT' : 'POST';
                    
                    // Fetch companies to associate with the report
                    const companiesToAssociate = [];
                    if (clientName) companiesToAssociate.push(clientName);
                    if (agencyName && agencyName !== clientName) companiesToAssociate.push(agencyName);
                    
                    // Format the date properly for the database (YYYY-MM-DD)
                    const dateForDB = selectedDate.toISOString().split('T')[0];
                    
                    const response = await fetch(apiUrl, {
                      method,
                      headers: {
                        'Content-Type': 'application/json',
                      },
                      body: JSON.stringify({
                        ...updatedData,
                        id: editingReport?.id, // Only include ID for updates
                        date: dateForDB, // Use ISO format for database
                        hours: Number(data.hours) / 60 || 0, // Convert minutes to hours for the database
                        companies: companiesToAssociate, // Include companies to associate
                      }),
                    });
                    
                    if (!response.ok) {
                      throw new Error('Failed to save report');
                    }
                    
                    const result = await response.json();
                    console.log('Report saved:', result);
                    
                    // Update local state after successful save to server
                    // Update the report if we're editing an existing one
                    if (editingReport) {
                      setAllReports(
                        allReports.map((report) =>
                          report.id === editingReport.id ? { ...updatedData } : report
                        ),
                      )
                    } else {
                      // Add new report
                      setAllReports([...allReports, updatedData])
                    }
                    
                    // Close the form and clear editing state
                    setShowEntryForm(false);
                    setEditingReport(null);
                  } catch (error) {
                    console.error('Error saving report:', error);
                    alert('Failed to save the report. Please try again.');
                  }
                };
                
                saveReport();
              }}
            />
          </CardContent>
        </Card>
      )}

      {/* Секция отчетов - показывается только когда выбрана дата */}
      {selectedDate && (
        <div className="mt-8">
          {/* Статистика по выбранной дате */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-base">{t('calendar.totalHours')}</CardTitle>
                <CardDescription>
                  {t('calendar.forDate', { date: selectedDate.toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long' }) })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stats.totalHours.toFixed(1)} {t('calendar.hours')}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-base">{t('calendar.entriesCount')}</CardTitle>
                <CardDescription>
                  {t('calendar.forDate', { date: selectedDate.toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long' }) })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stats.reportsCount}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-base">{t('calendar.avgTimePerEntry')}</CardTitle>
                <CardDescription>
                  {t('calendar.forDate', { date: selectedDate.toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long' }) })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {stats.averageHours > 0 ? `${stats.averageHours.toFixed(1)} ${t('calendar.hours')}` : "—"}
                </div>
              </CardContent>
            </Card>
          </div>

          {filteredReports.length > 0 ? (
            <Card>
              <CardHeader>
                <CardTitle>{t('calendar.summaryInfo')}</CardTitle>
                <CardDescription>
                  {t('calendar.reportsForDate', { date: selectedDate.toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long', year: 'numeric' }) })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>{t('calendar.market')}</TableHead>
                        <TableHead>{t('calendar.agency')}</TableHead>
                        <TableHead>{t('calendar.client')}</TableHead>
                        <TableHead>{t('calendar.projectBrand')}</TableHead>
                        <TableHead>{t('calendar.media')}</TableHead>
                        <TableHead>{t('calendar.jobType')}</TableHead>
                        <TableHead>{t('calendar.comments')}</TableHead>
                        <TableHead>{t('calendar.hours')}</TableHead>
                        <TableHead>{t('calendar.actions')}</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredReports.map((report) => (
                        <TableRow key={report.id}>
                          <TableCell>{report.market}</TableCell>
                          <TableCell>{report.contractingAgency}</TableCell>
                          <TableCell>{getClientName(report.client)}</TableCell>
                          <TableCell>{report.projectBrand}</TableCell>
                          <TableCell>{report.media}</TableCell>
                          <TableCell>{report.jobType}</TableCell>
                          <TableCell className="max-w-[200px] truncate" title={report.comments}>
                            {report.comments}
                          </TableCell>
                          <TableCell>{Number(report.hours).toFixed(1)}</TableCell>
                          <TableCell>
                            <div className="flex gap-2">
                              <Button variant="ghost" size="icon" onClick={() => handleEditReport(report)}>
                                <Pencil className="h-4 w-4" />
                                <span className="sr-only">{t('calendar.edit')}</span>
                              </Button>
                              <Button variant="ghost" size="icon" onClick={() => handleDeleteReport(report.id)}>
                                <Trash className="h-4 w-4" />
                                <span className="sr-only">{t('calendar.delete')}</span>
                              </Button>
                              <Button variant="ghost" size="icon" onClick={() => handleCopyReport(report.id)}>
                                <Copy className="h-4 w-4" />
                                <span className="sr-only">{t('calendar.copy')}</span>
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-8">
                <p className="text-lg font-medium mb-2">{t('calendar.noEntriesForDate')}</p>
                <p className="text-gray-500 mb-4">
                  {t('calendar.noEntriesForDateDesc', { date: selectedDate.toLocaleDateString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: 'numeric', month: 'long', year: 'numeric' }) })}
                </p>
                <Button onClick={handleAddNewEntry} className="gap-2">
                  <Plus className="h-4 w-4" />
                  {t('calendar.addFirstEntry')}
                </Button>
              </CardContent>
            </Card>
          )}
        </div>
      )}

      {/* Диалог выбора даты для копирования */}
      <Dialog open={showCopyDialog} onOpenChange={setShowCopyDialog}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>{t('calendar.copyEntry')}</DialogTitle>
            <DialogDescription>{t('calendar.selectDateToCopy')}</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="copy-dates">{t('calendar.selectDatesToCopy')}</Label>
              <div className="border rounded-md p-2">
                <div className="text-center mb-2">
                  <div className="flex justify-between items-center mb-2">
                    <Button variant="outline" size="icon" onClick={goToPreviousMonth}>
                      <ChevronLeft className="h-4 w-4" />
                    </Button>
                    <div className="font-medium">{formattedMonthName}</div>
                    <Button variant="outline" size="icon" onClick={goToNextMonth}>
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </div>
                </div>

                <div className="grid grid-cols-7 gap-1 text-center mb-1">
                  {weekDayNames.map((day, i) => (
                    <div key={i} className="text-xs font-medium py-1">
                      {t(`calendar.weekdayShort.${i}`)}
                    </div>
                  ))}
                </div>

                <div className="grid grid-cols-7 gap-1">
                  {calendarDays.map((day, i) => (
                    <Button
                      key={i}
                      variant={isDateSelected(day.day, day.month, day.year) ? "default" : "outline"}
                      className={`h-9 p-0 ${!day.isCurrentMonth ? "text-gray-400" : ""}`}
                      onClick={() => toggleDateSelection(day.day, day.month, day.year)}
                    >
                      {day.day}
                    </Button>
                  ))}
                </div>
              </div>
              <p className="text-sm text-muted-foreground">
                {copyDates?.length > 0
                  ? t('calendar.selectedDates', { count: copyDates.length })
                  : t('calendar.selectDatesToCopyHint')}
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCopyDialog(false)}>
              {t('calendar.cancel')}
            </Button>
            <Button onClick={executeCopy} disabled={!copyDates.length}>
              {t('calendar.copy')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
