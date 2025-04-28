"use client"

import { useState, useEffect } from "react"
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
import { Pencil, Plus, Search, Trash, AlertCircle } from "lucide-react"

interface Employee {
  id: number;
  name: string;
  email: string;
  position: string;
  department: string;
  joinDate: string;
  status: string;
}

interface EditingEmployee extends Employee {}

export function EmployeesList() {
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState("")
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [employeeToDelete, setEmployeeToDelete] = useState<number | null>(null)
  const [editingEmployee, setEditingEmployee] = useState<EditingEmployee | null>(null)
  const [newEmployee, setNewEmployee] = useState({
    name: "",
    email: "",
    position: "",
    department: "",
    status: "Активний",
    password: "",
  })

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
        
        // Format the join date for each employee
        const formattedEmployees = data.employees.map((emp: any) => {
          // Convert ISO date to DD.MM.YYYY format
          const joinDate = emp.joinDate 
            ? new Date(emp.joinDate).toLocaleDateString('uk-UA', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
              })
            : 'N/A';
            
          return {
            ...emp,
            joinDate,
            // Set default status if not present
            status: emp.status || 'Активний'
          };
        });
        
        setEmployees(formattedEmployees);
      } catch (error) {
        console.error("Error fetching employees:", error);
      } finally {
        setLoading(false);
      }
    }
    
    fetchEmployees();
  }, []);

  // Фільтрація співробітників за пошуковим запитом
  const filteredEmployees = employees.filter(
    (employee) =>
      employee.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.position.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.department.toLowerCase().includes(searchTerm.toLowerCase()),
  )

  // Додавання нового співробітника
  const handleAddEmployee = async () => {
    try {
      // Use the combined endpoint for adding employee with Clerk user
      const response = await fetch('/api/admin', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          action: 'addEmployee',
          employee: newEmployee
        }),
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(`Failed to add employee: ${errorData.error || 'Unknown error'}`);
      }
      
      const result = await response.json();
      
      // Format today's date
      const today = new Date();
      const joinDate = today.toLocaleDateString('uk-UA', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });

      // Add the new employee to local state
      setEmployees([
        ...employees,
        {
          id: result.id,
          ...newEmployee,
          joinDate,
        },
      ]);

      // Reset form
      setNewEmployee({
        name: "",
        email: "",
        position: "",
        department: "",
        status: "Активний",
        password: "",
      });
      setIsAddDialogOpen(false);
    } catch (error) {
      console.error("Error adding employee:", error);
      alert("Failed to add employee. Please try again.");
    }
  }

  // Редагування співробітника
  const handleEditEmployee = async () => {
    if (!editingEmployee) return;
    
    try {
      const response = await fetch('/api/admin', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          action: 'updateEmployee',
          employee: editingEmployee
        }),
      });
      
      if (!response.ok) {
        throw new Error('Failed to update employee');
      }
      
      // Update local state
      setEmployees(employees.map((emp) => 
        emp.id === editingEmployee.id ? editingEmployee : emp
      ));
      
      setIsEditDialogOpen(false);
    } catch (error) {
      console.error("Error updating employee:", error);
      alert("Failed to update employee. Please try again.");
    }
  }

  // Видалення співробітника
  const handleDeleteEmployee = async (id: number) => {
    try {
      const response = await fetch('/api/admin', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          action: 'deleteEmployee',
          id
        }),
      });
      
      if (!response.ok) {
        throw new Error('Failed to delete employee');
      }
      
      // Remove from local state
      setEmployees(employees.filter((emp) => emp.id !== id));
      setIsDeleteDialogOpen(false);
      setEmployeeToDelete(null);
    } catch (error) {
      console.error("Error deleting employee:", error);
      alert("Failed to delete employee. Please try again.");
    }
  }

  // Open delete confirmation dialog
  const confirmDelete = (id: number) => {
    setEmployeeToDelete(id);
    setIsDeleteDialogOpen(true);
  }

  if (loading) {
    return <div>Loading employees...</div>;
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
                  <Label htmlFor="password">Пароль</Label>
                  <Input
                    id="password"
                    type="password"
                    value={newEmployee.password}
                    onChange={(e) => setNewEmployee({ ...newEmployee, password: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="position">Посада</Label>
                  <Input
                    id="position"
                    value={newEmployee.position}
                    onChange={(e) => setNewEmployee({ ...newEmployee, position: e.target.value })}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
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

      {/* Delete confirmation dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Видалити співробітника</DialogTitle>
            <DialogDescription>
              Ви впевнені, що хочете видалити цього співробітника? Ця дія не може бути скасована.
            </DialogDescription>
          </DialogHeader>
          <div className="flex items-center justify-center py-4">
            <div className="rounded-full bg-red-100 p-3">
              <AlertCircle className="h-6 w-6 text-red-600" />
            </div>
          </div>
          <DialogFooter className="flex space-x-2 sm:justify-center">
            <Button variant="outline" onClick={() => setIsDeleteDialogOpen(false)}>
              Скасувати
            </Button>
            <Button 
              variant="destructive" 
              onClick={() => employeeToDelete && handleDeleteEmployee(employeeToDelete)}
            >
              Видалити
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

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
                          <Dialog open={isEditDialogOpen && editingEmployee?.id === employee.id} onOpenChange={setIsEditDialogOpen}>
                            <DialogTrigger asChild>
                              <Button variant="ghost" size="icon" onClick={() => setEditingEmployee(employee)}>
                                <Pencil className="h-4 w-4" />
                              </Button>
                            </DialogTrigger>
                            {editingEmployee && editingEmployee.id === employee.id && (
                              <DialogContent className="sm:max-w-[525px]">
                                <DialogHeader>
                                  <DialogTitle>Редагувати співробітника</DialogTitle>
                                  <DialogDescription>Змініть інформацію про співробітника</DialogDescription>
                                </DialogHeader>
                                <div className="grid gap-4 py-4">
                                  <div className="grid grid-cols-2 gap-4">
                                    <div className="space-y-2">
                                      <Label htmlFor="edit-name">ПІБ</Label>
                                      <Input
                                        id="edit-name"
                                        value={editingEmployee.name}
                                        onChange={(e) => setEditingEmployee({ ...editingEmployee, name: e.target.value })}
                                      />
                                    </div>
                                    <div className="space-y-2">
                                      <Label htmlFor="edit-email">Email</Label>
                                      <Input
                                        id="edit-email"
                                        type="email"
                                        value={editingEmployee.email}
                                        onChange={(e) => setEditingEmployee({ ...editingEmployee, email: e.target.value })}
                                      />
                                    </div>
                                  </div>
                                  <div className="grid grid-cols-2 gap-4">
                                    <div className="space-y-2">
                                      <Label htmlFor="edit-position">Посада</Label>
                                      <Input
                                        id="edit-position"
                                        value={editingEmployee.position}
                                        onChange={(e) => setEditingEmployee({ ...editingEmployee, position: e.target.value })}
                                      />
                                    </div>
                                    <div className="space-y-2">
                                      <Label htmlFor="edit-department">Відділ</Label>
                                      <Select
                                        value={editingEmployee.department}
                                        onValueChange={(value) => setEditingEmployee({ ...editingEmployee, department: value })}
                                      >
                                        <SelectTrigger id="edit-department">
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
                                  <Button variant="outline" onClick={() => setIsEditDialogOpen(false)}>
                                    Скасувати
                                  </Button>
                                  <Button onClick={handleEditEmployee}>Зберегти</Button>
                                </DialogFooter>
                              </DialogContent>
                            )}
                          </Dialog>
                          <Button variant="ghost" size="icon" onClick={() => confirmDelete(employee.id)}>
                            <Trash className="h-4 w-4" />
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
