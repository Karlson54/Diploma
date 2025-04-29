"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"

interface DashboardHeaderProps {
  isAdmin?: boolean
}

export function DashboardHeader({ isAdmin = false }: DashboardHeaderProps) {
  return (
    <header className="bg-white border-b h-16 flex items-center justify-between px-4 md:px-6">
      <div>
        {/* Page title or breadcrumbs can go here */}
      </div>
    </header>
  )
}
