"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import Link from "next/link"
import { BarChart3, Clock, Users, FileText, Menu, X, Building, FileSpreadsheet, LogOut, Calendar } from "lucide-react"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"
import { useMobile } from "@/hooks/use-mobile"
import { setAuthenticated } from "@/lib/auth"

interface DashboardSidebarProps {
  isAdmin?: boolean
}

export function DashboardSidebar({ isAdmin = false }: DashboardSidebarProps) {
  const isMobile = useMobile()
  const [isOpen, setIsOpen] = useState(!isMobile)
  const router = useRouter()

  const toggleSidebar = () => {
    setIsOpen(!isOpen)
  }

  const handleLogout = () => {
    setAuthenticated(false)
    router.push("/login")
  }

  return (
    <>
      {isMobile && (
        <Button variant="outline" size="icon" className="fixed top-4 left-4 z-50" onClick={toggleSidebar}>
          {isOpen ? <X className="h-4 w-4" /> : <Menu className="h-4 w-4" />}
        </Button>
      )}
      <div
        className={cn(
          "bg-white border-r w-64 flex flex-col transition-all duration-300 z-40",
          isMobile && "fixed h-full",
          isMobile && !isOpen && "-translate-x-full",
        )}
      >
        <div className="p-4 border-b">
          <div className="h-8">
            <img
              src="https://hebbkx1anhila5yf.public.blob.vercel-storage.com/%D0%91%D0%B5%D0%B7%20%D0%B7%D0%B0%D0%B3%D0%BE%D0%BB%D0%BE%D0%B2%D0%BA%D0%B0-fUH90pbu2g9blr3Tk2CoJfZWlS4CiP.png"
              alt="Mediacom"
              className="h-full"
            />
          </div>
          <p className="text-sm text-gray-500 mt-2">{isAdmin ? "Панель адміністратора" : "Облік робочого часу"}</p>
        </div>
        <nav className="flex-1 p-4">
          {isAdmin ? (
            <>
              <p className="text-xs uppercase font-semibold text-gray-500 mb-2">Функції користувача</p>
              <ul className="space-y-1 mb-6">
                <li>
                  <Link
                    href="/admin/user-dashboard"
                    className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                  >
                    <Calendar className="h-5 w-5" />
                    <span>Календар</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/admin/user-reports"
                    className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                  >
                    <FileText className="h-5 w-5" />
                    <span>Мої звіти</span>
                  </Link>
                </li>
              </ul>

              <div className="h-px bg-gray-200 my-4"></div>

              <p className="text-xs uppercase font-semibold text-gray-500 mb-2">Функції адміністратора</p>
              <ul className="space-y-1">
                <li>
                  <Link
                    href="/admin"
                    className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-900 bg-gray-100 font-medium"
                  >
                    <BarChart3 className="h-5 w-5" />
                    <span>Дашборд</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/admin/reports"
                    className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                  >
                    <FileSpreadsheet className="h-5 w-5" />
                    <span>Звіти</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/admin/companies"
                    className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                  >
                    <Building className="h-5 w-5" />
                    <span>Компанії</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/admin/employees"
                    className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                  >
                    <Users className="h-5 w-5" />
                    <span>Співробітники</span>
                  </Link>
                </li>
                <li className="mt-6 pt-6 border-t">
                  <Button
                    variant="ghost"
                    className="w-full flex items-center justify-start gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                    onClick={handleLogout}
                  >
                    <LogOut className="h-5 w-5" />
                    <span>Вийти</span>
                  </Button>
                </li>
              </ul>
            </>
          ) : (
            <ul className="space-y-1">
              <li>
                <Link
                  href="/"
                  className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-900 bg-gray-100 font-medium"
                >
                  <BarChart3 className="h-5 w-5" />
                  <span>Дашборд</span>
                </Link>
              </li>
              <li>
                <Link
                  href="#"
                  className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                >
                  <Clock className="h-5 w-5" />
                  <span>Облік часу</span>
                </Link>
              </li>
              <li>
                <Link
                  href="#"
                  className="flex items-center gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                >
                  <FileText className="h-5 w-5" />
                  <span>Звіти</span>
                </Link>
              </li>
              <li className="mt-6 pt-6 border-t">
                <Button
                  variant="ghost"
                  className="w-full flex items-center justify-start gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                  onClick={handleLogout}
                >
                  <LogOut className="h-5 w-5" />
                  <span>Вийти</span>
                </Button>
              </li>
            </ul>
          )}
        </nav>
      </div>
    </>
  )
}
