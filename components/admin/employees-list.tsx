"use client"

import { useState } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Pencil, Plus, Search, Trash, Download } from "lucide-react"

export function EmployeesList() {
  // Приклад даних співробітників
  const [employees, setEmployees] = useState([
    {
      id: 1,
      name: "Олексій Петров",
      email: "alex@example.com",
      position: "Старший дизайнер",
      department: "Дизайн",
      joinDate: "15.03.2023",
      status: "Активний",
    },
    {
      id: 2,
      name: "Олена Сидорова",
      email: "elena@example.com",
      position: "Копірайтер",
      department: "Маркетинг",
      joinDate: "02.06.2023",
      status: "Активна",
    },
    {
      id: 3,
      name: "Іван Смирнов",
      email: "ivan@example.com",
      position: "Веб-розробник",
      department: "Розробка",
      joinDate: "10.01.2024",
      status: "Активний",
    },
    {
      id: 4,
      name: "Марія Козлова",
      email: "maria@example.com",
      position: "Менеджер проєктів",
      department: "Управління",
      joinDate: "22.09.2022",
      status: "Активна",
    },
    {
      id: 5,
      name: "Дмитро Новіков",
      email: "dmitry@example.com",
      position: "SMM спеціаліст",
      department: "Маркетинг",
      joinDate: "05.04.2023",
      status: "Активний",
    },
  ])

  const [searchTerm, setSearchTerm] = useState("")
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [editingEmployee, setEditingEmployee] = useState(null)
  const [newEmployee, setNewEmployee] = useState({
    name: "",
    email: "",
    position: "",
    department: "",
    status: "Активний",
  })

  // Фільтрація співробітників за пошуковим запитом
  const filteredEmployees = employees.filter(
    (employee) =>
      employee.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.position.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.department.toLowerCase().includes(searchTerm.toLowerCase()),
  )

  // Додавання нового співробітника
  const handleAddEmployee = () => {
    const id = employees.length > 0 ? Math.max(...employees.map((e) => e.id)) + 1 : 1
    const today = new Date()
    const joinDate = `${today.getDate().toString().padStart(2, "0")}.${(today.getMonth() + 1)
      .toString()
      .padStart(2, "0")}.${today.getFullYear()}`

    setEmployees([
      ...employees,
      {
        id,
        ...newEmployee,
        joinDate,
      },
    ])

    setNewEmployee({
      name: "",
      email: "",
      position: "",
      department: "",
      status: "Активний",
    })
    setIsAddDialogOpen(false)
  }

  // Редагування співробітника
  const handleEditEmployee = () => {
    setEmployees(employees.map((emp) => (emp.id === editingEmployee.id ? editingEmployee : emp)))
    setIsEditDialogOpen(false)
  }

  // Видалення співробітника
  const handleDeleteEmployee = (id) => {
    if (confirm("Ви впевнені, що хочете видалити цього співробітника?")) {
      setEmployees(employees.filter((emp) => emp.id !== id))
    }
  }

  // Завантаження звіту по співробітнику
  const downloadEmployeeReport = (employeeId) => {
    console.log(`Завантаження звіту для співробітника ID: ${employeeId}`)
    alert(`Звіт для співробітника ID: ${employeeId} завантажується...`)
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Співробітники</h1>
          <p className="text-gray-500">Управління співробітниками компанії</p>
        </div>
        <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2">
              <Plus className="h-4 w-4" />
              Додати співробітника
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[525px]">
            <DialogHeader>
              <DialogTitle>Додати нового співробітника</DialogTitle>
              <DialogDescription>Введіть інформацію про нового співробітника</DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="name">ПІБ</Label>
                  <Input
                    id="name"
                    value={newEmployee.name}
                    onChange={(e) => setNewEmployee({ ...newEmployee, name: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    type="email"
                    value={newEmployee.email}
                    onChange={(e) => setNewEmployee({ ...newEmployee, email: e.target.value })}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="position">Посада</Label>
                  <Input
                    id="position"
                    value={newEmployee.position}
                    onChange={(e) => setNewEmployee({ ...newEmployee, position: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="department">Відділ</Label>
                  <Select
                    value={newEmployee.department}
                    onValueChange={(value) => setNewEmployee({ ...newEmployee, department: value })}
                  >
                    <SelectTrigger id="department">
                      <SelectValue placeholder="Оберіть відділ" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Дизайн">Дизайн</SelectItem>
                      <SelectItem value="Розробка">Розробка</SelectItem>
                      <SelectItem value="Маркетинг">Маркетинг</SelectItem>
                      <SelectItem value="Управління">Управління</SelectItem>
                      <SelectItem value="Продажі">Продажі</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                Скасувати
              </Button>
              <Button onClick={handleAddEmployee}>Додати</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-col md:flex-row justify-between md:items-center gap-4">
            <div>
              <CardTitle>Список співробітників</CardTitle>
              <CardDescription>Всього співробітників: {employees.length}</CardDescription>
            </div>
            <div className="relative w-full md:w-64">
              <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-gray-500" />
              <Input
                type="search"
                placeholder="Пошук співробітників..."
                className="pl-8"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Співробітник</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredEmployees.length > 0 ? (
                filteredEmployees.map((employee) => (
                  <TableRow key={employee.id}>
                    <TableCell>
                      <div className="flex items-center justify-between">
                        <div>
                          <div className="font-medium">{employee.name}</div>
                          <div className="text-sm text-gray-500">{employee.email}</div>
                          <div className="text-xs text-gray-400 mt-1">
                            {employee.position}, {employee.department}
                          </div>
                        </div>
                        <div className="flex gap-2">
                          <Button variant="ghost" size="icon" onClick={() => setEditingEmployee(employee)}>
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => handleDeleteEmployee(employee.id)}>
                            <Trash className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => downloadEmployeeReport(employee.id)}>
                            <Download className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={1} className="text-center py-8 text-gray-500">
                    {searchTerm ? "Співробітників не знайдено" : "Немає співробітників"}
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  )
}
