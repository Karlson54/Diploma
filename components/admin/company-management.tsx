"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Pencil, Trash, Plus } from "lucide-react"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Textarea } from "@/components/ui/textarea"

interface Company {
  id: number;
  name: string;
  contact: string;
  email: string;
  phone: string;
  projects?: number;
  address?: string | null;
  notes?: string | null;
}

interface EditingCompany extends Company {}

export function CompanyManagement() {
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)

  const [newCompany, setNewCompany] = useState<Omit<Company, 'id'>>({
    name: "",
    contact: "",
    email: "",
    phone: "",
    address: "",
    notes: "",
  })

  const [editingCompany, setEditingCompany] = useState<EditingCompany | null>(null)
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)

  useEffect(() => {
    async function fetchCompanies() {
      try {
        const response = await fetch('/api/admin', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ action: 'getCompanies' }),
        });
        
        if (!response.ok) {
          throw new Error('Failed to fetch companies');
        }
        
        const data = await response.json();
        setCompanies(data.companies);
      } catch (error) {
        console.error("Error fetching companies:", error);
      } finally {
        setLoading(false);
      }
    }
    
    fetchCompanies();
  }, []);

  const handleAddCompany = async () => {
    try {
      const response = await fetch('/api/admin', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ 
          action: 'addCompany', 
          company: newCompany 
        }),
      });
      
      if (!response.ok) {
        throw new Error('Failed to add company');
      }
      
      const result = await response.json();
      
      // Add the new company with the returned ID to the local state
      setCompanies([
        ...companies,
        {
          id: result.id,
          ...newCompany,
          projects: 0,
        },
      ]);
      
      // Reset form
      setNewCompany({
        name: "",
        contact: "",
        email: "",
        phone: "",
        address: "",
        notes: "",
      });
      setIsAddDialogOpen(false);
    } catch (error) {
      console.error("Error adding company:", error);
      alert("Failed to add company. Please try again.");
    }
  }

  const handleEditCompany = async () => {
    if (!editingCompany) return;
    
    try {
      const response = await fetch('/api/admin', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ 
          action: 'updateCompany', 
          company: editingCompany 
        }),
      });
      
      if (!response.ok) {
        throw new Error('Failed to update company');
      }
      
      // Update the local state with edited company
      setCompanies(companies.map((company) => 
        company.id === editingCompany.id ? { ...editingCompany } : company
      ));
      
      setIsEditDialogOpen(false);
    } catch (error) {
      console.error("Error updating company:", error);
      alert("Failed to update company. Please try again.");
    }
  }

  const handleDeleteCompany = async (id: number) => {
    if (confirm("Ви впевнені, що хочете видалити цю компанію?")) {
      try {
        const response = await fetch('/api/admin', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ 
            action: 'deleteCompany', 
            id 
          }),
        });
        
        if (!response.ok) {
          throw new Error('Failed to delete company');
        }
        
        // Remove the company from local state
        setCompanies(companies.filter((company) => company.id !== id));
      } catch (error) {
        console.error("Error deleting company:", error);
        alert("Failed to delete company. Please try again.");
      }
    }
  }

  if (loading) {
    return <div>Loading companies...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Управління компаніями</h1>
          <p className="text-gray-500">Додавання та редагування клієнтських компаній</p>
        </div>
        <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2">
              <Plus className="h-4 w-4" />
              Додати компанію
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[525px]">
            <DialogHeader>
              <DialogTitle>Додати нову компанію</DialogTitle>
              <DialogDescription>Введіть інформацію про нову клієнтську компанію</DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Назва компанії</Label>
                  <Input
                    id="name"
                    value={newCompany.name}
                    onChange={(e) => setNewCompany({ ...newCompany, name: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="contact">Контактна особа</Label>
                  <Input
                    id="contact"
                    value={newCompany.contact}
                    onChange={(e) => setNewCompany({ ...newCompany, contact: e.target.value })}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    type="email"
                    value={newCompany.email}
                    onChange={(e) => setNewCompany({ ...newCompany, email: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phone">Телефон</Label>
                  <Input
                    id="phone"
                    value={newCompany.phone}
                    onChange={(e) => setNewCompany({ ...newCompany, phone: e.target.value })}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="address">Адреса</Label>
                <Input
                  id="address"
                  value={newCompany.address}
                  onChange={(e) => setNewCompany({ ...newCompany, address: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="notes">Примітки</Label>
                <Textarea
                  id="notes"
                  value={newCompany.notes}
                  onChange={(e) => setNewCompany({ ...newCompany, notes: e.target.value })}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                Скасувати
              </Button>
              <Button onClick={handleAddCompany}>Додати</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Клієнтські компанії</CardTitle>
          <CardDescription>Список компаній, з якими працює агентство</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Назва</TableHead>
                <TableHead>Контактна особа</TableHead>
                <TableHead>Email</TableHead>
                <TableHead>Телефон</TableHead>
                <TableHead>Активні проєкти</TableHead>
                <TableHead>Дії</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {companies.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center">Немає доступних компаній</TableCell>
                </TableRow>
              ) : (
                companies.map((company) => (
                  <TableRow key={company.id}>
                    <TableCell className="font-medium">{company.name}</TableCell>
                    <TableCell>{company.contact}</TableCell>
                    <TableCell>{company.email}</TableCell>
                    <TableCell>{company.phone}</TableCell>
                    <TableCell>{company.projects || 0}</TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        <Dialog open={isEditDialogOpen && editingCompany?.id === company.id} onOpenChange={setIsEditDialogOpen}>
                          <DialogTrigger asChild>
                            <Button variant="ghost" size="icon" onClick={() => setEditingCompany(company)}>
                              <Pencil className="h-4 w-4" />
                              <span className="sr-only">Редагувати</span>
                            </Button>
                          </DialogTrigger>
                          {editingCompany && editingCompany.id === company.id && (
                            <DialogContent className="sm:max-w-[525px]">
                              <DialogHeader>
                                <DialogTitle>Редагувати компанію</DialogTitle>
                                <DialogDescription>Змініть інформацію про компанію</DialogDescription>
                              </DialogHeader>
                              <div className="grid gap-4 py-4">
                                <div className="grid grid-cols-2 gap-4">
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-name">Назва компанії</Label>
                                    <Input
                                      id="edit-name"
                                      value={editingCompany.name}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, name: e.target.value })}
                                    />
                                  </div>
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-contact">Контактна особа</Label>
                                    <Input
                                      id="edit-contact"
                                      value={editingCompany.contact}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, contact: e.target.value })}
                                    />
                                  </div>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-email">Email</Label>
                                    <Input
                                      id="edit-email"
                                      type="email"
                                      value={editingCompany.email}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, email: e.target.value })}
                                    />
                                  </div>
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-phone">Телефон</Label>
                                    <Input
                                      id="edit-phone"
                                      value={editingCompany.phone}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, phone: e.target.value })}
                                    />
                                  </div>
                                </div>
                                <div className="space-y-2">
                                  <Label htmlFor="edit-address">Адреса</Label>
                                  <Input
                                    id="edit-address"
                                    value={editingCompany.address || ''}
                                    onChange={(e) => setEditingCompany({ ...editingCompany, address: e.target.value })}
                                  />
                                </div>
                                <div className="space-y-2">
                                  <Label htmlFor="edit-notes">Примітки</Label>
                                  <Textarea
                                    id="edit-notes"
                                    value={editingCompany.notes || ''}
                                    onChange={(e) => setEditingCompany({ ...editingCompany, notes: e.target.value })}
                                  />
                                </div>
                              </div>
                              <DialogFooter>
                                <Button variant="outline" onClick={() => setIsEditDialogOpen(false)}>
                                  Скасувати
                                </Button>
                                <Button onClick={handleEditCompany}>Зберегти</Button>
                              </DialogFooter>
                            </DialogContent>
                          )}
                        </Dialog>
                        <Button variant="ghost" size="icon" onClick={() => handleDeleteCompany(company.id)}>
                          <Trash className="h-4 w-4" />
                          <span className="sr-only">Видалити</span>
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
    </div>
  )
}
