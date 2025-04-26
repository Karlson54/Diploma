"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"

// Простая функция для проверки авторизации
export function isAuthenticated(): boolean {
  if (typeof window === "undefined") return false
  return localStorage.getItem("isAuthenticated") === "true"
}

// Функция для установки статуса авторизации
export function setAuthenticated(value: boolean): void {
  if (typeof window === "undefined") return
  if (value) {
    localStorage.setItem("isAuthenticated", "true")
  } else {
    localStorage.removeItem("isAuthenticated")
  }
}

// Функция для установки роли пользователя
export function setUserRole(role: string): void {
  if (typeof window === "undefined") return
  localStorage.setItem("userRole", role)
}

// Функция для получения роли пользователя
export function getUserRole(): string {
  if (typeof window === "undefined") return ""
  return localStorage.getItem("userRole") || ""
}

// Функция для установки email пользователя
export function setUserEmail(email: string): void {
  if (typeof window === "undefined") return
  localStorage.setItem("userEmail", email)
}

// Функция для получения email пользователя
export function getUserEmail(): string {
  if (typeof window === "undefined") return ""
  return localStorage.getItem("userEmail") || ""
}

// Хук для защиты маршрутов
export function useAuthProtection() {
  const router = useRouter()

  useEffect(() => {
    if (!isAuthenticated()) {
      router.push("/login")
    }
  }, [router])

  return isAuthenticated()
}

// Хук для защиты маршрутов администратора
export function useAdminProtection() {
  const router = useRouter()

  useEffect(() => {
    if (!isAuthenticated()) {
      router.push("/login")
      return
    }

    if (getUserRole() !== "admin") {
      router.push("/")
    }
  }, [router])

  return isAuthenticated() && getUserRole() === "admin"
}
