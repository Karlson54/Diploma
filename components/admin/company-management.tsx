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
import { useTranslation } from "react-i18next"

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
  const { t } = useTranslation()
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [isDeleting, setIsDeleting] = useState<number | null>(null)
  const [isUpdating, setIsUpdating] = useState<number | null>(null)
  const [updateMessage, setUpdateMessage] = useState<string | null>(null)

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
      setIsUpdating(editingCompany.id);
      setUpdateMessage(null);
      
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
      
      const result = await response.json();
      
      if (!response.ok) {
        throw new Error(result.error || 'Failed to update company');
      }
      
      // Update the local state with edited company
      setCompanies(companies.map((company) => 
        company.id === editingCompany.id ? { ...editingCompany } : company
      ));
      
      // Show success message
      setUpdateMessage(result.message || t('admin.companies.edit.successMessage'));
      
      // Close the dialog after a short delay
      setTimeout(() => {
        setIsEditDialogOpen(false);
        setUpdateMessage(null);
      }, 1500);
    } catch (error) {
      console.error("Error updating company:", error);
      setUpdateMessage('Failed to update company. Please try again.');
    } finally {
      setIsUpdating(null);
    }
  }

  const handleDeleteCompany = async (id: number) => {
    if (confirm(t('admin.companies.deleteConfirm'))) {
      try {
        setIsDeleting(id);
        
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
        
        const result = await response.json();
        
        // Remove the company from local state
        setCompanies(companies.filter((company) => company.id !== id));
        
        // Show success message
        alert(result.message || t('admin.companies.deleteSuccess'));
      } catch (error) {
        console.error("Error deleting company:", error);
        alert("Failed to delete company. Please try again.");
      } finally {
        setIsDeleting(null);
      }
    }
  }

  if (loading) {
    return <div>{t('admin.companies.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('admin.companies.title')}</h1>
          <p className="text-gray-500">{t('admin.companies.description')}</p>
        </div>
        <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2">
              <Plus className="h-4 w-4" />
              {t('admin.companies.addCompany')}
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[525px]">
            <DialogHeader>
              <DialogTitle>{t('admin.companies.add.title')}</DialogTitle>
              <DialogDescription>{t('admin.companies.add.description')}</DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="name">{t('admin.companies.add.name')}</Label>
                  <Input
                    id="name"
                    value={newCompany.name}
                    onChange={(e) => setNewCompany({ ...newCompany, name: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="contact">{t('admin.companies.add.contact')}</Label>
                  <Input
                    id="contact"
                    value={newCompany.contact}
                    onChange={(e) => setNewCompany({ ...newCompany, contact: e.target.value })}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="email">{t('admin.companies.add.email')}</Label>
                  <Input
                    id="email"
                    type="email"
                    value={newCompany.email}
                    onChange={(e) => setNewCompany({ ...newCompany, email: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phone">{t('admin.companies.add.phone')}</Label>
                  <Input
                    id="phone"
                    value={newCompany.phone}
                    onChange={(e) => setNewCompany({ ...newCompany, phone: e.target.value })}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="address">{t('admin.companies.add.address')}</Label>
                <Input
                  id="address"
                  value={newCompany.address || ''}
                  onChange={(e) => setNewCompany({ ...newCompany, address: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="notes">{t('admin.companies.add.notes')}</Label>
                <Textarea
                  id="notes"
                  value={newCompany.notes || ''}
                  onChange={(e) => setNewCompany({ ...newCompany, notes: e.target.value })}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                {t('admin.companies.add.cancel')}
              </Button>
              <Button onClick={handleAddCompany}>{t('admin.companies.add.add')}</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t('admin.companies.clientCompanies')}</CardTitle>
          <CardDescription>{t('admin.companies.clientCompaniesDesc')}</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t('admin.companies.tableHeaders.name')}</TableHead>
                <TableHead>{t('admin.companies.tableHeaders.contact')}</TableHead>
                <TableHead>{t('admin.companies.tableHeaders.email')}</TableHead>
                <TableHead>{t('admin.companies.tableHeaders.phone')}</TableHead>
                <TableHead>{t('admin.companies.tableHeaders.projects')}</TableHead>
                <TableHead>{t('admin.companies.tableHeaders.actions')}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {companies.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center">{t('admin.companies.noCompanies')}</TableCell>
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
                              <span className="sr-only">{t('admin.companies.tableHeaders.edit')}</span>
                            </Button>
                          </DialogTrigger>
                          {editingCompany && editingCompany.id === company.id && (
                            <DialogContent className="sm:max-w-[525px]">
                              <DialogHeader>
                                <DialogTitle>{t('admin.companies.edit.title')}</DialogTitle>
                                <DialogDescription>{t('admin.companies.edit.description')}</DialogDescription>
                              </DialogHeader>
                              <div className="grid gap-4 py-4">
                                <div className="grid grid-cols-2 gap-4">
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-name">{t('admin.companies.add.name')}</Label>
                                    <Input
                                      id="edit-name"
                                      value={editingCompany.name}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, name: e.target.value })}
                                    />
                                  </div>
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-contact">{t('admin.companies.add.contact')}</Label>
                                    <Input
                                      id="edit-contact"
                                      value={editingCompany.contact}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, contact: e.target.value })}
                                    />
                                  </div>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-email">{t('admin.companies.add.email')}</Label>
                                    <Input
                                      id="edit-email"
                                      type="email"
                                      value={editingCompany.email}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, email: e.target.value })}
                                    />
                                  </div>
                                  <div className="space-y-2">
                                    <Label htmlFor="edit-phone">{t('admin.companies.add.phone')}</Label>
                                    <Input
                                      id="edit-phone"
                                      value={editingCompany.phone}
                                      onChange={(e) => setEditingCompany({ ...editingCompany, phone: e.target.value })}
                                    />
                                  </div>
                                </div>
                                <div className="space-y-2">
                                  <Label htmlFor="edit-address">{t('admin.companies.add.address')}</Label>
                                  <Input
                                    id="edit-address"
                                    value={editingCompany.address || ''}
                                    onChange={(e) => setEditingCompany({ ...editingCompany, address: e.target.value })}
                                  />
                                </div>
                                <div className="space-y-2">
                                  <Label htmlFor="edit-notes">{t('admin.companies.add.notes')}</Label>
                                  <Textarea
                                    id="edit-notes"
                                    value={editingCompany.notes || ''}
                                    onChange={(e) => setEditingCompany({ ...editingCompany, notes: e.target.value })}
                                  />
                                </div>
                              </div>
                              <DialogFooter>
                                <Button variant="outline" onClick={() => setIsEditDialogOpen(false)}>
                                  {t('admin.companies.edit.cancel')}
                                </Button>
                                <Button 
                                  onClick={handleEditCompany} 
                                  disabled={isUpdating === editingCompany?.id}
                                >
                                  {isUpdating === editingCompany?.id ? (
                                    <span className="flex items-center gap-2">
                                      <span className="animate-spin h-4 w-4 border-2 border-white rounded-full border-t-transparent" />
                                      {t('admin.companies.edit.updating')}
                                    </span>
                                  ) : (
                                    t('admin.companies.edit.save')
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
                          onClick={() => handleDeleteCompany(company.id)}
                          disabled={isDeleting === company.id}
                        >
                          {isDeleting === company.id ? (
                            <span className="animate-spin h-4 w-4 border-2 border-gray-500 rounded-full border-t-transparent" />
                          ) : (
                            <Trash className="h-4 w-4" />
                          )}
                          <span className="sr-only">{t('admin.companies.tableHeaders.delete')}</span>
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
