"use client"

import { EmployeeReports } from "@/components/admin/employee-reports"
import { DashboardSidebar } from "@/components/dashboard-sidebar"
import { useAdminProtection } from "@/lib/auth"
import { useEffect } from "react"
import i18n from "@/lib/i18n"

export default function ReportsPage() {
  const isAdmin = useAdminProtection()

  // Force English language for this page only
  useEffect(() => {
    const currentLang = i18n.language
    i18n.changeLanguage("en")
    
    // Restore original language when component unmounts
    return () => {
      if (currentLang !== "en") {
        i18n.changeLanguage(currentLang)
      }
    }
  }, [])

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
