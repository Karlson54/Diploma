"use client"

import type React from "react"

import { useState } from "react"
import { useRouter } from "next/navigation"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Checkbox } from "@/components/ui/checkbox"
import { AlertCircle } from "lucide-react"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { setAuthenticated, setUserRole } from "@/lib/auth"

export function LoginForm() {
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [rememberMe, setRememberMe] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState("")
  const router = useRouter()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError("")

    try {
      // Здесь будет логика аутентификации
      // Для демонстрации просто имитируем задержку и перенаправление
      await new Promise((resolve) => setTimeout(resolve, 1000))

      // Проверка демо-учетных данных
      if (email === "admin@example.com" && password === "password") {
        // Устанавливаем статус авторизации и роль
        setAuthenticated(true)
        setUserRole("admin")
        // Перенаправление на админ-панель
        router.push("/admin")
      } else if (email === "user@example.com" && password === "password") {
        // Устанавливаем статус авторизации и роль
        setAuthenticated(true)
        setUserRole("user")
        // Перенаправление на страницу пользователя
        router.push("/dashboard")
      } else {
        setError("Неверный email или пароль")
      }
    } catch (err) {
      setError("Произошла ошибка при входе. Пожалуйста, попробуйте снова.")
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {error && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <div className="space-y-2">
        <Label htmlFor="email">Email</Label>
        <Input
          id="email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="your.email@example.com"
          required
        />
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="password">Пароль</Label>
          <Link href="/forgot-password" className="text-sm font-medium text-blue-600 hover:text-blue-500">
            Забули пароль?
          </Link>
        </div>
        <Input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      </div>

      <div className="flex items-center space-x-2">
        <Checkbox id="remember" checked={rememberMe} onCheckedChange={(checked) => setRememberMe(checked as boolean)} />
        <label
          htmlFor="remember"
          className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
        >
          Запам'ятати мене
        </label>
      </div>

      <div>
        <Button type="submit" className="w-full" disabled={isLoading}>
          {isLoading ? "Вхід..." : "Увійти"}
        </Button>
      </div>

      <div className="text-center text-sm text-gray-500">
        <p>Для демонстрації використовуйте:</p>
        <p className="mt-1">
          <strong>Адмін:</strong> admin@example.com / password
        </p>
        <p>
          <strong>Користувач:</strong> user@example.com / password
        </p>
      </div>
    </form>
  )
}
