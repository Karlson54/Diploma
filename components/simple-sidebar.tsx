"use client"

import { useState, useEffect } from "react"
import { useRouter, usePathname } from "next/navigation"
import Link from "next/link"
import { Calendar, Menu, X, FileText, LogOut, BarChart3, Users, Building, FileSpreadsheet } from "lucide-react"
import { Button } from "@/components/ui/button"
import { cn, formatEmployeeName } from "@/lib/utils"
import { useMobile } from "@/hooks/use-mobile"
import { useClerk, useUser } from "@clerk/nextjs"
import { useCurrentEmployee } from "@/hooks/use-current-employee"
import { LanguageSwitcher } from "./language-switcher"
import { useTranslation } from "react-i18next"

export function SimpleSidebar() {
  const isMobile = useMobile()
  const [isOpen, setIsOpen] = useState(!isMobile)
  const router = useRouter()
  const pathname = usePathname()
  const { signOut } = useClerk()
  const { user, isLoaded } = useUser()
  const { employee, isLoading: isEmployeeLoading } = useCurrentEmployee()
  const isAdmin = isLoaded && user?.publicMetadata?.role === "admin"
  const { t } = useTranslation()

  useEffect(() => {
    if (isLoaded && user) {
      // Log user role information to console
      console.log("User role:", user.publicMetadata?.role || "regular user")
    }
  }, [isLoaded, user])

  const toggleSidebar = () => {
    setIsOpen(!isOpen)
  }

  const handleLogout = async () => {
    await signOut({ redirectUrl: '/login' })
  }

  // Format the employee name for display
  const formattedName = employee ? formatEmployeeName(employee.name) : null;

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
          <p className="text-sm font-medium mt-2">
            {!isEmployeeLoading && employee ? (
              <>
                {formattedName?.firstName} {formattedName?.lastName}
              </>
            ) : isLoaded && user ? (
              `${user.firstName} ${user.lastName}`
            ) : (
              t('calendar.loading')
            )}
            <span className="block text-xs text-gray-500">
              {isLoaded && user ? (user.publicMetadata?.role === "admin" ? t('calendar.adminRole') : user.emailAddresses[0]?.emailAddress) : ""}
            </span>
          </p>
        </div>
        <nav className="flex-1 p-4">
          {isAdmin ? (
            <>
              <p className="text-xs uppercase font-semibold text-gray-500 mb-2">{t('calendar.userFunctions')}</p>
              <ul className="space-y-1 mb-6">
                <li>
                  <Link
                    href="/dashboard"
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                      pathname === "/dashboard" 
                        ? "text-gray-900 bg-gray-100 font-medium" 
                        : "text-gray-500"
                    )}
                  >
                    <Calendar className="h-5 w-5" />
                    <span>{t('calendar.menu.calendar')}</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/reports"
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                      pathname === "/reports" 
                        ? "text-gray-900 bg-gray-100 font-medium" 
                        : "text-gray-500"
                    )}
                  >
                    <FileText className="h-5 w-5" />
                    <span>{t('calendar.menu.myReports')}</span>
                  </Link>
                </li>
              </ul>

              <div className="h-px bg-gray-200 my-4"></div>

              <p className="text-xs uppercase font-semibold text-gray-500 mb-2">{t('calendar.adminFunctions')}</p>
              <ul className="space-y-1">
                <li>
                  <Link
                    href="/admin"
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                      pathname === "/admin" 
                        ? "text-gray-900 bg-gray-100 font-medium" 
                        : "text-gray-500"
                    )}
                  >
                    <BarChart3 className="h-5 w-5" />
                    <span>{t('calendar.menu.dashboard')}</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/admin/reports"
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                      pathname === "/admin/reports" 
                        ? "text-gray-900 bg-gray-100 font-medium" 
                        : "text-gray-500"
                    )}
                  >
                    <FileSpreadsheet className="h-5 w-5" />
                    <span>{t('calendar.menu.reports')}</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/admin/companies"
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                      pathname === "/admin/companies" 
                        ? "text-gray-900 bg-gray-100 font-medium" 
                        : "text-gray-500"
                    )}
                  >
                    <Building className="h-5 w-5" />
                    <span>{t('calendar.menu.companies')}</span>
                  </Link>
                </li>
                <li>
                  <Link
                    href="/admin/employees"
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                      pathname === "/admin/employees" 
                        ? "text-gray-900 bg-gray-100 font-medium" 
                        : "text-gray-500"
                    )}
                  >
                    <Users className="h-5 w-5" />
                    <span>{t('calendar.menu.employees')}</span>
                  </Link>
                </li>
              </ul>
            </>
          ) : (
            <ul className="space-y-1">
              <li>
                <Link
                  href="/dashboard"
                  className={cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                    pathname === "/dashboard" 
                      ? "text-gray-900 bg-gray-100 font-medium" 
                      : "text-gray-500"
                  )}
                >
                  <Calendar className="h-5 w-5" />
                  <span>{t('calendar.menu.calendar')}</span>
                </Link>
              </li>
              <li>
                <Link
                  href="/reports"
                  className={cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 hover:text-gray-900 hover:bg-gray-50",
                    pathname === "/reports" 
                      ? "text-gray-900 bg-gray-100 font-medium" 
                      : "text-gray-500"
                  )}
                >
                  <FileText className="h-5 w-5" />
                  <span>{t('calendar.menu.myReports')}</span>
                </Link>
              </li>
            </ul>
          )}
          
          <div className="mt-6 pt-6 border-t">
            <div className="flex justify-between items-center">
              <Button
                variant="ghost"
                className="flex items-center justify-start gap-3 rounded-md px-3 py-2 text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                onClick={handleLogout}
              >
                <LogOut className="h-5 w-5" />
                <span>{t('calendar.menu.logout')}</span>
              </Button>
              <LanguageSwitcher />
            </div>
          </div>
        </nav>
      </div>
    </>
  )
}
