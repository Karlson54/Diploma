"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { useAuth, useUser } from "@clerk/nextjs"

// Hook to check if user is authenticated
export function useAuthProtection() {
  const { isLoaded, isSignedIn } = useAuth()
  const router = useRouter()
  const [isAuthenticated, setIsAuthenticated] = useState(false)

  useEffect(() => {
    if (isLoaded && !isSignedIn) {
      router.push("/login")
    }
    
    if (isLoaded && isSignedIn) {
      setIsAuthenticated(true)
    }
  }, [isLoaded, isSignedIn, router])

  return isAuthenticated
}

// Hook to check if user is admin
export function useAdminProtection() {
  const { isLoaded, isSignedIn } = useAuth()
  const { user } = useUser()
  const router = useRouter()
  const [isAdmin, setIsAdmin] = useState(false)

  useEffect(() => {
    if (isLoaded && !isSignedIn) {
      router.push("/login")
      return
    }

    if (isLoaded && isSignedIn && user) {
      // Check if user has admin role in public metadata
      // You need to set this in Clerk Dashboard or via API
      const userRole = user.publicMetadata.role as string
      if (userRole !== "admin") {
        router.push("/dashboard")
        return
      }
      setIsAdmin(true)
    }
  }, [isLoaded, isSignedIn, user, router])

  return isAdmin
}

// Function to redirect user based on role
export function redirectAfterLogin(user: any) {
  if (!user) return "/login"
  
  const userRole = user.publicMetadata?.role as string
  if (userRole === "admin") {
    return "/admin/dashboard"
  } else {
    return "/dashboard"
  }
}

// For development/demo purposes: Store authentication state
let _isAuthenticated = false;
let _userRole = "";

// Helper to set authentication state
export function setAuthenticated(value: boolean): void {
  _isAuthenticated = value;
}

// Helper to set user role
export function setUserRole(role: string): void {
  _userRole = role;
}

// Helper function to get current user's email
export function getUserEmail(): string {
  if (typeof window === "undefined") return ""
  
  // This should be used inside a component with useUser hook
  // This is just a placeholder function to maintain API compatibility
  return ""
}

// Helper function to get user role
export function getUserRole(): string {
  if (typeof window === "undefined") return ""
  
  // Return stored role for development/demo purposes
  return _userRole || "user"
}

// Helper to check if a user has a specific role
export function hasRole(role: string): boolean {
  if (typeof window === "undefined") return false
  
  // This should be used inside a component with useUser hook
  // This is just a placeholder function to maintain API compatibility
  return getUserRole() === role
}
