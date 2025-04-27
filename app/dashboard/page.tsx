"use client"
import { WeeklyCalendar } from "@/components/weekly-calendar"
import { DashboardHeader } from "@/components/dashboard-header"
import { SimpleSidebar } from "@/components/simple-sidebar"
import { withAuth } from "@/lib/AuthContext"

function DashboardPage() {
  return (
    <div className="flex h-screen bg-gray-50">
      <SimpleSidebar />
      <div className="flex-1 flex flex-col overflow-hidden">
        <DashboardHeader isAdmin={false} />
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          <WeeklyCalendar />
        </main>
      </div>
    </div>
  )
}

// Wrap with the auth protection HOC
export default withAuth(DashboardPage);
