"use client"

import { useState } from "react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { BarChart3, ArrowUpRight, Users, DollarSign } from "lucide-react"

export default function DashboardCard() {
  const [isHovered, setIsHovered] = useState(false)

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100 p-4">
      <Card
        className={`w-full max-w-md transition-all duration-300 ${isHovered ? "shadow-lg transform -translate-y-1" : "shadow"}`}
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
      >
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <div>
            <CardTitle className="text-xl font-bold">Monthly Revenue</CardTitle>
            <CardDescription>April 2025</CardDescription>
          </div>
          <div className="p-2 bg-purple-100 rounded-full">
            <BarChart3 className="h-6 w-6 text-purple-600" />
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div>
              <div className="text-3xl font-bold">$24,780</div>
              <div className="flex items-center text-sm text-green-600">
                <ArrowUpRight className="h-4 w-4 mr-1" />
                <span>12% from last month</span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4 pt-4">
              <div className="bg-gray-50 p-3 rounded-lg">
                <div className="flex items-center text-sm text-gray-500 mb-1">
                  <Users className="h-4 w-4 mr-1" />
                  <span>New Users</span>
                </div>
                <div className="text-lg font-semibold">1,482</div>
              </div>
              <div className="bg-gray-50 p-3 rounded-lg">
                <div className="flex items-center text-sm text-gray-500 mb-1">
                  <DollarSign className="h-4 w-4 mr-1" />
                  <span>Avg. Order</span>
                </div>
                <div className="text-lg font-semibold">$58.39</div>
              </div>
            </div>
          </div>
        </CardContent>
        <CardFooter className="flex justify-between border-t pt-4">
          <Badge variant="outline" className="text-purple-600 bg-purple-50">
            Dashboard
          </Badge>
          <Button variant="outline" size="sm">
            View Details
          </Button>
        </CardFooter>
      </Card>
    </div>
  )
}
