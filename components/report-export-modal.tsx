"use client"

import { useState, useEffect } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import ExcelJS from 'exceljs'

interface ReportData {
  date: string
  market: string
  agency: string
  client: string
  project: string
  media: string
  jobType: string
  comments: string
  hours: number
}

interface ReportExportModalProps {
  isOpen: boolean
  onClose: () => void
  reportData: ReportData[]
}

export function ReportExportModal({ isOpen, onClose, reportData }: ReportExportModalProps) {
  const [selectedColumns, setSelectedColumns] = useState({
    date: true,
    market: true,
    agency: true,
    client: true,
    project: true,
    media: true,
    jobType: true,
    comments: true,
    hours: true,
  })

  // Ensure previewData has content even if reportData is empty
  const previewData = reportData.length > 0 ? reportData.slice(0, 2) : [{
    date: '01.01.2025',
    market: 'Європа',
    agency: 'MediaCom',
    client: 'NewBiz',
    project: 'Campaign',
    media: 'Radio',
    jobType: 'Media plans',
    comments: 'asd',
    hours: 60
  }]

  // Reset column selection when modal is opened
  useEffect(() => {
    if (isOpen) {
      setSelectedColumns({
        date: true,
        market: true,
        agency: true,
        client: true,
        project: true,
        media: true,
        jobType: true,
        comments: true,
        hours: true,
      })
    }
  }, [isOpen])

  const toggleColumn = (column: keyof typeof selectedColumns) => {
    setSelectedColumns({
      ...selectedColumns,
      [column]: !selectedColumns[column]
    })
  }

  const downloadReport = async () => {
    try {
      // Validate that we have data to export
      if (reportData.length === 0) {
        alert('Немає даних для експорту')
        return
      }

      // Create a new workbook and worksheet
      const workbook = new ExcelJS.Workbook()
      workbook.creator = 'MediaCom'
      workbook.lastModifiedBy = 'MediaCom Report System'
      workbook.created = new Date()
      workbook.modified = new Date()
      
      const worksheet = workbook.addWorksheet('Звіт')

      // Define columns based on selected options
      const columns = []
      if (selectedColumns.date) columns.push({ header: 'Дата', key: 'date', width: 15 })
      if (selectedColumns.market) columns.push({ header: 'Ринок', key: 'market', width: 15 })
      if (selectedColumns.agency) columns.push({ header: 'Агентство', key: 'agency', width: 20 })
      if (selectedColumns.client) columns.push({ header: 'Клієнт', key: 'client', width: 20 })
      if (selectedColumns.project) columns.push({ header: 'Проект/Бренд', key: 'project', width: 20 })
      if (selectedColumns.media) columns.push({ header: 'Медіа', key: 'media', width: 15 })
      if (selectedColumns.jobType) columns.push({ header: 'Тип роботи', key: 'jobType', width: 20 })
      if (selectedColumns.comments) columns.push({ header: 'Коментарі', key: 'comments', width: 25 })
      if (selectedColumns.hours) columns.push({ header: 'Години', key: 'hours', width: 10 })

      worksheet.columns = columns

      // Style the headers
      worksheet.getRow(1).font = { bold: true }
      worksheet.getRow(1).fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFE6E6E6' }
      }

      // Add data rows
      reportData.forEach(row => {
        const rowData: any = {}
        if (selectedColumns.date) rowData.date = row.date
        if (selectedColumns.market) rowData.market = row.market
        if (selectedColumns.agency) rowData.agency = row.agency
        if (selectedColumns.client) rowData.client = row.client
        if (selectedColumns.project) rowData.project = row.project
        if (selectedColumns.media) rowData.media = row.media
        if (selectedColumns.jobType) rowData.jobType = row.jobType
        if (selectedColumns.comments) rowData.comments = row.comments
        if (selectedColumns.hours) rowData.hours = row.hours

        worksheet.addRow(rowData)
      })

      // Add borders to all cells
      worksheet.eachRow({ includeEmpty: false }, row => {
        row.eachCell({ includeEmpty: false }, cell => {
          cell.border = {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
          }
        })
      })

      // Generate Excel file
      const buffer = await workbook.xlsx.writeBuffer()
      const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
      const url = URL.createObjectURL(blob)
      
      // Create download link
      const a = document.createElement('a')
      a.href = url
      a.download = `Звіт_MediaCom_${new Date().toLocaleDateString().replace(/\./g, '-')}.xlsx`
      a.click()
      
      // Clean up
      URL.revokeObjectURL(url)
      onClose()
    } catch (error) {
      console.error('Помилка при створенні Excel файлу:', error)
      alert('Виникла помилка при створенні Excel файлу')
    }
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl">
        <DialogHeader>
          <DialogTitle>Завантаження звіту</DialogTitle>
        </DialogHeader>
        <div className="py-4">
          <p className="text-sm text-gray-500 mb-4">
            Оберіть стовпці для завантаження та перегляньте попередній вигляд таблиці
          </p>
          
          <div className="grid grid-cols-3 gap-4 mb-6">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="date"
                checked={selectedColumns.date}
                onCheckedChange={() => toggleColumn('date')}
              />
              <label htmlFor="date" className="text-sm font-medium">Дата</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="market"
                checked={selectedColumns.market}
                onCheckedChange={() => toggleColumn('market')}
              />
              <label htmlFor="market" className="text-sm font-medium">Ринок</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="agency"
                checked={selectedColumns.agency}
                onCheckedChange={() => toggleColumn('agency')}
              />
              <label htmlFor="agency" className="text-sm font-medium">Агентство</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="client"
                checked={selectedColumns.client}
                onCheckedChange={() => toggleColumn('client')}
              />
              <label htmlFor="client" className="text-sm font-medium">Клієнт</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="project"
                checked={selectedColumns.project}
                onCheckedChange={() => toggleColumn('project')}
              />
              <label htmlFor="project" className="text-sm font-medium">Проект/Бренд</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="media"
                checked={selectedColumns.media}
                onCheckedChange={() => toggleColumn('media')}
              />
              <label htmlFor="media" className="text-sm font-medium">Медіа</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="jobType"
                checked={selectedColumns.jobType}
                onCheckedChange={() => toggleColumn('jobType')}
              />
              <label htmlFor="jobType" className="text-sm font-medium">Тип роботи</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="comments"
                checked={selectedColumns.comments}
                onCheckedChange={() => toggleColumn('comments')}
              />
              <label htmlFor="comments" className="text-sm font-medium">Коментарі</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="hours"
                checked={selectedColumns.hours}
                onCheckedChange={() => toggleColumn('hours')}
              />
              <label htmlFor="hours" className="text-sm font-medium">Години</label>
            </div>
          </div>
          
          <h3 className="text-sm font-medium mb-2">Попередній перегляд таблиці</h3>
          <div className="border rounded-md overflow-auto max-h-64">
            <Table>
              <TableHeader>
                {selectedColumns.date && <TableHead>Дата</TableHead>}
                {selectedColumns.market && <TableHead>Ринок</TableHead>}
                {selectedColumns.agency && <TableHead>Агентство</TableHead>}
                {selectedColumns.client && <TableHead>Клієнт</TableHead>}
                {selectedColumns.project && <TableHead>Проект/Бренд</TableHead>}
                {selectedColumns.media && <TableHead>Медіа</TableHead>}
                {selectedColumns.jobType && <TableHead>Тип роботи</TableHead>}
                {selectedColumns.comments && <TableHead>Коментарі</TableHead>}
                {selectedColumns.hours && <TableHead>Години</TableHead>}
              </TableHeader>
              <TableBody>
                {previewData.map((row, index) => (
                  <TableRow key={index}>
                    {selectedColumns.date && <TableCell>{row.date}</TableCell>}
                    {selectedColumns.market && <TableCell>{row.market}</TableCell>}
                    {selectedColumns.agency && <TableCell>{row.agency}</TableCell>}
                    {selectedColumns.client && <TableCell>{row.client}</TableCell>}
                    {selectedColumns.project && <TableCell>{row.project}</TableCell>}
                    {selectedColumns.media && <TableCell>{row.media}</TableCell>}
                    {selectedColumns.jobType && <TableCell>{row.jobType}</TableCell>}
                    {selectedColumns.comments && <TableCell>{row.comments}</TableCell>}
                    {selectedColumns.hours && <TableCell>{row.hours}</TableCell>}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Скасувати
          </Button>
          <Button onClick={downloadReport}>
            Завантажити
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
} 