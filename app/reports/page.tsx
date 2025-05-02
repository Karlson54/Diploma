"use client"

import { EmployeeReports } from "@/components/employee-reports"
import { SimpleSidebar } from "@/components/simple-sidebar"
import { useAuthProtection } from "@/lib/auth"
import { useUser } from "@clerk/nextjs"
import { useEffect } from "react"
import i18n from "@/lib/i18n"

export default function ReportsPage() {
  const isAuthenticated = useAuthProtection()
  const { user } = useUser()

  // Keep translation for report title and description while using English for table headers
  useEffect(() => {
    // Force English language for this page only
    const currentLang = i18n.language
    i18n.changeLanguage("en")
    
    // Restore original language when component unmounts
    return () => {
      if (currentLang !== "en") {
        i18n.changeLanguage(currentLang)
      }
    }
  }, [])

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
