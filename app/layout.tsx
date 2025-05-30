import type React from "react"
import type { Metadata } from "next"
import "./globals.css"
import { ClerkProvider } from "@clerk/nextjs"
import { AuthProvider } from "@/lib/AuthContext"
import I18nProvider from "@/lib/I18nProvider"

export const metadata: Metadata = {
  title: "v0 App",
  description: "Created with v0",
  generator: "v0.dev",
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <ClerkProvider 
      appearance={{ 
        elements: { 
          footerAction: "hidden" 
        } 
      }}
    >
      <html lang="en">
        <body>
          <I18nProvider>
            <AuthProvider>
              {children}
            </AuthProvider>
          </I18nProvider>
        </body>
      </html>
    </ClerkProvider>
  )
}
