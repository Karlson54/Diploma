"use client"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { EmployeeOverview } from "@/components/admin/employee-overview"
import { CompanyOverview } from "@/components/admin/company-overview"
import { RecentReports } from "@/components/admin/recent-reports"

export function AdminDashboard() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Панель адміністратора</h1>
        <p className="text-gray-500">Управління співробітниками, компаніями та звітами</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-2">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">Всього співробітників</CardTitle>
            <CardDescription>Активні акаунти</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">5</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">Клієнтські компанії</CardTitle>
            <CardDescription>Всього в базі</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">24</div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="employees">
        <TabsList>
          <TabsTrigger value="employees">Співробітники</TabsTrigger>
          <TabsTrigger value="companies">Компанії</TabsTrigger>
          <TabsTrigger value="reports">Останні звіти</TabsTrigger>
        </TabsList>
        <TabsContent value="employees" className="mt-4">
          <EmployeeOverview />
        </TabsContent>
        <TabsContent value="companies" className="mt-4">
          <CompanyOverview />
        </TabsContent>
        <TabsContent value="reports" className="mt-4">
          <RecentReports />
        </TabsContent>
      </Tabs>
    </div>
  )
}
