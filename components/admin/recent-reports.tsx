"use client"

import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { Download } from "lucide-react"
import Link from "next/link"

export function RecentReports() {
  // Приклад даних звітів
  const reports = [
    {
      id: 1,
      employee: "Олексій Петров",
      date: "16.04.2025",
      hours: 8.5,
      companies: "Acme Inc, Tech Solutions",
    },
    {
      id: 2,
      employee: "Олена Сидорова",
      date: "16.04.2025",
      hours: 7.0,
      companies: "Globex Corp",
    },
    {
      id: 3,
      employee: "Іван Смирнов",
      date: "16.04.2025",
      hours: 8.0,
      companies: "Tech Solutions",
    },
    {
      id: 4,
      employee: "Марія Козлова",
      date: "16.04.2025",
      hours: 9.0,
      companies: "Stark Industries",
    },
    {
      id: 5,
      employee: "Дмитро Новіков",
      date: "16.04.2025",
      hours: 7.5,
      companies: "Wayne Enterprises",
    },
    {
      id: 6,
      employee: "Олексій Петров",
      date: "15.04.2025",
      hours: 8.0,
      companies: "Acme Inc",
    },
    {
      id: 7,
      employee: "Олена Сидорова",
      date: "15.04.2025",
      hours: 7.5,
      companies: "Globex Corp",
    },
  ]

  // Функція для завантаження звіту
  const downloadReport = (reportId) => {
    console.log(`Завантаження звіту ID: ${reportId}`)
    alert(`Звіт ID: ${reportId} завантажується у форматі Excel...`)
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Останні звіти</CardTitle>
        <CardDescription>Нещодавно додані записи обліку часу</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Співробітник</TableHead>
              <TableHead>Дата</TableHead>
              <TableHead>Години</TableHead>
              <TableHead>Компанії</TableHead>
              <TableHead>Дії</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {reports.map((report) => (
              <TableRow key={report.id}>
                <TableCell className="font-medium">{report.employee}</TableCell>
                <TableCell>{report.date}</TableCell>
                <TableCell>{report.hours}</TableCell>
                <TableCell>{report.companies}</TableCell>
                <TableCell>
                  <Button variant="ghost" size="icon" onClick={() => downloadReport(report.id)}>
                    <Download className="h-4 w-4" />
                    <span className="sr-only">Завантажити</span>
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
      <CardFooter>
        <Link href="/admin/reports">
          <Button variant="outline">Всі звіти</Button>
        </Link>
      </CardFooter>
    </Card>
  )
}
