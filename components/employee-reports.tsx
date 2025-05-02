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
import { useTranslation } from "react-i18next"

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
  employeeName?: string;
  projectBrand?: string;
  market?: string;
  agency?: string;
  media?: string;
  comments?: string;
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
    projectBrand?: string;
    market?: string;
    comments?: string;
    media?: string;
  };
  employee: {
    id: number;
    name: string;
    agency: string;
  };
  companies?: any[]; // Add companies array from enhanced API response
}

export function EmployeeReports() {
  const { t } = useTranslation()
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
        // Fetch from API endpoint with the current user filter
        const response = await fetch('/api/reports?currentUserOnly=true');
        if (!response.ok) {
          throw new Error('Failed to fetch reports');
        }
        const data = await response.json();
        
        // Check if the response contains a reports array
        const allReports: ApiReport[] = Array.isArray(data) ? data : (data.reports || []);
        
        // Ensure allReports is an array before using map
        if (!Array.isArray(allReports)) {
          console.error("API returned non-array data:", allReports);
          setReports([]);
          return;
        }
        
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
          
          // Extract employee name with fallback
          const employeeName = report.employee?.name || '-';
          
          // Directly extract fields from report
          const agency = report.employee?.agency || '-';
          const market = report.report.market || '-';
          const media = report.report.media || '-';
          const comments = report.report.comments || '-';
          const projectBrand = report.report.projectBrand || '-';
          
          // Log to debug the data extraction
          console.log('Extracted data from API:', {
            agency, market, media, comments, projectBrand, employeeName
          });
          
          // Create task based on actual report data
          const tasks: ReportTask[] = [];
          if (report.report.client) {
            tasks.push({
              company: report.report.client,
              description: report.report.jobType || "Work on project",
              hours: report.report.hours
            });
          }
          
          // Add company information if available
          if (report.companies && Array.isArray(report.companies) && report.companies.length > 0) {
            // Add additional tasks from associated companies
            report.companies.forEach(company => {
              if (company && company.name && !tasks.some(t => t.company === company.name)) {
                tasks.push({
                  company: company.name,
                  description: report.report.jobType || `Work with ${company.name}`,
                  hours: report.report.hours / report.companies!.length // Distribute hours
                });
              }
            });
          }
          
          return {
            id: report.report.id,
            date: formattedDate,
            totalHours: report.report.hours,
            companies,
            tasks,
            employeeName,
            projectBrand,
            market,
            agency,
            media,
            comments
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

  console.log("Report data to export:", reports);

  // Підготувати дані для експорту
  const prepareExportData = (reportsToExport: Report[] = reports) => {
    // Проверка данных для отладки
    console.log("Preparing export data from:", reportsToExport);
    
    return reportsToExport.map(report => {
      const task = report.tasks[0] || {};
      
      // Split companies string to extract client and contracting agency
      const companiesParts = report.companies.split(', ');
      const client = companiesParts[0] || '';
      const contractingAgency = companiesParts[1] || '';
      
      // Данные для отладки
      console.log("Raw report data:", report);
      
      const exportRow = {
        // Используем данные непосредственно из отчета/БД
        date: report.date || '-',
        // Пытаемся получить рынок из отчета, если нет - используем дефолтное значение
        market: report.market || '-',
        // Агентство сотрудника из отчета
        agency: report.agency || '-',
        // Полное имя сотрудника
        fullName: report.employeeName || '-',
        // Компания-подрядчик (Contracting Agency / Unit)
        company: contractingAgency || '-',
        // Клиент
        client: client || '-',
        // Проект из описания задачи
        project: task.description || '-',
        // Бренд проекта
        projectBrand: report.projectBrand || task.company || '-',
        // Тип медиа из отчета
        media: report.media || '-',
        // Тип работы из задачи
        jobType: task.description || '-',
        // Комментарии из отчета
        comments: report.comments || '-',
        // Часы из отчета
        hours: report.totalHours
      };
      
      // Для отладки
      console.log("Prepared export row:", exportRow);
      
      return exportRow;
    });
  }

  // Функція для створення Excel файлу зі звітом або звітами
  const createReportExcel = async (reportsToExport: Report[]) => {
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Звіти');
    
    // Додаємо заголовки
    worksheet.columns = [
      { header: 'Agency', key: 'agency', width: 15 },
      { header: 'Name', key: 'fullName', width: 20 },
      { header: 'Date', key: 'date', width: 15 },
      { header: 'Market', key: 'market', width: 15 },
      { header: 'Company', key: 'company', width: 20 },
      { header: 'Client', key: 'client', width: 20 },
      { header: 'Project / brand', key: 'project', width: 20 },
      { header: 'Media', key: 'media', width: 15 },
      { header: 'Job type', key: 'jobType', width: 20 },
      { header: 'Hours', key: 'hours', width: 10 },
      { header: 'Comments', key: 'comments', width: 25 }
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
      // Use the same data preparation logic as in prepareExportData
      const task = report.tasks[0] || {};
      
      // Split companies string to extract client and contracting agency
      const companiesParts = report.companies.split(', ');
      const client = companiesParts[0] || '';
      const contractingAgency = companiesParts[1] || '';
      
      console.log("Excel export - report data:", report, "contractingAgency:", contractingAgency);
      
      worksheet.addRow({
        agency: report.agency || '-',
        fullName: report.employeeName || '-',
        date: report.date || '-',
        market: report.market || '-',
        company: contractingAgency || '-',
        client: client || '-',
        project: report.projectBrand || task.company || '-',
        media: report.media || '-',
        jobType: task.description || '-',
        hours: report.totalHours,
        comments: report.comments || '-'
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
      
      // Подготовка данных для экспорта с использованием данных из базы
      const processedReports = reports.map(report => {
        const task = report.tasks[0] || {};
        
        // Split companies string to extract client and contracting agency
        const companiesParts = report.companies.split(', ');
        const client = companiesParts[0] || '';
        const contractingAgency = companiesParts[1] || '';
        
        console.log("Download all reports - report data:", report, "contractingAgency:", contractingAgency);
        
        return {
          date: report.date || '-',
          market: report.market || '-',
          agency: report.agency || '-',
          fullName: report.employeeName || '-',
          company: contractingAgency || '-',
          client: client || '-',
          project: task.description || '-',
          projectBrand: report.projectBrand || task.company || '-',
          media: report.media || '-',
          jobType: task.description || '-',
          comments: report.comments || '-',
          hours: report.totalHours
        };
      });
      
      console.log("Final export data:", processedReports);
      
      // Подготовленные данные в модальное окно
      setExportData(processedReports);
      setShowExportModal(true);
    } catch (error) {
      console.error('Помилка при завантаженні всіх звітів:', error);
      alert('Виникла помилка при створенні Excel файлу');
    }
  }

  // Розрахунок загальної кількості годин
  const totalHours = reports.reduce((sum, report) => sum + report.totalHours, 0)

  if (loading) {
    return <div>{t('admin.reports.loadingReports')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('calendar.menu.myReports')}</h1>
          <p className="text-gray-500">{t('admin.reports.description')}</p>
        </div>
        <Button onClick={downloadAllReports} className="gap-2">
          <FileSpreadsheet className="h-4 w-4" />
          <span>{t('admin.reports.exportAllToExcel')}</span>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t('admin.reports.filters.title')}</CardTitle>
          <CardDescription>{t('admin.reports.filters.description')}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="col-span-1 md:col-span-2">
              <label className="text-sm font-medium mb-2 block">{t('admin.reports.filters.period')}</label>
              <DatePickerWithRange date={dateRange} setDate={setDateRange} />
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">{t('admin.reports.summary.totalHoursTitle')}</CardTitle>
            <CardDescription>{t('admin.reports.summary.period')}</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalHours} {t('calendar.hours')}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">{t('admin.reports.summary.reportsCountTitle')}</CardTitle>
            <CardDescription>{t('admin.reports.summary.period')}</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{reports.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">{t('admin.reports.summary.avgTimePerDay')}</CardTitle>
            <CardDescription>{t('admin.reports.summary.period')}</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {reports.length ? (totalHours / reports.length).toFixed(1) : "0"} {t('calendar.hours')}
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="summary">
        <TabsList>
          <TabsTrigger value="summary">{t('admin.reports.summary.summaryTitle')}</TabsTrigger>
          <TabsTrigger value="detailed">{t('admin.reports.detailed.title')}</TabsTrigger>
        </TabsList>
        <TabsContent value="summary" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle>{t('admin.reports.summary.summaryTitle')}</CardTitle>
              <CardDescription>{t('admin.reports.summary.summaryDescription')}</CardDescription>
            </CardHeader>
            <CardContent>
              <Table className="min-w-full table-fixed">
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Hours</TableHead>
                    <TableHead>Companies</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {reports.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={4} className="text-center">{t('admin.reports.summary.noReportsAvailable')}</TableCell>
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
                              <span className="sr-only">{t('admin.reports.downloadDialog.download')}</span>
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
              <CardTitle>{t('admin.reports.detailed.title')}</CardTitle>
              <CardDescription>{t('admin.reports.detailed.description')}</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                <div className="border rounded-lg p-4">
                  <h3 className="font-medium mb-2">{t('admin.reports.detailed.timeDistribution')}</h3>
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
                      <TableHead>Date</TableHead>
                      <TableHead>Company</TableHead>
                      <TableHead>Job Type</TableHead>
                      <TableHead>Hours</TableHead>
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
                    {t('admin.reports.detailed.exportToExcel')}
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
