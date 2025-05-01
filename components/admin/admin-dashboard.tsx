"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { EmployeeOverview } from "@/components/admin/employee-overview"
import { CompanyOverview } from "@/components/admin/company-overview"
import { RecentReports } from "@/components/admin/recent-reports"
import { useTranslation } from "react-i18next"

export function AdminDashboard() {
  const { t } = useTranslation()
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
    return <div>{t('admin.dashboard.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('admin.dashboard.title')}</h1>
        <p className="text-gray-500">{t('admin.dashboard.description')}</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-2">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">{t('admin.dashboard.stats.employees')}</CardTitle>
            <CardDescription>{t('admin.dashboard.stats.activeAccounts')}</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.employeeCount}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">{t('admin.dashboard.stats.companies')}</CardTitle>
            <CardDescription>{t('admin.dashboard.stats.totalInDatabase')}</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.companyCount}</div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="employees">
        <TabsList>
          <TabsTrigger value="employees">{t('admin.dashboard.tabs.employees')}</TabsTrigger>
          <TabsTrigger value="companies">{t('admin.dashboard.tabs.companies')}</TabsTrigger>
          <TabsTrigger value="reports">{t('admin.dashboard.tabs.reports')}</TabsTrigger>
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
