"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { UserAccountNav } from "@/components/user-account-nav"
import { Bell } from "lucide-react"
import { Button } from "@/components/ui/button"

interface DashboardHeaderProps {
  isAdmin?: boolean
}

export function DashboardHeader({ isAdmin = false }: DashboardHeaderProps) {
  const [notifications, setNotifications] = useState(3)

  return (
    <header className="bg-white border-b h-16 flex items-center justify-between px-4 md:px-6">
      <div>
        {/* Page title or breadcrumbs can go here */}
      </div>
      <div className="flex items-center space-x-4">
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-5 w-5" />
          {notifications > 0 && (
            <span className="absolute top-1 right-1 h-4 w-4 rounded-full bg-red-500 text-xs flex items-center justify-center text-white">
              {notifications}
            </span>
          )}
        </Button>
        <UserAccountNav />
      </div>
    </header>
  )
}
