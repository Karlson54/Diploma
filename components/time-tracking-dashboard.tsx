"use client"

import { useState } from "react"
import { BarChart3, Clock, Calendar, ArrowUpRight, ArrowDownRight, Play, Pause, Plus } from "lucide-react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { ProjectTable } from "@/components/project-table"
import { TimeEntryForm } from "@/components/time-entry-form"

export function TimeTrackingDashboard() {
  const [isTracking, setIsTracking] = useState(false)
  const [showTimeEntryForm, setShowTimeEntryForm] = useState(false)

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
              <CardDescription>Апрель 16, 2025</CardDescription>
            </div>
            <Clock className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">4.5 ч</div>
            <Progress value={56} className="h-2 mt-2" />
            <p className="text-xs text-gray-500 mt-1">56% от дневной нормы</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <div className="space-y-0.5">
              <CardTitle className="text-base">Часы за неделю</CardTitle>
              <CardDescription>10-16 Апреля</CardDescription>
            </div>
            <Calendar className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">32.5 ч</div>
            <div className="flex items-center text-sm text-green-600 mt-1">
              <ArrowUpRight className="h-4 w-4 mr-1" />
              <span>8% больше прошлой недели</span>
            </div>
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
            <div className="text-2xl font-bold">7</div>
            <div className="flex items-center text-sm text-red-600 mt-1">
              <ArrowDownRight className="h-4 w-4 mr-1" />
              <span>2 проекта просрочены</span>
            </div>
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
            <div className="text-2xl font-bold">78%</div>
            <Progress value={78} className="h-2 mt-2" />
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
