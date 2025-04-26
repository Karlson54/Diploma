"use client"

import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import Link from "next/link"

export function CompanyOverview() {
  // Приклад даних компаній
  const companies = [
    {
      id: 1,
      name: "Acme Inc",
      contact: "Джон Сміт",
      email: "john@acme.com",
      activeProjects: 3,
      totalRevenue: "1,250,000 ₴",
    },
    {
      id: 2,
      name: "Globex Corp",
      contact: "Марія Іванова",
      email: "maria@globex.com",
      activeProjects: 2,
      totalRevenue: "850,000 ₴",
    },
    {
      id: 3,
      name: "Tech Solutions",
      contact: "Олексій Петров",
      email: "alex@techsolutions.com",
      activeProjects: 1,
      totalRevenue: "420,000 ₴",
    },
    {
      id: 4,
      name: "Stark Industries",
      contact: "Олена Сидорова",
      email: "elena@stark.com",
      activeProjects: 4,
      totalRevenue: "1,780,000 ₴",
    },
    {
      id: 5,
      name: "Wayne Enterprises",
      contact: "Дмитро Новіков",
      email: "dmitry@wayne.com",
      activeProjects: 2,
      totalRevenue: "920,000 ₴",
    },
  ]

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
            {companies.map((company) => (
              <TableRow key={company.id}>
                <TableCell className="font-medium">{company.name}</TableCell>
                <TableCell>{company.contact}</TableCell>
                <TableCell>{company.email}</TableCell>
                <TableCell>{company.activeProjects}</TableCell>
                <TableCell>{company.totalRevenue}</TableCell>
              </TableRow>
            ))}
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
