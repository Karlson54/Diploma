"use client"

import { EmployeeReports } from "@/components/employee-reports"
import { SimpleSidebar } from "@/components/simple-sidebar"
import { useAuthProtection } from "@/lib/auth"
import { useUser } from "@clerk/nextjs"

export default function ReportsPage() {
  const isAuthenticated = useAuthProtection()
  const { user } = useUser()

  if (!isAuthenticated) {
    return null
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <SimpleSidebar />
      <div className="flex-1 flex flex-col overflow-hidden">
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          <EmployeeReports />
        </main>
      </div>
    </div>
  )
}
