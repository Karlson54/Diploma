"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { Download } from "lucide-react"
import Link from "next/link"
import { useTranslation } from "react-i18next"

interface ReportWithDetails {
  id: number;
  employee: string;
  date: string;
  hours: number;
  companies: string;
}

export function RecentReports() {
  const { t } = useTranslation()
  const [reports, setReports] = useState<ReportWithDetails[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchReports() {
      try {
        const response = await fetch('/api/admin', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ action: 'getReports' }),
        });
        
        if (!response.ok) {
          throw new Error('Failed to fetch reports');
        }
        
        const data = await response.json();
        
        // Transform data and sort by date (most recent first)
        const formattedReports = data.reports
          .map((reportData: any) => {
            // Format date to DD.MM.YYYY
            const reportDate = new Date(reportData.report.date);
            const formattedDate = reportDate.toLocaleDateString('uk-UA', {
              day: '2-digit',
              month: '2-digit',
              year: 'numeric'
            });
            
            // Get companies involved in this report
            const companies = [
              reportData.report.client,
              reportData.report.contractingAgency
            ].filter(Boolean).join(", ");
            
            return {
              id: reportData.report.id,
              employee: reportData.employee?.name || 'Unknown',
              date: formattedDate,
              hours: reportData.report.hours,
              companies: companies
            };
          })
          .sort((a: ReportWithDetails, b: ReportWithDetails) => {
            // Sort by date (most recent first)
            const dateA = a.date.split('.').reverse().join('-');
            const dateB = b.date.split('.').reverse().join('-');
            return dateB.localeCompare(dateA);
          })
          // Only take the most recent reports
          .slice(0, 7);
        
        setReports(formattedReports);
      } catch (error) {
        console.error("Error fetching reports:", error);
      } finally {
        setLoading(false);
      }
    }
    
    fetchReports();
  }, []);

  // Function to download report
  const downloadReport = (reportId: number) => {
    console.log(`Downloading report ID: ${reportId}`)
    alert(t('admin.recentReports.downloadMessage', { reportId }))
  }

  if (loading) {
    return <div>{t('admin.recentReports.loading')}</div>;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t('admin.recentReports.title')}</CardTitle>
        <CardDescription>{t('admin.recentReports.description')}</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('admin.recentReports.employee')}</TableHead>
              <TableHead>{t('admin.recentReports.date')}</TableHead>
              <TableHead>{t('admin.recentReports.hours')}</TableHead>
              <TableHead>{t('admin.recentReports.companies')}</TableHead>
              <TableHead>{t('admin.recentReports.actions')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {reports.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} className="text-center">{t('admin.recentReports.noReports')}</TableCell>
              </TableRow>
            ) : (
              reports.map((report) => (
                <TableRow key={report.id}>
                  <TableCell className="font-medium">{report.employee}</TableCell>
                  <TableCell>{report.date}</TableCell>
                  <TableCell>{report.hours}</TableCell>
                  <TableCell>{report.companies}</TableCell>
                  <TableCell>
                    <Button variant="ghost" size="icon" onClick={() => downloadReport(report.id)}>
                      <Download className="h-4 w-4" />
                      <span className="sr-only">{t('admin.recentReports.download')}</span>
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </CardContent>
      <CardFooter>
        <Link href="/admin/reports">
          <Button variant="outline">{t('admin.recentReports.allReports')}</Button>
        </Link>
      </CardFooter>
    </Card>
  )
}
