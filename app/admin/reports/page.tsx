"use client"

import { EmployeeReports } from "@/components/admin/employee-reports"
import { DashboardSidebar } from "@/components/dashboard-sidebar"
import { useAdminProtection } from "@/lib/auth"

export default function ReportsPage() {
  const isAdmin = useAdminProtection()

  if (!isAdmin) {
    return null
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <DashboardSidebar isAdmin={true} />
      <div className="flex-1 flex flex-col overflow-hidden">
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          <EmployeeReports />
        </main>
      </div>
    </div>
  )
}
