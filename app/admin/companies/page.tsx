"use client"

import { CompanyManagement } from "@/components/admin/company-management"
import { DashboardHeader } from "@/components/dashboard-header"
import { DashboardSidebar } from "@/components/dashboard-sidebar"
import { useAdminProtection } from "@/lib/auth"

export default function CompaniesPage() {
  const isAdmin = useAdminProtection()

  if (!isAdmin) {
    return null
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <DashboardSidebar isAdmin={true} />
      <div className="flex-1 flex flex-col overflow-hidden">
        <DashboardHeader isAdmin={true} />
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          <CompanyManagement />
        </main>
      </div>
    </div>
  )
}
