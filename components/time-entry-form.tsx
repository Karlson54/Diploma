"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"

interface TimeEntryFormProps {
  onClose: () => void
}

export function TimeEntryForm({ onClose }: TimeEntryFormProps) {
  const [date, setDate] = useState(new Date().toISOString().split("T")[0])

  return (
    <form className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="project">Проект</Label>
          <Select>
            <SelectTrigger id="project">
              <SelectValue placeholder="Выберите проект" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="1">Ребрендинг Acme Inc</SelectItem>
              <SelectItem value="2">Маркетинговая кампания</SelectItem>
              <SelectItem value="3">Разработка веб-сайта</SelectItem>
              <SelectItem value="4">SMM стратегия</SelectItem>
              <SelectItem value="5">Дизайн упаковки</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="task">Задача</Label>
          <Select>
            <SelectTrigger id="task">
              <SelectValue placeholder="Выберите задачу" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="design">Дизайн</SelectItem>
              <SelectItem value="development">Разработка</SelectItem>
              <SelectItem value="meeting">Встреча</SelectItem>
              <SelectItem value="research">Исследование</SelectItem>
              <SelectItem value="other">Другое</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="space-y-2">
          <Label htmlFor="date">Дата</Label>
          <Input id="date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        </div>
        <div className="space-y-2">
          <Label htmlFor="start-time">Время начала</Label>
          <Input id="start-time" type="time" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="duration">Продолжительность (ч)</Label>
          <Input id="duration" type="number" min="0.25" step="0.25" placeholder="0.00" />
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="description">Описание</Label>
        <Textarea id="description" placeholder="Опишите выполненную работу..." rows={3} />
      </div>

      <div className="flex justify-end gap-2">
        <Button variant="outline" onClick={onClose}>
          Отмена
        </Button>
        <Button type="submit">Сохранить</Button>
      </div>
    </form>
  )
}
