"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { Download } from "lucide-react"

interface Employee {
  id: number;
  name: string;
  email: string;
  position: string;
  department: string;
  status: string;
}

export function EmployeeOverview() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [dateRanges, setDateRanges] = useState<Record<number, { start: string, end: string }>>({});

  // Get today's date and one month ago for default date range
  const today = new Date();
  const oneMonthAgo = new Date();
  oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1);
  
  const formatDate = (date: Date) => {
    return date.toISOString().split('T')[0];
  };

  useEffect(() => {
    async function fetchEmployees() {
      try {
        const response = await fetch('/api/admin', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ action: 'getEmployees' }),
        });
        
        if (!response.ok) {
          throw new Error('Failed to fetch employees');
        }
        
        const data = await response.json();
        setEmployees(data.employees);
        
        // Initialize date ranges for all employees
        const ranges: Record<number, { start: string, end: string }> = {};
        data.employees.forEach((employee: Employee) => {
          ranges[employee.id] = {
            start: formatDate(oneMonthAgo),
            end: formatDate(today)
          };
        });
        setDateRanges(ranges);
        
      } catch (error) {
        console.error("Error fetching employees:", error);
      } finally {
        setLoading(false);
      }
    }
    
    fetchEmployees();
  }, []);
  
  const handleDateChange = (employeeId: number, field: 'start' | 'end', value: string) => {
    setDateRanges(prev => ({
      ...prev,
      [employeeId]: {
        ...prev[employeeId],
        [field]: value
      }
    }));
  };

  if (loading) {
    return <div>Loading employee data...</div>;
  }

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
            {employees.length === 0 ? (
              <TableRow>
                <TableCell colSpan={3} className="text-center">Немає доступних співробітників</TableCell>
              </TableRow>
            ) : (
              employees.map((employee) => (
                <TableRow key={employee.id} data-employee-id={employee.id}>
                  <TableCell className="font-medium">{employee.name}</TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <input 
                        type="date" 
                        value={dateRanges[employee.id]?.start || formatDate(oneMonthAgo)}
                        onChange={(e) => handleDateChange(employee.id, 'start', e.target.value)}
                        className="border rounded px-2 py-1 text-sm" 
                      />
                      <span>—</span>
                      <input 
                        type="date" 
                        value={dateRanges[employee.id]?.end || formatDate(today)}
                        onChange={(e) => handleDateChange(employee.id, 'end', e.target.value)}
                        className="border rounded px-2 py-1 text-sm" 
                      />
                    </div>
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="outline"
                      size="sm"
                      className="gap-2"
                      onClick={() => downloadEmployeeReport(
                        employee.id, 
                        dateRanges[employee.id]?.start || formatDate(oneMonthAgo),
                        dateRanges[employee.id]?.end || formatDate(today)
                      )}
                    >
                      <Download className="h-4 w-4" />
                      <span>Excel</span>
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  )
}

// Функція для завантаження звіту співробітника в Excel форматі
function downloadEmployeeReport(employeeId: number, startDate: string, endDate: string) {
  // В реальному додатку тут був би код для генерації та завантаження Excel файлу
  console.log(`Завантаження звіту для співробітника ID: ${employeeId} за період ${startDate} - ${endDate}`)
  alert(`Звіт для співробітника ID: ${employeeId} за період ${startDate} - ${endDate} завантажується...`)
}
