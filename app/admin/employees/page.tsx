"use client"

import { EmployeesList } from "@/components/admin/employees-list"
import { DashboardSidebar } from "@/components/dashboard-sidebar"
import { useAdminProtection } from "@/lib/auth"
import { useTranslation } from "react-i18next"

export default function EmployeesPage() {
  const isAdmin = useAdminProtection()
  const { t } = useTranslation()

  if (!isAdmin) {
    return null
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <DashboardSidebar isAdmin={true} />
      <div className="flex-1 flex flex-col overflow-hidden">
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          <EmployeesList />
        </main>
      </div>
    </div>
  )
}
