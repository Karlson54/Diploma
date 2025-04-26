"use client"

import { useState } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { DatePickerWithRange } from "@/components/date-range-picker"
import { Download, Eye, FileSpreadsheet } from "lucide-react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

export function EmployeeReports() {
  const [dateRange, setDateRange] = useState({
    from: new Date(2025, 3, 1),
    to: new Date(2025, 3, 16),
  })

  // Приклад даних звітів поточного співробітника
  // В реальному додатку ці дані будуть завантажуватися з API
  const reports = [
    {
      id: 1,
      date: "16.04.2025",
      totalHours: 8.5,
      companies: "Acme Inc, Tech Solutions",
      tasks: [
        { company: "Acme Inc", description: "Розробка дизайну логотипу", hours: 4.5 },
        { company: "Tech Solutions", description: "Верстка головної сторінки", hours: 4.0 },
      ],
    },
    {
      id: 2,
      date: "15.04.2025",
      totalHours: 8.0,
      companies: "Acme Inc",
      tasks: [
        { company: "Acme Inc", description: "Дизайн брошури", hours: 5.5 },
        { company: "Acme Inc", description: "Зустріч з клієнтом", hours: 2.5 },
      ],
    },
    {
      id: 3,
      date: "14.04.2025",
      totalHours: 7.5,
      companies: "Tech Solutions",
      tasks: [{ company: "Tech Solutions", description: "Адаптивна верстка для мобільних пристроїв", hours: 7.5 }],
    },
    {
      id: 4,
      date: "13.04.2025",
      totalHours: 8.0,
      companies: "Globex Corp",
      tasks: [
        { company: "Globex Corp", description: "Розробка стратегії маркетингової кампанії", hours: 3.5 },
        { company: "Globex Corp", description: "Аналіз конкурентів", hours: 4.5 },
      ],
    },
    {
      id: 5,
      date: "12.04.2025",
      totalHours: 6.5,
      companies: "Tech Solutions",
      tasks: [{ company: "Tech Solutions", description: "Інтеграція CMS та налаштування форм", hours: 6.5 }],
    },
  ]

  // Функція для завантаження звіту
  const downloadReport = (reportId) => {
    console.log(`Завантаження звіту ID: ${reportId}`)
    alert(`Звіт ID: ${reportId} завантажується у форматі Excel...`)
  }

  // Функція для завантаження всіх звітів
  const downloadAllReports = () => {
    console.log("Завантаження всіх звітів")
    alert("Всі звіти завантажуються у форматі Excel...")
  }

  // Функція для перегляду деталей звіту
  const [selectedReport, setSelectedReport] = useState(null)
  const [showDetails, setShowDetails] = useState(false)

  const viewReportDetails = (report) => {
    setSelectedReport(report)
    setShowDetails(true)
  }

  // Розрахунок загальної кількості годин
  const totalHours = reports.reduce((sum, report) => sum + report.totalHours, 0)

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Мої звіти</h1>
          <p className="text-gray-500">Перегляд та експорт ваших звітів робочого часу</p>
        </div>
        <Button onClick={downloadAllReports} className="gap-2">
          <FileSpreadsheet className="h-4 w-4" />
          Експорт всіх в Excel
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Фільтри звітів</CardTitle>
          <CardDescription>Оберіть період для відображення звітів</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="col-span-1 md:col-span-2">
              <label className="text-sm font-medium mb-2 block">Період</label>
              <DatePickerWithRange date={dateRange} setDate={setDateRange} />
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">Загальна кількість годин</CardTitle>
            <CardDescription>За обраний період</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalHours} годин</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">Кількість звітів</CardTitle>
            <CardDescription>За обраний період</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{reports.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">Середній час на день</CardTitle>
            <CardDescription>За обраний період</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{(totalHours / reports.length).toFixed(1)} годин</div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="summary">
        <TabsList>
          <TabsTrigger value="summary">Зведена інформація</TabsTrigger>
          <TabsTrigger value="detailed">Детальний звіт</TabsTrigger>
        </TabsList>
        <TabsContent value="summary" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle>Зведена інформація</CardTitle>
              <CardDescription>Загальна інформація по звітах</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Дата</TableHead>
                    <TableHead>Години</TableHead>
                    <TableHead>Компанії</TableHead>
                    <TableHead>Дії</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {reports.map((report) => (
                    <TableRow key={report.id}>
                      <TableCell className="font-medium">{report.date}</TableCell>
                      <TableCell>{report.totalHours}</TableCell>
                      <TableCell>{report.companies}</TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button variant="ghost" size="icon" onClick={() => viewReportDetails(report)}>
                            <Eye className="h-4 w-4" />
                            <span className="sr-only">Перегляд</span>
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => downloadReport(report.id)}>
                            <Download className="h-4 w-4" />
                            <span className="sr-only">Завантажити</span>
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
        <TabsContent value="detailed" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle>Детальний звіт</CardTitle>
              <CardDescription>Детальна інформація про витрачений час</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                <div className="border rounded-lg p-4">
                  <h3 className="font-medium mb-2">Розподіл часу по компаніях</h3>
                  <div className="space-y-3">
                    <div>
                      <div className="flex justify-between text-sm mb-1">
                        <span>Acme Inc</span>
                        <span>12.5г (36%)</span>
                      </div>
                      <div className="w-full bg-gray-100 rounded-full h-2">
                        <div className="bg-primary h-2 rounded-full" style={{ width: "36%" }}></div>
                      </div>
                    </div>
                    <div>
                      <div className="flex justify-between text-sm mb-1">
                        <span>Tech Solutions</span>
                        <span>18.0г (52%)</span>
                      </div>
                      <div className="w-full bg-gray-100 rounded-full h-2">
                        <div className="bg-primary h-2 rounded-full" style={{ width: "52%" }}></div>
                      </div>
                    </div>
                    <div>
                      <div className="flex justify-between text-sm mb-1">
                        <span>Globex Corp</span>
                        <span>8.0г (12%)</span>
                      </div>
                      <div className="w-full bg-gray-100 rounded-full h-2">
                        <div className="bg-primary h-2 rounded-full" style={{ width: "12%" }}></div>
                      </div>
                    </div>
                  </div>
                </div>

                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Дата</TableHead>
                      <TableHead>Компанія</TableHead>
                      <TableHead>Опис роботи</TableHead>
                      <TableHead>Години</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {reports.flatMap((report) =>
                      report.tasks.map((task, index) => (
                        <TableRow key={`${report.id}-${index}`}>
                          <TableCell>{report.date}</TableCell>
                          <TableCell>{task.company}</TableCell>
                          <TableCell>{task.description}</TableCell>
                          <TableCell>{task.hours}</TableCell>
                        </TableRow>
                      )),
                    )}
                  </TableBody>
                </Table>

                <div className="flex justify-end">
                  <Button onClick={downloadAllReports} className="gap-2">
                    <FileSpreadsheet className="h-4 w-4" />
                    Експорт в Excel
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {showDetails && selectedReport && (
        <Card>
          <CardHeader>
            <div className="flex justify-between items-center">
              <div>
                <CardTitle>Деталі звіту за {selectedReport.date}</CardTitle>
                <CardDescription>Загальний час: {selectedReport.totalHours} годин</CardDescription>
              </div>
              <Button variant="outline" size="sm" onClick={() => setShowDetails(false)}>
                Закрити
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Компанія</TableHead>
                  <TableHead>Опис роботи</TableHead>
                  <TableHead>Години</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {selectedReport.tasks.map((task, index) => (
                  <TableRow key={index}>
                    <TableCell>{task.company}</TableCell>
                    <TableCell>{task.description}</TableCell>
                    <TableCell>{task.hours}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
