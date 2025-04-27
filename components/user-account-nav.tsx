"use client"

import { UserButton, useUser } from "@clerk/nextjs"
import { 
  DropdownMenu, 
  DropdownMenuContent, 
  DropdownMenuItem, 
  DropdownMenuLabel, 
  DropdownMenuSeparator, 
  DropdownMenuTrigger 
} from "@/components/ui/dropdown-menu"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { LogOut, User } from "lucide-react"
import Link from "next/link"

export function UserAccountNav() {
  const { user, isSignedIn } = useUser()
  
  if (!isSignedIn || !user) {
    return null
  }
  
  return (
    <div className="flex items-center gap-2">
      {/* Simple version - just use the Clerk UserButton */}
      <UserButton afterSignOutUrl="/login" />
      
      {/* Alternative custom dropdown */}
      {/*
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="relative h-8 w-8 rounded-full">
            <Avatar className="h-8 w-8">
              <AvatarImage src={user.imageUrl} alt={user.username || user.firstName || "User"} />
              <AvatarFallback>
                {user.firstName?.charAt(0) || user.username?.charAt(0) || "U"}
              </AvatarFallback>
            </Avatar>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent className="w-56" align="end" forceMount>
          <DropdownMenuLabel className="font-normal">
            <div className="flex flex-col space-y-1">
              <p className="text-sm font-medium leading-none">
                {user.firstName} {user.lastName}
              </p>
              <p className="text-xs leading-none text-muted-foreground">
                {user.emailAddresses[0].emailAddress}
              </p>
            </div>
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem asChild>
            <Link href="/profile">
              <User className="mr-2 h-4 w-4" />
              <span>Профіль</span>
            </Link>
          </DropdownMenuItem>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            className="cursor-pointer"
            onSelect={() => window.location.href = "/api/auth/signout"}
          >
            <LogOut className="mr-2 h-4 w-4" />
            <span>Вихід</span>
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      */}
    </div>
  )
} 