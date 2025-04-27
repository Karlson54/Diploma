"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import Link from "next/link"

interface Company {
  id: number;
  name: string;
  contact: string;
  email: string;
  phone: string;
  projects: number | null;
  address: string | null;
  notes: string | null;
  activeProjects: number;
  totalHours: number;
  revenue: number;
}

export function CompanyOverview() {
  const [companies, setCompanies] = useState<Company[]>([]);
  const [loading, setLoading] = useState(true);

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

  // Format currency in UAH
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('uk-UA', { 
      style: 'currency', 
      currency: 'UAH',
      maximumFractionDigits: 0
    }).format(amount);
  };

  if (loading) {
    return <div>Loading company data...</div>;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Огляд компаній</CardTitle>
        <CardDescription>Клієнтські компанії та їх активні проєкти</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Назва</TableHead>
              <TableHead>Контактна особа</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Активні проєкти</TableHead>
              <TableHead>Загальний дохід</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {companies.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} className="text-center">Немає доступних компаній</TableCell>
              </TableRow>
            ) : (
              companies.map((company) => (
                <TableRow key={company.id}>
                  <TableCell className="font-medium">{company.name}</TableCell>
                  <TableCell>{company.contact}</TableCell>
                  <TableCell>{company.email}</TableCell>
                  <TableCell>{company.activeProjects}</TableCell>
                  <TableCell>{formatCurrency(company.revenue)}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </CardContent>
      <CardFooter>
        <Link href="/admin/companies">
          <Button variant="outline">Управління компаніями</Button>
        </Link>
      </CardFooter>
    </Card>
  )
}
