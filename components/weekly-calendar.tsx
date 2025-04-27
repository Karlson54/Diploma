"use client"

import { useState } from "react"
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

export function WeeklyCalendar() {
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

  // Add these arrays right after the useState declarations
  // Список клієнтів
  const clients = [
    { id: "1", name: "All clients" },
    { id: "2", name: "NewBiz" },
    { id: "3", name: "Adidas/Reebook" },
    { id: "4", name: "Adobe" },
    // ... other clients
    { id: "127", name: "CCHBC" },
  ]

  // Приклад списку ринків
  const markets = [
    { id: "1", name: "Україна" },
    { id: "2", name: "Європа" },
    { id: "3", name: "США" },
    { id: "4", name: "Азія" },
    { id: "5", name: "Глобальний" },
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
    return date.toLocaleDateString("uk-UA", { day: "numeric", month: "long" })
  }

  // Форматування дня тижня
  const formatDayOfWeek = (date: Date) => {
    return date.toLocaleDateString("uk-UA", { weekday: "short" })
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
  const [allReports, setAllReports] = useState([
    {
      id: 1,
      date: "16.04.2025",
      market: "Україна",
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
      market: "Європа",
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
      market: "Україна",
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
      market: "Глобальний",
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
      market: "Україна",
      contractingAgency: "Brand Partners",
      client: "Wayne Enterprises",
      projectBrand: "Корпоративний сайт",
      media: "Веб-сайт",
      jobType: "Розробка",
      comments: "Верстка сторінок для корпоративного сайту",
      hours: 5.0,
    },
  ])

  // Фільтрація звітів за вибраною датою
  const getFilteredReports = () => {
    if (!selectedDate) return []

    const selectedDateStr = selectedDate
      .toLocaleDateString("uk-UA", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      })
      .replace(/\./g, ".")

    return allReports.filter((report) => {
      const [day, month, year] = report.date.split(".")
      const reportDate = `${day}.${month}.${year}`
      return reportDate === selectedDateStr
    })
  }

  // Проверка наличия записей на конкретный день
  const hasDayRecords = (date: Date) => {
    const dateStr = date
      .toLocaleDateString("uk-UA", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      })
      .replace(/\./g, ".")

    return allReports.some((report) => {
      const [day, month, year] = report.date.split(".")
      const reportDate = `${day}.${month}.${year}`
      return reportDate === dateStr
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

    // Добавляем новые отчеты в список
    setAllReports([...allReports, ...newReports])

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
    // Find the corresponding IDs for dropdown fields
    const marketId = markets.find((m) => m.name === report.market)?.id || ""
    const agencyId = agencies.find((a) => a.name === report.contractingAgency)?.id || ""
    const clientId = clients.find((c) => c.name === report.client)?.id || ""
    const mediaId = mediaTypes.find((m) => m.name === report.media)?.id || ""
    const jobTypeId = jobTypes.find((j) => j.name === report.jobType)?.id || ""

    // Convert hours to minutes (e.g., 1.5 hours = 90 minutes)
    const minutesValue = Math.round(report.hours * 60).toString()

    // Создаем объект с данными для редактирования, включая все необходимые поля
    const editData = {
      ...report,
      market: marketId || report.market, // Используем ID если найден, иначе оставляем имя
      contractingAgency: agencyId || report.contractingAgency,
      client: clientId || report.client,
      media: mediaId || report.media,
      jobType: jobTypeId || report.jobType,
      hours: minutesValue, // Convert hours to minutes
    }

    console.log("Данные для редактирования:", editData)
    
    // Устанавливаем данные для редактирования
    setEditingReport(editData);
    
    // Открываем форму в режиме редактирования
    setShowEntryForm(true);
    console.log(`Редагування запису ID: ${report.id}`)
  }

  // Функція для видалення запису
  const handleDeleteReport = (reportId: number) => {
    if (confirm(`Ви впевнені, що хочете видалити запис ID: ${reportId}?`)) {
      setAllReports(allReports.filter((report) => report.id !== reportId))
      console.log(`Видалення запису ID: ${reportId}`)
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

  const monthName = currentMonth.toLocaleDateString("uk-UA", { month: "long", year: "numeric" })
  const calendarDays = generateCalendarDays(currentMonth.getFullYear(), currentMonth.getMonth())
  const weekDayNames = ["Нд", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Календар робочого часу</h1>
        <p className="text-gray-500">Оберіть день для внесення або перегляду записів</p>
      </div>

      <Card>
        <CardHeader className="pb-0">
          <div className="flex justify-between items-center">
            <CardTitle>
              Тиждень {weekDays[0].toLocaleDateString("uk-UA", { day: "numeric", month: "long" })} -{" "}
              {weekDays[6].toLocaleDateString("uk-UA", { day: "numeric", month: "long" })}
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
                    ? "outline" // змінено з "default" на "outline", щоб застосувати власний стиль
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
                <span className="text-xs font-medium">{formatDayOfWeek(day)}</span>
                <span className="text-lg font-bold">{day.getDate()}</span>
                <span className="text-xs">{day.toLocaleDateString("uk-UA", { month: "short" })}</span>
              </Button>
            ))}
          </div>
        </CardContent>
      </Card>

      {selectedDate && (
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-bold">
            Записи за {selectedDate.toLocaleDateString("uk-UA", { day: "numeric", month: "long", year: "numeric" })}
          </h2>
          <Button onClick={handleAddNewEntry} className="gap-2">
            <Plus className="h-4 w-4" />
            Додати запис
          </Button>
        </div>
      )}

      {showEntryForm && selectedDate && (
        <Card>
          <CardHeader>
            <CardTitle>
              {editingReport ? "Редагування запису" : "Запис робочого часу"} на {formatDate(selectedDate)}
            </CardTitle>
            <CardDescription>Вкажіть компанію, опис роботи та витрачений час</CardDescription>
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
                  hours: Number(data.hours) / 60 || 0, // Правильное преобразование минут в часы
                }

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

                // Закрываем форму и очищаем данные одновременно
                setShowEntryForm(false);
                setEditingReport(null);
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
                <CardTitle className="text-base">Загальна кількість годин</CardTitle>
                <CardDescription>
                  За {selectedDate.toLocaleDateString("uk-UA", { day: "numeric", month: "long" })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stats.totalHours} годин</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-base">Кількість записів</CardTitle>
                <CardDescription>
                  За {selectedDate.toLocaleDateString("uk-UA", { day: "numeric", month: "long" })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stats.reportsCount}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-base">Середній час на запис</CardTitle>
                <CardDescription>
                  За {selectedDate.toLocaleDateString("uk-UA", { day: "numeric", month: "long" })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {stats.averageHours > 0 ? `${stats.averageHours.toFixed(1)} годин` : "—"}
                </div>
              </CardContent>
            </Card>
          </div>

          {filteredReports.length > 0 ? (
            <Card>
              <CardHeader>
                <CardTitle>Зведена інформація</CardTitle>
                <CardDescription>
                  Звіти за{" "}
                  {selectedDate.toLocaleDateString("uk-UA", { day: "numeric", month: "long", year: "numeric" })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Market</TableHead>
                        <TableHead>Agency/Unit</TableHead>
                        <TableHead>Client</TableHead>
                        <TableHead>Project/Brand</TableHead>
                        <TableHead>Media</TableHead>
                        <TableHead>Job Type</TableHead>
                        <TableHead>Comments</TableHead>
                        <TableHead>Години</TableHead>
                        <TableHead>Дії</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredReports.map((report) => (
                        <TableRow key={report.id}>
                          <TableCell>{report.market}</TableCell>
                          <TableCell>{report.contractingAgency}</TableCell>
                          <TableCell>{report.client}</TableCell>
                          <TableCell>{report.projectBrand}</TableCell>
                          <TableCell>{report.media}</TableCell>
                          <TableCell>{report.jobType}</TableCell>
                          <TableCell className="max-w-[200px] truncate" title={report.comments}>
                            {report.comments}
                          </TableCell>
                          <TableCell>{report.hours}</TableCell>
                          <TableCell>
                            <div className="flex gap-2">
                              <Button variant="ghost" size="icon" onClick={() => handleEditReport(report)}>
                                <Pencil className="h-4 w-4" />
                                <span className="sr-only">Редагувати</span>
                              </Button>
                              <Button variant="ghost" size="icon" onClick={() => handleDeleteReport(report.id)}>
                                <Trash className="h-4 w-4" />
                                <span className="sr-only">Видалити</span>
                              </Button>
                              <Button variant="ghost" size="icon" onClick={() => handleCopyReport(report.id)}>
                                <Copy className="h-4 w-4" />
                                <span className="sr-only">Копіювати</span>
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
                <p className="text-lg font-medium mb-2">Немає записів за цю дату</p>
                <p className="text-gray-500 mb-4">
                  За {selectedDate.toLocaleDateString("uk-UA", { day: "numeric", month: "long", year: "numeric" })} ще
                  не було додано жодного запису
                </p>
                <Button onClick={handleAddNewEntry} className="gap-2">
                  <Plus className="h-4 w-4" />
                  Додати перший запис
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
            <DialogTitle>Копіювати запис</DialogTitle>
            <DialogDescription>Оберіть дату, на яку потрібно скопіювати запис</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="copy-dates">Оберіть дати для копіювання</Label>
              <div className="border rounded-md p-2">
                <div className="text-center mb-2">
                  <div className="flex justify-between items-center mb-2">
                    <Button variant="outline" size="icon" onClick={goToPreviousMonth}>
                      <ChevronLeft className="h-4 w-4" />
                    </Button>
                    <div className="font-medium">{monthName}</div>
                    <Button variant="outline" size="icon" onClick={goToNextMonth}>
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </div>
                </div>

                <div className="grid grid-cols-7 gap-1 text-center mb-1">
                  {weekDayNames.map((day, i) => (
                    <div key={i} className="text-xs font-medium py-1">
                      {day}
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
                  ? `Обрано ${copyDates.length} ${copyDates.length === 1 ? "дату" : copyDates.length < 5 ? "дати" : "дат"}`
                  : "Оберіть дати для копіювання запису"}
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCopyDialog(false)}>
              Скасувати
            </Button>
            <Button onClick={executeCopy} disabled={!copyDates.length}>
              Копіювати
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
