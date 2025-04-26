"use client"

import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Progress } from "@/components/ui/progress"

export function ProjectTable() {
  // Пример данных проектов
  const projects = [
    {
      id: 1,
      name: "Ребрендинг Acme Inc",
      client: "Acme Inc",
      allocated: 120,
      spent: 78,
      deadline: "28 Апр",
      status: "В процессе",
    },
    {
      id: 2,
      name: "Маркетинговая кампания",
      client: "Globex Corp",
      allocated: 80,
      spent: 76,
      deadline: "15 Апр",
      status: "Просрочен",
    },
    {
      id: 3,
      name: "Разработка веб-сайта",
      client: "Tech Solutions",
      allocated: 160,
      spent: 42,
      deadline: "10 Мая",
      status: "В процессе",
    },
    {
      id: 4,
      name: "SMM стратегия",
      client: "Stark Industries",
      allocated: 60,
      spent: 12,
      deadline: "30 Апр",
      status: "В процессе",
    },
    {
      id: 5,
      name: "Дизайн упаковки",
      client: "Wayne Enterprises",
      allocated: 40,
      spent: 38,
      deadline: "12 Апр",
      status: "Просрочен",
    },
  ]

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
        {projects.map((project) => (
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
        ))}
      </TableBody>
    </Table>
  )
}
