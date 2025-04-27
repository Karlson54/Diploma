"use client"

import { useState, useEffect } from "react"
import { BarChart3, Clock, Calendar, ArrowUpRight, ArrowDownRight, Play, Pause, Plus } from "lucide-react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { ProjectTable } from "@/components/project-table"
import { TimeEntryForm } from "@/components/time-entry-form"
import { companyQueries, reportQueries } from "@/db/queries"

export function TimeTrackingDashboard() {
  const [isTracking, setIsTracking] = useState(false)
  const [showTimeEntryForm, setShowTimeEntryForm] = useState(false)
  const [dashboardData, setDashboardData] = useState({
    todayHours: 0,
    weeklyHours: 0,
    activeProjects: 0,
    overdueProjects: 0,
    efficiency: 0,
    weeklyChange: 0,
    todayDate: new Date().toLocaleDateString('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    }),
    weekStart: new Date(),
    weekEnd: new Date()
  })
  const [loading, setLoading] = useState(true)

  // Fetch dashboard data from database
  useEffect(() => {
    async function fetchDashboardData() {
      try {
        // Get all reports
        const allReports = await reportQueries.getAllWithEmployee()
        
        // Get companies (projects)
        const companies = await companyQueries.getAll()
        
        // Calculate week start and end dates
        const today = new Date()
        const dayOfWeek = today.getDay()
        const weekStart = new Date(today)
        weekStart.setDate(today.getDate() - dayOfWeek + (dayOfWeek === 0 ? -6 : 1)) // Start from Monday
        weekStart.setHours(0, 0, 0, 0)
        
        const weekEnd = new Date(weekStart)
        weekEnd.setDate(weekStart.getDate() + 6) // End on Sunday
        weekEnd.setHours(23, 59, 59, 999)
        
        // Format dates for display
        const weekStartFormatted = weekStart.toLocaleDateString('ru-RU', { day: 'numeric', month: 'numeric' })
        const weekEndFormatted = weekEnd.toLocaleDateString('ru-RU', { day: 'numeric', month: 'numeric' })
        
        // Filter today's reports
        const todayStart = new Date()
        todayStart.setHours(0, 0, 0, 0)
        const todayEnd = new Date()
        todayEnd.setHours(23, 59, 59, 999)
        
        const todayReports = allReports.filter(report => {
          const reportDate = new Date(report.report.date)
          return reportDate >= todayStart && reportDate <= todayEnd
        })
        
        // Filter weekly reports
        const weeklyReports = allReports.filter(report => {
          const reportDate = new Date(report.report.date)
          return reportDate >= weekStart && reportDate <= weekEnd
        })
        
        // Calculate hours
        const todayHours = todayReports.reduce((sum, report) => sum + report.report.hours, 0)
        const weeklyHours = weeklyReports.reduce((sum, report) => sum + report.report.hours, 0)
        
        // Previous week for comparison
        const prevWeekStart = new Date(weekStart)
        prevWeekStart.setDate(prevWeekStart.getDate() - 7)
        const prevWeekEnd = new Date(weekEnd)
        prevWeekEnd.setDate(prevWeekEnd.getDate() - 7)
        
        const prevWeekReports = allReports.filter(report => {
          const reportDate = new Date(report.report.date)
          return reportDate >= prevWeekStart && reportDate <= prevWeekEnd
        })
        
        const prevWeekHours = prevWeekReports.reduce((sum, report) => sum + report.report.hours, 0)
        
        // Calculate weekly change percentage
        const weeklyChange = prevWeekHours > 0 
          ? Math.round(((weeklyHours - prevWeekHours) / prevWeekHours) * 100) 
          : 0
        
        // Calculate efficiency (billable vs non-billable)
        const totalHours = allReports.reduce((sum, report) => sum + report.report.hours, 0)
        const billableHours = allReports
          .filter(report => report.report.client || report.report.contractingAgency)
          .reduce((sum, report) => sum + report.report.hours, 0)
        
        const efficiency = totalHours > 0 ? Math.round((billableHours / totalHours) * 100) : 0
        
        // Active and overdue projects
        const activeProjects = companies.length
        const overdueProjects = companies.filter(company => {
          const companyReports = allReports.filter(r => 
            r.report.client?.includes(company.name) || 
            r.report.contractingAgency?.includes(company.name)
          )
          
          const hoursSpent = companyReports.reduce((sum, r) => sum + r.report.hours, 0)
          const allocatedHours = company.projects ? company.projects * 40 : 80
          
          return hoursSpent > allocatedHours * 0.9
        }).length
        
        setDashboardData({
          todayHours,
          weeklyHours,
          activeProjects,
          overdueProjects,
          efficiency,
          weeklyChange,
          todayDate: today.toLocaleDateString('ru-RU', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
          }),
          weekStart: weekStart,
          weekEnd: weekEnd
        })
      } catch (error) {
        console.error("Error fetching dashboard data:", error)
      } finally {
        setLoading(false)
      }
    }
    
    fetchDashboardData()
  }, [])
  
  // Format week display
  const weekDisplay = `${dashboardData.weekStart.getDate()}-${dashboardData.weekEnd.getDate()} ${
    dashboardData.weekStart.toLocaleDateString('ru-RU', { month: 'long' })
  }`

  if (loading) {
    return <div>Загрузка данных...</div>
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Дашборд</h1>
          <p className="text-gray-500">Обзор учета рабочего времени и проектов</p>
        </div>
        <div className="flex gap-2">
          <Button
            variant={isTracking ? "destructive" : "default"}
            onClick={() => setIsTracking(!isTracking)}
            className="gap-2"
          >
            {isTracking ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
            {isTracking ? "Остановить" : "Начать учет"}
          </Button>
          <Button variant="outline" onClick={() => setShowTimeEntryForm(true)} className="gap-2">
            <Plus className="h-4 w-4" />
            Добавить время
          </Button>
        </div>
      </div>

      {showTimeEntryForm && (
        <Card>
          <CardHeader>
            <CardTitle>Добавить запись времени</CardTitle>
            <CardDescription>Внесите данные о затраченном времени на проект</CardDescription>
          </CardHeader>
          <CardContent>
            <TimeEntryForm onClose={() => setShowTimeEntryForm(false)} />
          </CardContent>
        </Card>
      )}

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <div className="space-y-0.5">
              <CardTitle className="text-base">Часы сегодня</CardTitle>
              <CardDescription>{dashboardData.todayDate}</CardDescription>
            </div>
            <Clock className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboardData.todayHours} ч</div>
            <Progress value={Math.min(dashboardData.todayHours * 12.5, 100)} className="h-2 mt-2" />
            <p className="text-xs text-gray-500 mt-1">{Math.min(Math.round(dashboardData.todayHours / 8 * 100), 100)}% от дневной нормы</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <div className="space-y-0.5">
              <CardTitle className="text-base">Часы за неделю</CardTitle>
              <CardDescription>{weekDisplay}</CardDescription>
            </div>
            <Calendar className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboardData.weeklyHours} ч</div>
            {dashboardData.weeklyChange !== 0 && (
              <div className={`flex items-center text-sm ${dashboardData.weeklyChange >= 0 ? 'text-green-600' : 'text-red-600'} mt-1`}>
                {dashboardData.weeklyChange >= 0 ? 
                  <ArrowUpRight className="h-4 w-4 mr-1" /> : 
                  <ArrowDownRight className="h-4 w-4 mr-1" />
                }
                <span>{Math.abs(dashboardData.weeklyChange)}% {dashboardData.weeklyChange >= 0 ? 'больше' : 'меньше'} прошлой недели</span>
              </div>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <div className="space-y-0.5">
              <CardTitle className="text-base">Активные проекты</CardTitle>
              <CardDescription>Требуют внимания</CardDescription>
            </div>
            <BarChart3 className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboardData.activeProjects}</div>
            {dashboardData.overdueProjects > 0 && (
              <div className="flex items-center text-sm text-red-600 mt-1">
                <ArrowDownRight className="h-4 w-4 mr-1" />
                <span>{dashboardData.overdueProjects} проекта просрочены</span>
              </div>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <div className="space-y-0.5">
              <CardTitle className="text-base">Эффективность</CardTitle>
              <CardDescription>Биллинг времени</CardDescription>
            </div>
            <BarChart3 className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboardData.efficiency}%</div>
            <Progress value={dashboardData.efficiency} className="h-2 mt-2" />
            <p className="text-xs text-gray-500 mt-1">Целевой показатель: 85%</p>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="projects">
        <TabsList>
          <TabsTrigger value="projects">Проекты</TabsTrigger>
          <TabsTrigger value="team">Команда</TabsTrigger>
          <TabsTrigger value="recent">Недавние записи</TabsTrigger>
        </TabsList>
        <TabsContent value="projects" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle>Активные проекты</CardTitle>
              <CardDescription>Управляйте распределением времени по проектам</CardDescription>
            </CardHeader>
            <CardContent>
              <ProjectTable />
            </CardContent>
            <CardFooter>
              <Button variant="outline" size="sm">
                Просмотреть все проекты
              </Button>
            </CardFooter>
          </Card>
        </TabsContent>
        <TabsContent value="team" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle>Команда</CardTitle>
              <CardDescription>Распределение нагрузки по сотрудникам</CardDescription>
            </CardHeader>
            <CardContent>
              <p>Здесь будет отображаться информация о команде и загрузке сотрудников.</p>
            </CardContent>
          </Card>
        </TabsContent>
        <TabsContent value="recent" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle>Недавние записи времени</CardTitle>
              <CardDescription>Последние внесенные записи учета времени</CardDescription>
            </CardHeader>
            <CardContent>
              <p>Здесь будут отображаться последние записи учета времени.</p>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}
