"use client"

import { useState } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { DatePickerWithRange } from "@/components/date-range-picker"
import { Download, FileSpreadsheet, Eye } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"

export function EmployeeReports() {
  const [selectedEmployee, setSelectedEmployee] = useState("all")
  const [dateRange, setDateRange] = useState({
    from: new Date(2025, 3, 1),
    to: new Date(2025, 3, 16),
  })

  const [showDownloadDialog, setShowDownloadDialog] = useState(false)
  const [selectedReport, setSelectedReport] = useState(null)
  const [selectedColumns, setSelectedColumns] = useState({
    market: true,
    contractingAgency: true,
    client: true,
    projectBrand: true,
    media: true,
    jobType: true,
    comments: true,
    hours: true,
    date: true,
  })

  // Пример данных отчетов
  const reports = [
    {
      id: 1,
      employee: "Олексій Петров",
      employeeId: 1,
      period: "1-15 Квітня, 2025",
      totalHours: 82,
      projects: 4,
      efficiency: 92,
      status: "Підтверджено",
      date: "15.04.2025",
      market: "Ринок 1",
      contractingAgency: "Агенція 1",
      client: "Клієнт 1",
      projectBrand: "Проект 1",
      media: "Медіа 1",
      jobType: "Тип 1",
      comments: "Коментар 1",
      hours: 8,
    },
    {
      id: 2,
      employee: "Олена Сидорова",
      employeeId: 2,
      period: "1-15 Квітня, 2025",
      totalHours: 76,
      projects: 6,
      efficiency: 85,
      status: "Підтверджено",
      date: "15.04.2025",
      market: "Ринок 2",
      contractingAgency: "Агенція 2",
      client: "Клієнт 2",
      projectBrand: "Проект 2",
      media: "Медіа 2",
      jobType: "Тип 2",
      comments: "Коментар 2",
      hours: 7,
    },
    {
      id: 3,
      employee: "Іван Смирнов",
      employeeId: 3,
      period: "1-15 Квітня, 2025",
      totalHours: 80,
      projects: 3,
      efficiency: 78,
      status: "На перевірці",
      date: "15.04.2025",
      market: "Ринок 3",
      contractingAgency: "Агенція 3",
      client: "Клієнт 3",
      projectBrand: "Проект 3",
      media: "Медіа 3",
      jobType: "Тип 3",
      comments: "Коментар 3",
      hours: 9,
    },
    {
      id: 4,
      employee: "Марія Козлова",
      employeeId: 4,
      period: "1-15 Квітня, 2025",
      totalHours: 84,
      projects: 8,
      efficiency: 88,
      status: "Підтверджено",
      date: "15.04.2025",
      market: "Ринок 4",
      contractingAgency: "Агенція 4",
      client: "Клієнт 4",
      projectBrand: "Проект 4",
      media: "Медіа 4",
      jobType: "Тип 4",
      comments: "Коментар 4",
      hours: 6,
    },
    {
      id: 5,
      employee: "Дмитро Новіков",
      employeeId: 5,
      period: "1-15 Квітня, 2025",
      totalHours: 78,
      projects: 5,
      efficiency: 76,
      status: "На перевірці",
      date: "15.04.2025",
      market: "Ринок 5",
      contractingAgency: "Агенція 5",
      client: "Клієнт 5",
      projectBrand: "Проект 5",
      media: "Медіа 5",
      jobType: "Тип 5",
      comments: "Коментар 5",
      hours: 10,
    },
  ]

  // Фильтрация отчетов по выбранному сотруднику
  const filteredReports =
    selectedEmployee === "all"
      ? reports
      : reports.filter((report) => report.employeeId === Number.parseInt(selectedEmployee))

  // Функція для відкриття діалогу завантаження звіту
  const openDownloadDialog = (reportId) => {
    const report = reports.find((r) => r.id === reportId)
    if (report) {
      setSelectedReport(report)
      setShowDownloadDialog(true)
    }
  }

  // Функція для завантаження звіту з вибраними стовпцями
  const handleDownloadWithColumns = () => {
    console.log(`Завантаження звіту ID: ${selectedReport.id} з вибраними стовпцями:`, selectedColumns)
    alert(`Звіт ID: ${selectedReport.id} завантажується у форматі Excel з вибраними стовпцями`)
    setShowDownloadDialog(false)
  }

  // Функция для скачивания всех отчетов в Excel формате
  const downloadAllReports = () => {
    console.log("Завантаження всіх звітів")
    alert("Всі звіти завантажуються у форматі Excel...")
  }

  // Function to download a report (implementation needed)
  const downloadReport = (employeeId) => {
    console.log(`Downloading report for employee ID: ${employeeId}`)
    alert(`Report for employee ID: ${employeeId} is being downloaded in Excel format...`)
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Звіти співробітників</h1>
          <p className="text-gray-500">Перегляд та експорт звітів про робочий час</p>
        </div>
        <Button onClick={downloadAllReports} className="gap-2">
          <FileSpreadsheet className="h-4 w-4" />
          Експорт всіх в Excel
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Фільтри звітів</CardTitle>
          <CardDescription>Оберіть параметри для відображення звітів</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="text-sm font-medium mb-2 block">Співробітник</label>
              <Select value={selectedEmployee} onValueChange={setSelectedEmployee}>
                <SelectTrigger>
                  <SelectValue placeholder="Оберіть співробітника" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Всі співробітники</SelectItem>
                  <SelectItem value="1">Олексій Петров</SelectItem>
                  <SelectItem value="2">Олена Сидорова</SelectItem>
                  <SelectItem value="3">Іван Смирнов</SelectItem>
                  <SelectItem value="4">Марія Козлова</SelectItem>
                  <SelectItem value="5">Дмитро Новіков</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="col-span-1 md:col-span-2">
              <label className="text-sm font-medium mb-2 block">Період</label>
              <DatePickerWithRange date={dateRange} setDate={setDateRange} />
            </div>
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue="summary">
        <TabsList>
          <TabsTrigger value="summary">Зведення</TabsTrigger>
          <TabsTrigger value="detailed">Детальний звіт</TabsTrigger>
        </TabsList>
        <TabsContent value="summary" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle>Зведення звітів</CardTitle>
              <CardDescription>Загальна інформація по звітах співробітників</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Співробітник</TableHead>
                    <TableHead>Період</TableHead>
                    <TableHead>Всього годин</TableHead>
                    <TableHead>Проєкти</TableHead>
                    <TableHead>Ефективність</TableHead>
                    <TableHead>Статус</TableHead>
                    <TableHead>Дії</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredReports.map((report) => (
                    <TableRow key={report.id}>
                      <TableCell className="font-medium">{report.employee}</TableCell>
                      <TableCell>{report.period}</TableCell>
                      <TableCell>{report.totalHours}</TableCell>
                      <TableCell>{report.projects}</TableCell>
                      <TableCell>{report.efficiency}%</TableCell>
                      <TableCell>
                        <Badge
                          variant="outline"
                          className={
                            report.status === "Підтверджено"
                              ? "bg-green-50 text-green-700"
                              : "bg-yellow-50 text-yellow-700"
                          }
                        >
                          {report.status === "Подтвержден" ? "Підтверджено" : "На перевірці"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button variant="ghost" size="icon">
                            <Eye className="h-4 w-4" />
                            <span className="sr-only">Перегляд</span>
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => openDownloadDialog(report.id)}>
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
              <p className="text-muted-foreground mb-4">
                Оберіть співробітника у фільтрах вище, щоб побачити детальний звіт за проєктами та завданнями.
              </p>
              {selectedEmployee !== "all" ? (
                <div className="space-y-6">
                  <div className="border rounded-lg p-4">
                    <h3 className="font-medium mb-2">Розподіл часу за проєктами</h3>
                    <div className="space-y-3">
                      <div>
                        <div className="flex justify-between text-sm mb-1">
                          <span>Ребрендинг Acme Inc</span>
                          <span>24ч (30%)</span>
                        </div>
                        <div className="w-full bg-gray-100 rounded-full h-2">
                          <div className="bg-primary h-2 rounded-full" style={{ width: "30%" }}></div>
                        </div>
                      </div>
                      <div>
                        <div className="flex justify-between text-sm mb-1">
                          <span>Маркетингова кампанія</span>
                          <span>18ч (22%)</span>
                        </div>
                        <div className="w-full bg-gray-100 rounded-full h-2">
                          <div className="bg-primary h-2 rounded-full" style={{ width: "22%" }}></div>
                        </div>
                      </div>
                      <div>
                        <div className="flex justify-between text-sm mb-1">
                          <span>Розробка веб-сайту</span>
                          <span>32ч (40%)</span>
                        </div>
                        <div className="w-full bg-gray-100 rounded-full h-2">
                          <div className="bg-primary h-2 rounded-full" style={{ width: "40%" }}></div>
                        </div>
                      </div>
                      <div>
                        <div className="flex justify-between text-sm mb-1">
                          <span>Інші завдання</span>
                          <span>6ч (8%)</span>
                        </div>
                        <div className="w-full bg-gray-100 rounded-full h-2">
                          <div className="bg-primary h-2 rounded-full" style={{ width: "8%" }}></div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Дата</TableHead>
                        <TableHead>Проєкт</TableHead>
                        <TableHead>Завдання</TableHead>
                        <TableHead>Години</TableHead>
                        <TableHead>Коментар</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      <TableRow>
                        <TableCell>15.04.2025</TableCell>
                        <TableCell>Ребрендинг Acme Inc</TableCell>
                        <TableCell>Дизайн логотипу</TableCell>
                        <TableCell>4.5</TableCell>
                        <TableCell>Фінальні правки за коментарями клієнта</TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell>14.04.2025</TableCell>
                        <TableCell>Розробка веб-сайту</TableCell>
                        <TableCell>Верстка головної сторінки</TableCell>
                        <TableCell>6.0</TableCell>
                        <TableCell>Адаптивна верстка для мобільних пристроїв</TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell>13.04.2025</TableCell>
                        <TableCell>Маркетингова кампанія</TableCell>
                        <TableCell>Розробка стратегії</TableCell>
                        <TableCell>3.5</TableCell>
                        <TableCell>Аналіз конкурентів та цільової аудиторії</TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell>12.04.2025</TableCell>
                        <TableCell>Розробка веб-сайту</TableCell>
                        <TableCell>Програмування</TableCell>
                        <TableCell>8.0</TableCell>
                        <TableCell>Інтеграція CMS та налаштування форм</TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell>11.04.2025</TableCell>
                        <TableCell>Ребрендинг Acme Inc</TableCell>
                        <TableCell>Дизайн брошури</TableCell>
                        <TableCell>5.5</TableCell>
                        <TableCell>Створення макетів для друку</TableCell>
                      </TableRow>
                    </TableBody>
                  </Table>

                  <div className="flex justify-end">
                    <Button onClick={() => downloadReport(Number.parseInt(selectedEmployee))} className="gap-2">
                      <FileSpreadsheet className="h-4 w-4" />
                      Експорт в Excel
                    </Button>
                  </div>
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center py-8 text-center">
                  <FileSpreadsheet className="h-16 w-16 text-gray-300 mb-4" />
                  <h3 className="text-lg font-medium mb-1">Оберіть співробітника</h3>
                  <p className="text-muted-foreground">
                    Для перегляду детального звіту оберіть конкретного співробітника у фільтрах вище
                  </p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Діалог для вибору стовпців та попереднього перегляду */}
      <Dialog open={showDownloadDialog} onOpenChange={setShowDownloadDialog}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Завантаження звіту</DialogTitle>
            <DialogDescription>
              Оберіть стовпці для завантаження та перегляньте попередній вигляд таблиці
            </DialogDescription>
          </DialogHeader>

          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 py-4">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-date"
                checked={selectedColumns.date}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, date: !!checked })}
              />
              <label
                htmlFor="column-date"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Дата
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-market"
                checked={selectedColumns.market}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, market: !!checked })}
              />
              <label
                htmlFor="column-market"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Ринок
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-agency"
                checked={selectedColumns.contractingAgency}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, contractingAgency: !!checked })}
              />
              <label
                htmlFor="column-agency"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Агентство
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-client"
                checked={selectedColumns.client}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, client: !!checked })}
              />
              <label
                htmlFor="column-client"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Клієнт
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-project"
                checked={selectedColumns.projectBrand}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, projectBrand: !!checked })}
              />
              <label
                htmlFor="column-project"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Проект/Бренд
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-media"
                checked={selectedColumns.media}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, media: !!checked })}
              />
              <label
                htmlFor="column-media"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Медіа
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-jobType"
                checked={selectedColumns.jobType}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, jobType: !!checked })}
              />
              <label
                htmlFor="column-jobType"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Тип роботи
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-comments"
                checked={selectedColumns.comments}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, comments: !!checked })}
              />
              <label
                htmlFor="column-comments"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Коментарі
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="column-hours"
                checked={selectedColumns.hours}
                onCheckedChange={(checked) => setSelectedColumns({ ...selectedColumns, hours: !!checked })}
              />
              <label
                htmlFor="column-hours"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                Години
              </label>
            </div>
          </div>

          <div className="border rounded-md p-2 my-4 overflow-x-auto" style={{ maxHeight: "400px", overflowY: "auto" }}>
            <h3 className="font-medium mb-2">Попередній перегляд таблиці</h3>
            {selectedReport && (
              <Table>
                <TableHeader>
                  <TableRow>
                    {selectedColumns.date && <TableHead>Дата</TableHead>}
                    {selectedColumns.market && <TableHead>Ринок</TableHead>}
                    {selectedColumns.contractingAgency && <TableHead>Агентство</TableHead>}
                    {selectedColumns.client && <TableHead>Клієнт</TableHead>}
                    {selectedColumns.projectBrand && <TableHead>Проект/Бренд</TableHead>}
                    {selectedColumns.media && <TableHead>Медіа</TableHead>}
                    {selectedColumns.jobType && <TableHead>Тип роботи</TableHead>}
                    {selectedColumns.comments && <TableHead>Коментарі</TableHead>}
                    {selectedColumns.hours && <TableHead>Години</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {/* Перший запис - вибраний звіт */}
                  <TableRow>
                    {selectedColumns.date && <TableCell>{selectedReport.date}</TableCell>}
                    {selectedColumns.market && <TableCell>{selectedReport.market || "—"}</TableCell>}
                    {selectedColumns.contractingAgency && (
                      <TableCell>{selectedReport.contractingAgency || "—"}</TableCell>
                    )}
                    {selectedColumns.client && <TableCell>{selectedReport.client || "—"}</TableCell>}
                    {selectedColumns.projectBrand && <TableCell>{selectedReport.projectBrand || "—"}</TableCell>}
                    {selectedColumns.media && <TableCell>{selectedReport.media || "—"}</TableCell>}
                    {selectedColumns.jobType && <TableCell>{selectedReport.jobType || "—"}</TableCell>}
                    {selectedColumns.comments && <TableCell>{selectedReport.comments || "—"}</TableCell>}
                    {selectedColumns.hours && <TableCell>{selectedReport.hours}</TableCell>}
                  </TableRow>

                  {/* Додаткові записи з реальними даними */}
                  <TableRow>
                    {selectedColumns.date && <TableCell>15.04.2025</TableCell>}
                    {selectedColumns.market && <TableCell>Україна</TableCell>}
                    {selectedColumns.contractingAgency && <TableCell>MediaCom</TableCell>}
                    {selectedColumns.client && <TableCell>Coca-Cola</TableCell>}
                    {selectedColumns.projectBrand && <TableCell>Літня кампанія 2025</TableCell>}
                    {selectedColumns.media && <TableCell>Digital - Paid Social</TableCell>}
                    {selectedColumns.jobType && <TableCell>Media plans</TableCell>}
                    {selectedColumns.comments && <TableCell>Розробка медіа-плану для Instagram та TikTok</TableCell>}
                    {selectedColumns.hours && <TableCell>4.5</TableCell>}
                  </TableRow>

                  <TableRow>
                    {selectedColumns.date && <TableCell>15.04.2025</TableCell>}
                    {selectedColumns.market && <TableCell>Європа</TableCell>}
                    {selectedColumns.contractingAgency && <TableCell>Mindshare</TableCell>}
                    {selectedColumns.client && <TableCell>Adidas</TableCell>}
                    {selectedColumns.projectBrand && <TableCell>Запуск нової колекції</TableCell>}
                    {selectedColumns.media && <TableCell>Digital - Video</TableCell>}
                    {selectedColumns.jobType && <TableCell>Campaign running</TableCell>}
                    {selectedColumns.comments && <TableCell>Налаштування таргетингу для YouTube кампанії</TableCell>}
                    {selectedColumns.hours && <TableCell>3.0</TableCell>}
                  </TableRow>

                  <TableRow>
                    {selectedColumns.date && <TableCell>14.04.2025</TableCell>}
                    {selectedColumns.market && <TableCell>Україна</TableCell>}
                    {selectedColumns.contractingAgency && <TableCell>Wavemaker</TableCell>}
                    {selectedColumns.client && <TableCell>Darnitsa</TableCell>}
                    {selectedColumns.projectBrand && <TableCell>Промо нового препарату</TableCell>}
                    {selectedColumns.media && <TableCell>TV</TableCell>}
                    {selectedColumns.jobType && <TableCell>Strategy planning</TableCell>}
                    {selectedColumns.comments && <TableCell>Аналіз ефективності рекламних слотів</TableCell>}
                    {selectedColumns.hours && <TableCell>5.5</TableCell>}
                  </TableRow>

                  <TableRow>
                    {selectedColumns.date && <TableCell>14.04.2025</TableCell>}
                    {selectedColumns.market && <TableCell>Глобальний</TableCell>}
                    {selectedColumns.contractingAgency && <TableCell>GroupM</TableCell>}
                    {selectedColumns.client && <TableCell>IKEA</TableCell>}
                    {selectedColumns.projectBrand && <TableCell>Сезонна кампанія</TableCell>}
                    {selectedColumns.media && <TableCell>Digital Commerce</TableCell>}
                    {selectedColumns.jobType && <TableCell>Reporting</TableCell>}
                    {selectedColumns.comments && <TableCell>Підготовка щотижневого звіту з ефективності</TableCell>}
                    {selectedColumns.hours && <TableCell>2.5</TableCell>}
                  </TableRow>

                  <TableRow>
                    {selectedColumns.date && <TableCell>13.04.2025</TableCell>}
                    {selectedColumns.market && <TableCell>Україна</TableCell>}
                    {selectedColumns.contractingAgency && <TableCell>MediaCom</TableCell>}
                    {selectedColumns.client && <TableCell>Fozzy Group</TableCell>}
                    {selectedColumns.projectBrand && <TableCell>Великодня кампанія</TableCell>}
                    {selectedColumns.media && <TableCell>OOH</TableCell>}
                    {selectedColumns.jobType && <TableCell>Media plans</TableCell>}
                    {selectedColumns.comments && <TableCell>Планування розміщення бордів у Києві</TableCell>}
                    {selectedColumns.hours && <TableCell>3.0</TableCell>}
                  </TableRow>
                </TableBody>
              </Table>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDownloadDialog(false)}>
              Скасувати
            </Button>
            <Button onClick={handleDownloadWithColumns} className="gap-2">
              <Download className="h-4 w-4" />
              Завантажити
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
