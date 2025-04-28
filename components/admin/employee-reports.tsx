"use client"

import { useState, useEffect } from "react"
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
import ExcelJS from 'exceljs'

interface Employee {
  id: number;
  name: string;
  email: string;
  position: string;
  department: string;
  agency: string;
}

interface Report {
  id: number;
  employee: string;
  employeeId: number;
  period: string;
  totalHours: number;
  projects: number;
  efficiency: number;
  status: string;
  date: string;
  market: string;
  contractingAgency: string;
  client: string;
  projectBrand: string;
  media: string;
  jobType: string;
  comments: string;
  hours: number;
  company: string;
}

export function EmployeeReports() {
  const [selectedEmployee, setSelectedEmployee] = useState("all")
  const [dateRange, setDateRange] = useState({
    from: new Date(new Date().setDate(1)), // First day of current month
    to: new Date(), // Today
  })

  const [showDownloadDialog, setShowDownloadDialog] = useState(false)
  const [selectedReport, setSelectedReport] = useState<Report | null>(null)
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
    fullName: true,
    company: true,
  })

  const [reports, setReports] = useState<Report[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function fetchData() {
      try {
        // Fetch employees
        const employeesResponse = await fetch('/api/admin', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ action: 'getEmployees' }),
        });
        
        if (!employeesResponse.ok) {
          throw new Error('Failed to fetch employees');
        }
        
        const employeesData = await employeesResponse.json();
        setEmployees(employeesData.employees);
        
        // Fetch reports
        const reportsResponse = await fetch('/api/admin', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ action: 'getReports' }),
        });
        
        if (!reportsResponse.ok) {
          throw new Error('Failed to fetch reports');
        }
        
        const reportsData = await reportsResponse.json();
        
        // Process and transform the reports data
        const processedReports = reportsData.reports.map((item: any) => {
          const reportDate = new Date(item.report.date);
          const formattedDate = reportDate.toLocaleDateString('uk-UA', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
          });
          
          // Determine the period (e.g., "1-15 [Month], [Year]")
          const day = reportDate.getDate();
          const month = reportDate.toLocaleDateString('uk-UA', { month: 'long' });
          const year = reportDate.getFullYear();
          const periodPrefix = day <= 15 ? "1-15" : "16-30";
          const period = `${periodPrefix} ${month}, ${year}`;
          
          // Calculate efficiency (arbitrary for this example)
          const efficiency = Math.floor(70 + Math.random() * 30);
          
          // Check if the projectBrand field is available in the API response
          // We'll try both 'project' and 'projectBrand' fields, as well as nested fields
          const projectBrandValue = 
            item.report.projectBrand || 
            item.report.project_brand || 
            item.report.project || 
            (item.project ? item.project : "N/A");
          
          return {
            id: item.report.id,
            employee: item.employee?.name || 'Unknown',
            employeeId: item.employee?.id || 0,
            period,
            totalHours: item.report.hours || 0,
            projects: Math.floor(Math.random() * 8) + 1, // Mock projects count
            efficiency,
            status: Math.random() > 0.3 ? "Підтверджено" : "На перевірці",
            date: formattedDate,
            market: item.report.market || "N/A",
            contractingAgency: item.report.contractingAgency || "N/A",
            client: item.report.client || "N/A",
            projectBrand: projectBrandValue,
            media: item.report.media || "N/A",
            jobType: item.report.jobType || "N/A",
            comments: item.report.comments || "N/A",
            hours: item.report.hours || 0,
            company: item.employee?.agency || 'MediaCom',
          };
        });
        
        // Log the first report for debugging
        if (reportsData.reports.length > 0) {
          console.log("First report data:", reportsData.reports[0]);
        }
        
        setReports(processedReports);
      } catch (error) {
        console.error("Error fetching data:", error);
      } finally {
        setLoading(false);
      }
    }
    
    fetchData();
  }, []);

  // Фильтрация отчетов по выбранному сотруднику
  const filteredReports =
    selectedEmployee === "all"
      ? reports
      : reports.filter((report) => report.employeeId === Number.parseInt(selectedEmployee))

  // Функція для створення та завантаження Excel файлу
  const createAndDownloadExcel = async (reportsToExport: Report[], fileName: string) => {
    try {
      // Create a new workbook and worksheet
      const workbook = new ExcelJS.Workbook()
      const worksheet = workbook.addWorksheet('Звіти')
      
      // Define columns based on selected columns
      const columns = []
      if (selectedColumns.company) columns.push({ header: 'Agency', key: 'company', width: 20 })
      if (selectedColumns.fullName) columns.push({ header: 'Name', key: 'fullName', width: 20 })
      if (selectedColumns.date) columns.push({ header: 'Date', key: 'date', width: 15 })
      if (selectedColumns.market) columns.push({ header: 'Market', key: 'market', width: 15 })
      if (selectedColumns.contractingAgency) columns.push({ header: 'Contracting Agency / Unit', key: 'contractingAgency', width: 20 })
      if (selectedColumns.client) columns.push({ header: 'Client', key: 'client', width: 20 })
      if (selectedColumns.projectBrand) columns.push({ header: 'Project / brand', key: 'projectBrand', width: 20 })
      if (selectedColumns.media) columns.push({ header: 'Media', key: 'media', width: 15 })
      if (selectedColumns.jobType) columns.push({ header: 'Job type', key: 'jobType', width: 20 })
      if (selectedColumns.hours) columns.push({ header: 'Hours', key: 'hours', width: 10 })
      if (selectedColumns.comments) columns.push({ header: 'Comments', key: 'comments', width: 25 })
      
      worksheet.columns = columns
      
      // Style the headers
      worksheet.getRow(1).font = { bold: true }
      worksheet.getRow(1).fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFE6E6E6' }
      }
      
      // Add data rows
      reportsToExport.forEach(report => {
        const rowData: any = {}
        if (selectedColumns.company) rowData.company = report.company // Use employee's agency field
        if (selectedColumns.fullName) rowData.fullName = report.employee // Using employee name field as full name
        if (selectedColumns.date) rowData.date = report.date
        if (selectedColumns.market) rowData.market = report.market
        if (selectedColumns.contractingAgency) rowData.contractingAgency = report.contractingAgency
        if (selectedColumns.client) rowData.client = report.client
        if (selectedColumns.projectBrand) rowData.projectBrand = report.projectBrand
        if (selectedColumns.media) rowData.media = report.media
        if (selectedColumns.jobType) rowData.jobType = report.jobType
        if (selectedColumns.hours) rowData.hours = report.hours
        if (selectedColumns.comments) rowData.comments = report.comments
        
        worksheet.addRow(rowData)
      })
      
      // Add borders to all cells
      worksheet.eachRow({ includeEmpty: false }, row => {
        row.eachCell({ includeEmpty: false }, cell => {
          cell.border = {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
          }
        })
      })
      
      // Generate Excel file
      const buffer = await workbook.xlsx.writeBuffer()
      const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
      const url = URL.createObjectURL(blob)
      
      // Create download link
      const a = document.createElement('a')
      a.href = url
      a.download = fileName
      a.click()
      
      // Clean up
      URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Помилка при створенні Excel файлу:', error)
      alert('Виникла помилка при створенні Excel файлу')
    }
  }

  // Функція для завантаження звіту з вибраними стовпцями
  const handleDownloadWithColumns = async () => {
    if (!selectedReport) return;
    
    try {
      await createAndDownloadExcel(
        [selectedReport], 
        `Звіт_${selectedReport.employee.replace(/\s+/g, '_')}_${selectedReport.date.replace(/\./g, '-')}.xlsx`
      )
      setShowDownloadDialog(false)
    } catch (error) {
      console.error('Помилка при завантаженні звіту:', error)
      alert('Виникла помилка при створенні Excel файлу')
    }
  }

  // Функция для скачивания всех отчетов в Excel формате
  const downloadAllReports = async () => {
    try {
      const reportsToExport = filteredReports
      if (reportsToExport.length === 0) {
        alert('Немає звітів для експорту')
        return
      }
      
      const employeeInfo = selectedEmployee === 'all' 
        ? 'All_Employees' 
        : employees.find(e => e.id.toString() === selectedEmployee)?.name.replace(/\s+/g, '_') || 'Employee'
      
      const fromDate = dateRange.from.toLocaleDateString().replace(/\./g, '-')
      const toDate = dateRange.to.toLocaleDateString().replace(/\./g, '-')
      
      await createAndDownloadExcel(
        reportsToExport,
        `Звіти_${employeeInfo}_${fromDate}_${toDate}.xlsx`
      )
    } catch (error) {
      console.error('Помилка при завантаженні всіх звітів:', error)
      alert('Виникла помилка при створенні Excel файлу')
    }
  }

  // Function to download a report
  const downloadReport = async (reportId: number) => {
    try {
      const report = reports.find(r => r.id === reportId)
      if (!report) {
        alert(`Звіт з ID ${reportId} не знайдено`)
        return
      }
      
      setSelectedReport(report)
      setShowDownloadDialog(true)
    } catch (error) {
      console.error(`Помилка при завантаженні звіту ID: ${reportId}:`, error)
      alert('Виникла помилка при створенні Excel файлу')
    }
  }

  if (loading) {
    return <div>Loading employee reports...</div>;
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
                  {employees.map((employee) => (
                    <SelectItem key={employee.id} value={employee.id.toString()}>
                      {employee.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="col-span-1 md:col-span-2">
              <label className="text-sm font-medium mb-2 block">Період</label>
              <DatePickerWithRange 
                date={{
                  from: dateRange.from,
                  to: dateRange.to
                }} 
                setDate={(value) => {
                  if (value?.from && value?.to) {
                    setDateRange({
                      from: value.from,
                      to: value.to
                    });
                  }
                }} 
              />
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
                    <TableHead>Агентство</TableHead>
                    <TableHead>Період</TableHead>
                    <TableHead>Всього годин</TableHead>
                    <TableHead>Проєкти</TableHead>
                    <TableHead>Ефективність</TableHead>
                    <TableHead>Статус</TableHead>
                    <TableHead>Дії</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredReports.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={8} className="text-center">
                        Немає доступних звітів за обраний період
                      </TableCell>
                    </TableRow>
                  ) : (
                    filteredReports.map((report) => (
                      <TableRow key={report.id}>
                        <TableCell className="font-medium">{report.employee}</TableCell>
                        <TableCell>{report.company}</TableCell>
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
                            {report.status}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div className="flex gap-2">
                            <Button variant="ghost" size="icon">
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
                    ))
                  )}
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
                      {filteredReports.length === 0 ? (
                        <TableRow>
                          <TableCell colSpan={5} className="text-center">
                            Немає доступних звітів за обраний період
                          </TableCell>
                        </TableRow>
                      ) : (
                        filteredReports.map((report) => (
                          <TableRow key={report.id}>
                            <TableCell>{report.date}</TableCell>
                            <TableCell>{report.projectBrand}</TableCell>
                            <TableCell>{report.jobType}</TableCell>
                            <TableCell>{report.hours}</TableCell>
                            <TableCell>{report.comments}</TableCell>
                          </TableRow>
                        ))
                      )}
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
        <DialogContent className="sm:max-w-4xl">
          <DialogHeader>
            <DialogTitle>Завантажити звіт</DialogTitle>
            <DialogDescription>
              Оберіть стовпці для включення в Excel звіт
            </DialogDescription>
          </DialogHeader>
          <div className="flex flex-row flex-wrap gap-4 py-4">
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="company-checkbox" 
                checked={selectedColumns.company}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, company: !!checked})
                }
              />
              <label htmlFor="company-checkbox" className="text-sm font-medium">
                Agency
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="fullName-checkbox" 
                checked={selectedColumns.fullName}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, fullName: !!checked})
                }
              />
              <label htmlFor="fullName-checkbox" className="text-sm font-medium">
                Name
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="date-checkbox" 
                checked={selectedColumns.date}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, date: !!checked})
                }
              />
              <label htmlFor="date-checkbox" className="text-sm font-medium">
                Date
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="market-checkbox" 
                checked={selectedColumns.market}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, market: !!checked})
                }
              />
              <label htmlFor="market-checkbox" className="text-sm font-medium">
                Market
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="contractingAgency-checkbox" 
                checked={selectedColumns.contractingAgency}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, contractingAgency: !!checked})
                }
              />
              <label htmlFor="contractingAgency-checkbox" className="text-sm font-medium">
                Contracting Agency / Unit
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="client-checkbox" 
                checked={selectedColumns.client}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, client: !!checked})
                }
              />
              <label htmlFor="client-checkbox" className="text-sm font-medium">
                Client
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="projectBrand-checkbox" 
                checked={selectedColumns.projectBrand}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, projectBrand: !!checked})
                }
              />
              <label htmlFor="projectBrand-checkbox" className="text-sm font-medium">
                Project / brand
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="media-checkbox" 
                checked={selectedColumns.media}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, media: !!checked})
                }
              />
              <label htmlFor="media-checkbox" className="text-sm font-medium">
                Media
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="jobType-checkbox" 
                checked={selectedColumns.jobType}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, jobType: !!checked})
                }
              />
              <label htmlFor="jobType-checkbox" className="text-sm font-medium">
                Job type
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="hours-checkbox" 
                checked={selectedColumns.hours}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, hours: !!checked})
                }
              />
              <label htmlFor="hours-checkbox" className="text-sm font-medium">
                Hours
              </label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox 
                id="comments-checkbox" 
                checked={selectedColumns.comments}
                onCheckedChange={(checked) => 
                  setSelectedColumns({...selectedColumns, comments: !!checked})
                }
              />
              <label htmlFor="comments-checkbox" className="text-sm font-medium">
                Comments
              </label>
            </div>
          </div>
          <div className="border rounded-md p-2 my-4 overflow-x-auto" style={{ maxHeight: "500px", overflowY: "auto" }}>
            <h3 className="font-medium mb-2">Попередній перегляд таблиці</h3>
            {selectedReport && (
              <Table className="min-w-full">
                <TableHeader>
                  <TableRow>
                    {selectedColumns.company && <TableHead>Agency</TableHead>}
                    {selectedColumns.fullName && <TableHead>Name</TableHead>}
                    {selectedColumns.date && <TableHead>Date</TableHead>}
                    {selectedColumns.market && <TableHead>Market</TableHead>}
                    {selectedColumns.contractingAgency && <TableHead>Contracting Agency / Unit</TableHead>}
                    {selectedColumns.client && <TableHead>Client</TableHead>}
                    {selectedColumns.projectBrand && <TableHead>Project / brand</TableHead>}
                    {selectedColumns.media && <TableHead>Media</TableHead>}
                    {selectedColumns.jobType && <TableHead>Job type</TableHead>}
                    {selectedColumns.hours && <TableHead>Hours</TableHead>}
                    {selectedColumns.comments && <TableHead>Comments</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {/* Выбранный отчет */}
                  <TableRow>
                    {selectedColumns.company && <TableCell>{selectedReport.company}</TableCell>}
                    {selectedColumns.fullName && <TableCell>{selectedReport.employee}</TableCell>}
                    {selectedColumns.date && <TableCell>{selectedReport.date}</TableCell>}
                    {selectedColumns.market && <TableCell>{selectedReport.market || "—"}</TableCell>}
                    {selectedColumns.contractingAgency && (
                      <TableCell>{selectedReport.contractingAgency || "—"}</TableCell>
                    )}
                    {selectedColumns.client && <TableCell>{selectedReport.client || "—"}</TableCell>}
                    {selectedColumns.projectBrand && <TableCell>{selectedReport.projectBrand || "—"}</TableCell>}
                    {selectedColumns.media && <TableCell>{selectedReport.media || "—"}</TableCell>}
                    {selectedColumns.jobType && <TableCell>{selectedReport.jobType || "—"}</TableCell>}
                    {selectedColumns.hours && <TableCell>{selectedReport.hours}</TableCell>}
                    {selectedColumns.comments && <TableCell>{selectedReport.comments || "—"}</TableCell>}
                  </TableRow>

                  {/* Дополнительные отчеты для примера */}
                  {reports.slice(0, 2).filter(r => r.id !== selectedReport.id).map(report => (
                    <TableRow key={`preview-${report.id}`}>
                      {selectedColumns.company && <TableCell>{report.company}</TableCell>}
                      {selectedColumns.fullName && <TableCell>{report.employee}</TableCell>}
                      {selectedColumns.date && <TableCell>{report.date}</TableCell>}
                      {selectedColumns.market && <TableCell>{report.market || "—"}</TableCell>}
                      {selectedColumns.contractingAgency && (
                        <TableCell>{report.contractingAgency || "—"}</TableCell>
                      )}
                      {selectedColumns.client && <TableCell>{report.client || "—"}</TableCell>}
                      {selectedColumns.projectBrand && <TableCell>{report.projectBrand || "—"}</TableCell>}
                      {selectedColumns.media && <TableCell>{report.media || "—"}</TableCell>}
                      {selectedColumns.jobType && <TableCell>{report.jobType || "—"}</TableCell>}
                      {selectedColumns.hours && <TableCell>{report.hours}</TableCell>}
                      {selectedColumns.comments && <TableCell>{report.comments || "—"}</TableCell>}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDownloadDialog(false)}>
              Скасувати
            </Button>
            <Button onClick={handleDownloadWithColumns}>
              Завантажити
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
