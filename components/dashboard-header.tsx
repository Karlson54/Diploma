"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { setAuthenticated } from "@/lib/auth"

interface DashboardHeaderProps {
  isAdmin?: boolean
}

export function DashboardHeader({ isAdmin = false }: DashboardHeaderProps) {
  const router = useRouter()
  const [notifications, setNotifications] = useState(3)

  const handleLogout = () => {
    setAuthenticated(false)
    router.push("/login")
  }

  return null
}
