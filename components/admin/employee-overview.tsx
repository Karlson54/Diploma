"use client"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { Download } from "lucide-react"

export function EmployeeOverview() {
  // Приклад даних співробітників
  const employees = [
    {
      id: 1,
      name: "Олексій Петров",
      position: "Старший дизайнер",
      projects: 4,
      hoursThisWeek: 38,
      efficiency: 92,
      status: "Активний",
    },
    {
      id: 2,
      name: "Олена Сидорова",
      position: "Копірайтер",
      projects: 6,
      hoursThisWeek: 32,
      efficiency: 85,
      status: "Активна",
    },
    {
      id: 3,
      name: "Іван Смирнов",
      position: "Веб-розробник",
      projects: 3,
      hoursThisWeek: 40,
      efficiency: 78,
      status: "Активний",
    },
    {
      id: 4,
      name: "Марія Козлова",
      position: "Менеджер проєктів",
      projects: 8,
      hoursThisWeek: 42,
      efficiency: 88,
      status: "Активна",
    },
    {
      id: 5,
      name: "Дмитро Новіков",
      position: "SMM спеціаліст",
      projects: 5,
      hoursThisWeek: 36,
      efficiency: 76,
      status: "Активний",
    },
  ]

  return (
    <Card>
      <CardHeader>
        <CardTitle>Огляд співробітників</CardTitle>
        <CardDescription>Управління та моніторинг активності співробітників</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Співробітник</TableHead>
              <TableHead>Період звіту</TableHead>
              <TableHead>Дії</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {employees.map((employee) => (
              <TableRow key={employee.id} data-employee-id={employee.id}>
                <TableCell className="font-medium">{employee.name}</TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <input type="date" defaultValue="2025-04-01" className="border rounded px-2 py-1 text-sm" />
                    <span>—</span>
                    <input type="date" defaultValue="2025-04-15" className="border rounded px-2 py-1 text-sm" />
                  </div>
                </TableCell>
                <TableCell>
                  <Button
                    variant="outline"
                    size="sm"
                    className="gap-2"
                    onClick={() => downloadEmployeeReport(employee.id)}
                  >
                    <Download className="h-4 w-4" />
                    <span>Excel</span>
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  )
}

// Функція для завантаження звіту співробітника в Excel форматі
function downloadEmployeeReport(employeeId: number) {
  // Отримуємо вибрані дати для співробітника
  const row = document.querySelector(`tr[data-employee-id="${employeeId}"]`)
  const startDate = row?.querySelector('input[type="date"]:first-of-type')?.value || "2025-04-01"
  const endDate = row?.querySelector('input[type="date"]:last-of-type')?.value || "2025-04-15"

  // В реальному додатку тут був би код для генерації та завантаження Excel файлу
  console.log(`Завантаження звіту для співробітника ID: ${employeeId} за період ${startDate} - ${endDate}`)
  alert(`Звіт для співробітника ID: ${employeeId} за період ${startDate} - ${endDate} завантажується...`)
}
