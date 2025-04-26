"use client"

import { useState } from "react"
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

export function CompanyManagement() {
  const [companies, setCompanies] = useState([
    {
      id: 1,
      name: "Acme Inc",
      contact: "Джон Сміт",
      email: "john@acme.com",
      phone: "+380 99 123 4567",
      projects: 3,
    },
    {
      id: 2,
      name: "Globex Corp",
      contact: "Марія Іванова",
      email: "maria@globex.com",
      phone: "+380 99 234 5678",
      projects: 2,
    },
    {
      id: 3,
      name: "Tech Solutions",
      contact: "Олексій Петров",
      email: "alex@techsolutions.com",
      phone: "+380 99 345 6789",
      projects: 1,
    },
    {
      id: 4,
      name: "Stark Industries",
      contact: "Олена Сидорова",
      email: "elena@stark.com",
      phone: "+380 99 456 7890",
      projects: 4,
    },
  ])

  const [newCompany, setNewCompany] = useState({
    name: "",
    contact: "",
    email: "",
    phone: "",
    address: "",
    notes: "",
  })

  const [editingCompany, setEditingCompany] = useState(null)
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)

  const handleAddCompany = () => {
    setCompanies([
      ...companies,
      {
        id: companies.length + 1,
        ...newCompany,
        projects: 0,
      },
    ])
    setNewCompany({
      name: "",
      contact: "",
      email: "",
      phone: "",
      address: "",
      notes: "",
    })
    setIsAddDialogOpen(false)
  }

  const handleEditCompany = () => {
    setCompanies(companies.map((company) => (company.id === editingCompany.id ? { ...editingCompany } : company)))
    setIsEditDialogOpen(false)
  }

  const handleDeleteCompany = (id) => {
    if (confirm("Ви впевнені, що хочете видалити цю компанію?")) {
      setCompanies(companies.filter((company) => company.id !== id))
    }
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
              {companies.map((company) => (
                <TableRow key={company.id}>
                  <TableCell className="font-medium">{company.name}</TableCell>
                  <TableCell>{company.contact}</TableCell>
                  <TableCell>{company.email}</TableCell>
                  <TableCell>{company.phone}</TableCell>
                  <TableCell>{company.projects}</TableCell>
                  <TableCell>
                    <div className="flex gap-2">
                      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
                        <DialogTrigger asChild>
                          <Button variant="ghost" size="icon" onClick={() => setEditingCompany(company)}>
                            <Pencil className="h-4 w-4" />
                            <span className="sr-only">Редагувати</span>
                          </Button>
                        </DialogTrigger>
                        {editingCompany && (
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
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  )
}
