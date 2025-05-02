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
import { useTranslation } from "react-i18next"

interface Employee {
  id: number;
  name: string;
  email: string;
  position: string;
  department: string;
  joinDate: string;
  status: string;
  isAdmin?: boolean;
  agency?: string;
}

interface EditingEmployee extends Employee {
  password?: string;
}

export function EmployeesList() {
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState("")
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [employeeToDelete, setEmployeeToDelete] = useState<number | null>(null)
  const [editingEmployee, setEditingEmployee] = useState<EditingEmployee | null>(null)
  const [isUpdating, setIsUpdating] = useState<number | null>(null)
  const [updateMessage, setUpdateMessage] = useState<string | null>(null)
  const { t } = useTranslation()
  const [newEmployee, setNewEmployee] = useState({
    name: "",
    email: "",
    position: "",
    department: "",
    status: t('admin.employees.status.active'),
    password: "",
    agency: "",
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
            status: emp.status || 'Активний',
            // Check if user has admin role
            isAdmin: emp.isAdmin || false
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
        status: t('admin.employees.status.active'),
        password: "",
        agency: "",
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
      setIsUpdating(editingEmployee.id);
      setUpdateMessage(null);
      
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
      
      const result = await response.json();
      
      if (!response.ok) {
        throw new Error(result.error || 'Failed to update employee');
      }
      
      // Update local state
      setEmployees(employees.map((emp) => 
        emp.id === editingEmployee.id ? editingEmployee : emp
      ));
      
      // Show success message
      setUpdateMessage(result.message || 'Employee updated successfully');
      
      // Close the dialog after a short delay
      setTimeout(() => {
        setIsEditDialogOpen(false);
        setUpdateMessage(null);
      }, 1500);
    } catch (error) {
      console.error("Error updating employee:", error);
      setUpdateMessage('Failed to update employee. Please try again.');
    } finally {
      setIsUpdating(null);
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
    return <div>{t('admin.employees.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('admin.employees.title')}</h1>
          <p className="text-gray-500">{t('admin.employees.description')}</p>
        </div>
        <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2">
              <Plus className="h-4 w-4" />
              {t('admin.employees.addEmployee')}
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[525px]">
            <DialogHeader>
              <DialogTitle>{t('admin.employees.add.title')}</DialogTitle>
              <DialogDescription>{t('admin.employees.add.description')}</DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="name">{t('admin.employees.add.name')}</Label>
                  <Input
                    id="name"
                    value={newEmployee.name}
                    onChange={(e) => setNewEmployee({ ...newEmployee, name: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="email">{t('admin.employees.add.email')}</Label>
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
                  <Label htmlFor="password">{t('admin.employees.add.password')}</Label>
                  <Input
                    id="password"
                    type="password"
                    value={newEmployee.password}
                    onChange={(e) => setNewEmployee({ ...newEmployee, password: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="position">{t('admin.employees.add.position')}</Label>
                  <Select
                    value={newEmployee.position}
                    onValueChange={(value) => setNewEmployee({ ...newEmployee, position: value })}
                  >
                    <SelectTrigger id="position">
                      <SelectValue placeholder={t('admin.employees.add.selectPosition')} />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Бухгалтер">{t('admin.employees.positions.accountant')}</SelectItem>
                      <SelectItem value="Головний бухгалтер">{t('admin.employees.positions.chiefAccountant')}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="department">{t('admin.employees.add.department')}</Label>
                  <Select
                    value={newEmployee.department}
                    onValueChange={(value) => setNewEmployee({ ...newEmployee, department: value })}
                  >
                    <SelectTrigger id="department">
                      <SelectValue placeholder={t('admin.employees.add.selectDepartment')} />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Бухгалтерія">{t('admin.employees.departments.accounting')}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="agency">{t('admin.employees.add.agency')}</Label>
                  <Select
                    value={newEmployee.agency}
                    onValueChange={(value) => setNewEmployee({ ...newEmployee, agency: value })}
                  >
                    <SelectTrigger id="agency">
                      <SelectValue placeholder={t('admin.employees.add.selectAgency')} />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="GroupM">GroupM</SelectItem>
                      <SelectItem value="MediaCom">MediaCom</SelectItem>
                      <SelectItem value="Mindshare">Mindshare</SelectItem>
                      <SelectItem value="Wavemaker">Wavemaker</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                {t('admin.employees.add.cancel')}
              </Button>
              <Button onClick={handleAddEmployee}>{t('admin.employees.add.add')}</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      {/* Delete confirmation dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>{t('admin.employees.delete.title')}</DialogTitle>
            <DialogDescription>
              {t('admin.employees.delete.confirmation')}
            </DialogDescription>
          </DialogHeader>
          <div className="flex items-center justify-center py-4">
            <div className="rounded-full bg-red-100 p-3">
              <AlertCircle className="h-6 w-6 text-red-600" />
            </div>
          </div>
          <DialogFooter className="flex space-x-2 sm:justify-center">
            <Button variant="outline" onClick={() => setIsDeleteDialogOpen(false)}>
              {t('admin.employees.delete.cancel')}
            </Button>
            <Button 
              variant="destructive" 
              onClick={() => employeeToDelete && handleDeleteEmployee(employeeToDelete)}
            >
              {t('admin.employees.delete.delete')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-col md:flex-row justify-between md:items-center gap-4">
            <div>
              <CardTitle>{t('admin.employees.list.title')}</CardTitle>
              <CardDescription>{t('admin.employees.list.description', { count: employees.length })}</CardDescription>
            </div>
            <div className="relative w-full md:w-64">
              <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-gray-500" />
              <Input
                type="search"
                placeholder={t('admin.employees.list.searchPlaceholder')}
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
                <TableHead>{t('admin.employees.tableHeaders.employee')}</TableHead>
                <TableHead>{t('admin.employees.tableHeaders.email')}</TableHead>
                <TableHead>{t('admin.employees.tableHeaders.position')}</TableHead>
                <TableHead>{t('admin.employees.tableHeaders.department')}</TableHead>
                <TableHead>{t('admin.employees.tableHeaders.agency')}</TableHead>
                <TableHead>{t('admin.employees.tableHeaders.status')}</TableHead>
                <TableHead className="text-right">{t('admin.employees.tableHeaders.actions')}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredEmployees.length > 0 ? (
                filteredEmployees.map((employee) => (
                  <TableRow key={employee.id}>
                    <TableCell>
                      <div className="font-medium">{employee.name}</div>
                    </TableCell>
                    <TableCell>{employee.email}</TableCell>
                    <TableCell>{employee.position}</TableCell>
                    <TableCell>{employee.department}</TableCell>
                    <TableCell>{employee.agency || '-'}</TableCell>
                    <TableCell>
                      <div className="flex items-center">
                        <div className={`w-2 h-2 rounded-full mr-2 ${
                          employee.status === 'active' ? 'bg-green-500' : 'bg-gray-300'
                        }`} />
                        {employee.isAdmin && (
                          <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-indigo-100 text-indigo-800">
                            {t('admin.employees.roles.admin')}
                          </span>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Dialog open={isEditDialogOpen && editingEmployee?.id === employee.id} onOpenChange={setIsEditDialogOpen}>
                          <DialogTrigger asChild>
                            <Button variant="ghost" size="icon" onClick={() => setEditingEmployee(employee)}>
                              <Pencil className="h-4 w-4" />
                            </Button>
                          </DialogTrigger>
                          {editingEmployee && editingEmployee.id === employee.id && (
                            <DialogContent className="sm:max-w-[525px]">
                              <DialogHeader>
                                <DialogTitle>{t('admin.employees.edit.title')}</DialogTitle>
                                <DialogDescription>{t('admin.employees.edit.description')}</DialogDescription>
                              </DialogHeader>
                              <div className="grid gap-4 py-4">
                                <div className="grid grid-cols-2 gap-4">
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-name">{t('admin.employees.edit.name')}</Label>
                                    <Input
                                      id="edit-name"
                                      value={editingEmployee.name}
                                      onChange={(e) => setEditingEmployee({ ...editingEmployee, name: e.target.value })}
                                    />
                                  </div>
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-email">{t('admin.employees.edit.email')}</Label>
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
                                    <Label htmlFor="edit-position">{t('admin.employees.edit.position')}</Label>
                                    <Select
                                      value={editingEmployee.position}
                                      onValueChange={(value) => setEditingEmployee({ ...editingEmployee, position: value })}
                                    >
                                      <SelectTrigger id="edit-position">
                                        <SelectValue placeholder={t('admin.employees.edit.selectPosition')} />
                                      </SelectTrigger>
                                      <SelectContent>
                                        <SelectItem value="Бухгалтер">{t('admin.employees.positions.accountant')}</SelectItem>
                                        <SelectItem value="Головний бухгалтер">{t('admin.employees.positions.chiefAccountant')}</SelectItem>
                                      </SelectContent>
                                    </Select>
                                  </div>
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-department">{t('admin.employees.edit.department')}</Label>
                                    <Select
                                      value={editingEmployee.department}
                                      onValueChange={(value) => setEditingEmployee({ ...editingEmployee, department: value })}
                                    >
                                      <SelectTrigger id="edit-department">
                                        <SelectValue placeholder={t('admin.employees.edit.selectDepartment')} />
                                      </SelectTrigger>
                                      <SelectContent>
                                        <SelectItem value="Бухгалтерія">{t('admin.employees.departments.accounting')}</SelectItem>
                                      </SelectContent>
                                    </Select>
                                  </div>
                                </div>
                                <div className="space-y-2 mt-2">
                                  <Label htmlFor="edit-password">{t('admin.employees.edit.password')}</Label>
                                  <Input
                                    id="edit-password"
                                    type="password"
                                    placeholder={t('admin.employees.edit.passwordPlaceholder')}
                                    value={editingEmployee.password || ''}
                                    onChange={(e) => setEditingEmployee({ ...editingEmployee, password: e.target.value })}
                                  />
                                  <p className="text-sm text-gray-500">
                                    {t('admin.employees.edit.passwordNote')}
                                  </p>
                                </div>
                                <div className="space-y-2 mt-2">
                                  <Label htmlFor="edit-agency">{t('admin.employees.edit.agency')}</Label>
                                  <Select
                                    value={editingEmployee.agency || ''}
                                    onValueChange={(value) => setEditingEmployee({ ...editingEmployee, agency: value })}
                                  >
                                    <SelectTrigger id="edit-agency">
                                      <SelectValue placeholder={t('admin.employees.edit.selectAgency')} />
                                    </SelectTrigger>
                                    <SelectContent>
                                      <SelectItem value="GroupM">GroupM</SelectItem>
                                      <SelectItem value="MediaCom">MediaCom</SelectItem>
                                      <SelectItem value="Mindshare">Mindshare</SelectItem>
                                      <SelectItem value="Wavemaker">Wavemaker</SelectItem>
                                    </SelectContent>
                                  </Select>
                                </div>
                                <div className="space-y-2 mt-4">
                                  <div className="flex items-center space-x-2">
                                    <input
                                      type="checkbox"
                                      id="edit-admin-role"
                                      className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
                                      checked={editingEmployee.isAdmin}
                                      onChange={(e) => setEditingEmployee({ 
                                        ...editingEmployee, 
                                        isAdmin: e.target.checked 
                                      })}
                                    />
                                    <Label htmlFor="edit-admin-role" className="font-medium">
                                      {t('admin.employees.edit.adminRole')}
                                    </Label>
                                  </div>
                                  <p className="text-sm text-gray-500">
                                    {t('admin.employees.edit.adminRoleDescription')}
                                  </p>
                                </div>
                              </div>
                              <DialogFooter>
                                <Button variant="outline" onClick={() => setIsEditDialogOpen(false)}>
                                  {t('admin.employees.edit.cancel')}
                                </Button>
                                <Button 
                                  onClick={handleEditEmployee}
                                  disabled={isUpdating === editingEmployee?.id}
                                >
                                  {isUpdating === editingEmployee?.id ? (
                                    <span className="flex items-center gap-2">
                                      <span className="animate-spin h-4 w-4 border-2 border-white rounded-full border-t-transparent" />
                                      {t('admin.employees.edit.updating')}
                                    </span>
                                  ) : (
                                    t('admin.employees.edit.save')
                                  )}
                                </Button>
                              </DialogFooter>
                              {updateMessage && (
                                <div className={`mt-2 p-2 text-center rounded ${updateMessage.includes('Failed') ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'}`}>
                                  {updateMessage}
                                </div>
                              )}
                            </DialogContent>
                          )}
                        </Dialog>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => confirmDelete(employee.id)}
                        >
                          <Trash className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={7} className="h-24 text-center">
                    {t('admin.employees.list.noEmployees')}
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
