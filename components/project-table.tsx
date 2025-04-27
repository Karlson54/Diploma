"use client"

import { useState, useEffect } from "react"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Progress } from "@/components/ui/progress"
import { companyQueries, reportQueries } from "@/db/queries"

// Interface for project data
interface Project {
  id: number
  name: string
  client: string
  allocated: number
  spent: number
  deadline: string
  status: string
}

export function ProjectTable() {
  const [projects, setProjects] = useState<Project[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function fetchProjects() {
      try {
        // Fetch companies from the database
        const companies = await companyQueries.getAll()
        
        // Get all reports to calculate hours spent
        const allReports = await reportQueries.getAllWithEmployee()
        
        // Transform companies into projects with calculated data
        const projectData: Project[] = companies.map(company => {
          // Calculate hours spent for this company
          const reportsForCompany = allReports.filter(r => 
            r.report.client?.includes(company.name) || 
            r.report.contractingAgency?.includes(company.name)
          )
          
          const hoursSpent = reportsForCompany.reduce((sum, r) => sum + r.report.hours, 0)
          
          // Estimate allocated hours based on company's projects count
          const allocatedHours = company.projects ? company.projects * 40 : 80
          
          // Determine status based on spent vs allocated
          const isOverdue = hoursSpent > allocatedHours * 0.9
          
          // Mock deadline date (in a real app, this would come from a projects table)
          const today = new Date()
          const deadlineDate = new Date(today)
          deadlineDate.setDate(today.getDate() + (isOverdue ? -2 : 14))
          const deadline = `${deadlineDate.getDate()} ${deadlineDate.toLocaleString('default', { month: 'short' })}`
          
          return {
            id: company.id,
            name: `Project for ${company.name}`,
            client: company.name,
            allocated: allocatedHours,
            spent: hoursSpent,
            deadline,
            status: isOverdue ? "Просрочен" : "В процессе"
          }
        })
        
        setProjects(projectData)
      } catch (error) {
        console.error("Error fetching projects:", error)
        // Fallback to empty array if error occurs
        setProjects([])
      } finally {
        setLoading(false)
      }
    }
    
    fetchProjects()
  }, [])

  if (loading) {
    return <div>Загрузка проектов...</div>
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Проект</TableHead>
          <TableHead>Клиент</TableHead>
          <TableHead>Прогресс</TableHead>
          <TableHead>Дедлайн</TableHead>
          <TableHead>Статус</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {projects.length === 0 ? (
          <TableRow>
            <TableCell colSpan={5} className="text-center">Нет доступных проектов</TableCell>
          </TableRow>
        ) : (
          projects.map((project) => (
            <TableRow key={project.id}>
              <TableCell className="font-medium">{project.name}</TableCell>
              <TableCell>{project.client}</TableCell>
              <TableCell>
                <div className="flex items-center gap-2">
                  <Progress value={(project.spent / project.allocated) * 100} className="h-2 w-[100px]" />
                  <span className="text-sm text-gray-500">
                    {project.spent}/{project.allocated}ч
                  </span>
                </div>
              </TableCell>
              <TableCell>{project.deadline}</TableCell>
              <TableCell>
                <Badge variant={project.status === "Просрочен" ? "destructive" : "default"}>{project.status}</Badge>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  )
}
