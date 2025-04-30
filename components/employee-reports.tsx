"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { DatePickerWithRange } from "@/components/date-range-picker"
import { Download, FileSpreadsheet } from "lucide-react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import type { DateRange } from "react-day-picker"
import ExcelJS from 'exceljs'
import { ReportExportModal } from "@/components/report-export-modal"

interface ReportTask {
  company: string;
  description: string;
  hours: number;
}

interface Report {
  id: number;
  date: string;
  totalHours: number;
  companies: string;
  tasks: ReportTask[];
}

// Define interface for API response
interface ApiReport {
  report: {
    id: number;
    date: string;
    hours: number;
    client?: string;
    contractingAgency?: string;
    jobType?: string;
  };
  employee: {
    id: number;
    name: string;
    agency: string;
  };
}

export function EmployeeReports() {
  // Initialize with definite from and to dates
  const firstDayOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
  const today = new Date();
  
  const [dateRange, setDateRange] = useState<DateRange | undefined>({
    from: firstDayOfMonth,
    to: today
  });
  
  const [reports, setReports] = useState<Report[]>([])
  const [loading, setLoading] = useState(true)
  const [showExportModal, setShowExportModal] = useState(false)
  const [exportData, setExportData] = useState<any[]>([])

  // Fetch reports from the API instead of directly from the database
  useEffect(() => {
    async function fetchReports() {
      try {
        // Fetch from API endpoint instead of directly using database functions
        const response = await fetch('/api/reports');
        if (!response.ok) {
          throw new Error('Failed to fetch reports');
        }
        const allReports: ApiReport[] = await response.json();
        
        // Transform reports to required format
        const formattedReports: Report[] = allReports.map(report => {
          // Format date from ISO to DD.MM.YYYY
          const reportDate = new Date(report.report.date)
          const formattedDate = reportDate.toLocaleDateString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
          });
          
          // Extract companies info
          const companies = [
            report.report.client,
            report.report.contractingAgency
          ].filter(Boolean).join(", ");
          
          // Create task based on actual report data
          const tasks: ReportTask[] = [];
          if (report.report.client) {
            tasks.push({
              company: report.report.client,
              description: report.report.jobType || "Work on project",
              hours: report.report.hours
            });
          }
          
          return {
            id: report.report.id,
            date: formattedDate,
            totalHours: report.report.hours,
            companies,
            tasks
          };
        });
        
        // Filter by date range if provided
        const filteredReports = formattedReports.filter(report => {
          if (!dateRange || !dateRange.from) return true; // No filter if no date range
          
          const reportDate = new Date(report.date.split('.').reverse().join('-'));
          const fromDate = dateRange.from;
          const toDate = dateRange.to || new Date(); // Use today if to is not set
          
          return reportDate >= fromDate && reportDate <= toDate;
        });
        
        setReports(filteredReports);
      } catch (error) {
        console.error("Error fetching reports:", error);
        setReports([]);
      } finally {
        setLoading(false);
      }
    }
    
    fetchReports();
  }, [dateRange]);

  // Підготувати дані для експорту
  const prepareExportData = (reportsToExport: Report[] = reports) => {
    return reportsToExport.map(report => {
      const task = report.tasks[0] || {};
      return {
        date: report.date,
        market: 'Європа',
        agency: 'MediaCom',
        client: report.companies.split(', ')[0] || '',
        project: 'N/A',
        media: 'Radio',
        jobType: task.description || '',
        comments: 'Автоматичний експорт',
        hours: report.totalHours
      };
    });
  }

  // Функція для створення Excel файлу зі звітом або звітами
  const createReportExcel = async (reportsToExport: Report[]) => {
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Звіти');
    
    // Додаємо заголовки
    worksheet.columns = [
      { header: 'Дата', key: 'date', width: 15 },
      { header: 'Ринок', key: 'market', width: 15 },
      { header: 'Агентство', key: 'agency', width: 20 },
      { header: 'Клієнт', key: 'client', width: 20 },
      { header: 'Проект/Бренд', key: 'project', width: 20 },
      { header: 'Медіа', key: 'media', width: 15 },
      { header: 'Тип роботи', key: 'jobType', width: 20 },
      { header: 'Коментарі', key: 'comments', width: 25 },
      { header: 'Години', key: 'hours', width: 10 }
    ];
    
    // Стилізуємо заголовки
    worksheet.getRow(1).font = { bold: true };
    worksheet.getRow(1).fill = {
      type: 'pattern',
      pattern: 'solid',
      fgColor: { argb: 'FFE6E6E6' }
    };
    
    // Додаємо дані зі звітів
    reportsToExport.forEach(report => {
      const task = report.tasks[0] || {};
      
      worksheet.addRow({
        date: report.date,
        market: 'Європа', // Припускаємо, що це за замовчуванням
        agency: 'MediaCom', // Припускаємо, що це за замовчуванням
        client: report.companies.split(', ')[0] || '',
        project: 'N/A',
        media: 'Radio', // Припускаємо, що це за замовчуванням
        jobType: task.description || '',
        comments: 'Автоматичний експорт', // Припускаємо, що це за замовчуванням
        hours: report.totalHours
      });
    });
    
    // Встановлюємо рамки для всіх клітинок
    worksheet.eachRow({ includeEmpty: false }, row => {
      row.eachCell({ includeEmpty: false }, cell => {
        cell.border = {
          top: { style: 'thin' },
          left: { style: 'thin' },
          bottom: { style: 'thin' },
          right: { style: 'thin' }
        };
      });
    });
    
    // Створюємо буфер з даними Excel
    return workbook.xlsx.writeBuffer();
  }

  // Функція для завантаження звіту
  const downloadReport = async (reportId: number) => {
    try {
      const report = reports.find(r => r.id === reportId);
      if (!report) {
        throw new Error(`Звіт з ID ${reportId} не знайдено`);
      }
      
      // Підготувати дані для модального вікна
      const exportData = prepareExportData([report]);
      setExportData(exportData);
      setShowExportModal(true);
    } catch (error) {
      console.error('Помилка при завантаженні звіту:', error);
      alert('Виникла помилка при створенні Excel файлу');
    }
  }

  // Функція для завантаження всіх звітів
  const downloadAllReports = async () => {
    try {
      if (reports.length === 0) {
        alert('Немає звітів для експорту');
        return;
      }
      
      // Підготувати дані для модального вікна
      const exportData = prepareExportData();
      setExportData(exportData);
      setShowExportModal(true);
    } catch (error) {
      console.error('Помилка при завантаженні всіх звітів:', error);
      alert('Виникла помилка при створенні Excel файлу');
    }
  }

  // Розрахунок загальної кількості годин
  const totalHours = reports.reduce((sum, report) => sum + report.totalHours, 0)

  if (loading) {
    return <div>Загрузка отчетов...</div>;
  }

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
            <div className="text-2xl font-bold">
              {reports.length ? (totalHours / reports.length).toFixed(1) : "0"} годин
            </div>
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
              <Table className="min-w-full table-fixed">
                <TableHeader>
                  <TableRow>
                    <TableHead>Дата</TableHead>
                    <TableHead>Години</TableHead>
                    <TableHead>Компанії</TableHead>
                    <TableHead>Дії</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {reports.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={4} className="text-center">Немає звітів за обраний період</TableCell>
                    </TableRow>
                  ) : (
                    reports.map((report) => (
                      <TableRow key={report.id}>
                        <TableCell className="font-medium">{report.date}</TableCell>
                        <TableCell>{report.totalHours}</TableCell>
                        <TableCell>{report.companies}</TableCell>
                        <TableCell>
                          <div className="flex gap-2">
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

                <Table className="min-w-full table-fixed">
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

      <ReportExportModal 
        isOpen={showExportModal}
        onClose={() => setShowExportModal(false)}
        reportData={exportData}
      />
    </div>
  )
}
