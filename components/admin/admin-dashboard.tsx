"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { EmployeeOverview } from "@/components/admin/employee-overview"
import { CompanyOverview } from "@/components/admin/company-overview"
import { RecentReports } from "@/components/admin/recent-reports"

export function AdminDashboard() {
  const [stats, setStats] = useState({
    employeeCount: 0,
    companyCount: 0,
    loading: true
  });

  useEffect(() => {
    async function fetchDashboardStats() {
      try {
        // Fetch counts from API
        const response = await fetch('/api/admin');
        if (!response.ok) {
          throw new Error('Failed to fetch dashboard data');
        }
        
        const data = await response.json();
        setStats({
          employeeCount: data.employeeCount,
          companyCount: data.companyCount,
          loading: false
        });
      } catch (error) {
        console.error("Error fetching dashboard stats:", error);
        setStats({
          ...stats,
          loading: false
        });
      }
    }
    
    fetchDashboardStats();
  }, []);

  if (stats.loading) {
    return <div>Loading dashboard stats...</div>;
  }

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
            <div className="text-2xl font-bold">{stats.employeeCount}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">Клієнтські компанії</CardTitle>
            <CardDescription>Всього в базі</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.companyCount}</div>
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
