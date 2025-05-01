"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useTranslation } from "react-i18next"

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
  const { t, i18n } = useTranslation()
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
    // Use the user's current language for formatting
    const locale = i18n.language === 'uk' ? 'uk-UA' : 'en-US';
    return new Intl.NumberFormat(locale, { 
      style: 'currency', 
      currency: 'UAH',
      maximumFractionDigits: 0
    }).format(amount);
  };

  if (loading) {
    return <div>{t('admin.companyOverview.loading')}</div>;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t('admin.companyOverview.title')}</CardTitle>
        <CardDescription>{t('admin.companyOverview.description')}</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('admin.companyOverview.name')}</TableHead>
              <TableHead>{t('admin.companyOverview.contact')}</TableHead>
              <TableHead>{t('admin.companyOverview.email')}</TableHead>
              <TableHead>{t('admin.companyOverview.activeProjects')}</TableHead>
              <TableHead>{t('admin.companyOverview.totalRevenue')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {companies.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} className="text-center">{t('admin.companyOverview.noCompanies')}</TableCell>
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
          <Button variant="outline">{t('admin.companyOverview.manageCompanies')}</Button>
        </Link>
      </CardFooter>
    </Card>
  )
}
