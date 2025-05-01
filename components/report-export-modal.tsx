"use client"

import { useState, useEffect } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import ExcelJS from 'exceljs'
import { useTranslation } from "react-i18next"

interface ReportData {
  date: string
  market: string
  agency: string
  client: string
  project: string
  projectBrand?: string
  media: string
  jobType: string
  comments: string
  hours: number
  fullName?: string
  company?: string
}

interface ReportExportModalProps {
  isOpen: boolean
  onClose: () => void
  reportData: ReportData[]
}

export function ReportExportModal({ isOpen, onClose, reportData }: ReportExportModalProps) {
  const { t } = useTranslation()
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
    fullName: true,
    company: true,
  })

  // Log incoming report data for debugging
  useEffect(() => {
    if (isOpen && reportData.length > 0) {
      console.log("ReportExportModal received data with fields:", 
        Object.keys(reportData[0]),
        "First record:", reportData[0]
      );
    }
  }, [isOpen, reportData]);

  // Ensure previewData has content even if reportData is empty
  const previewData = reportData.length > 0 ? reportData.slice(0, 10) : [{
    date: '22.04.2025',
    market: 'Європа',
    agency: 'MediaCom',
    fullName: 'Петренко',
    company: 'ФОП Петренко',
    client: 'NewBiz',
    project: 'Campaign',
    projectBrand: 'N/A',
    media: 'Radio',
    jobType: 'Бухгалтерський облік',
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
        fullName: true,
        company: true,
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
        alert(t('admin.reports.downloadDialog.noDataToExport'))
        return
      }

      console.log("Exporting data to Excel:", reportData);

      // Create a new workbook and worksheet
      const workbook = new ExcelJS.Workbook()
      workbook.creator = 'MediaCom'
      workbook.lastModifiedBy = 'MediaCom Report System'
      workbook.created = new Date()
      workbook.modified = new Date()
      
      const worksheet = workbook.addWorksheet('Звіт')

      // Define columns based on selected options
      const columns = []
      if (selectedColumns.agency) columns.push({ header: 'Agency', key: 'agency', width: 15 })
      if (selectedColumns.fullName) columns.push({ header: 'Name', key: 'fullName', width: 20 })
      if (selectedColumns.date) columns.push({ header: 'Date', key: 'date', width: 15 })
      if (selectedColumns.market) columns.push({ header: 'Market', key: 'market', width: 15 })
      if (selectedColumns.company) columns.push({ header: 'Contracting Agency / Unit', key: 'company', width: 20 })
      if (selectedColumns.client) columns.push({ header: 'Client', key: 'client', width: 20 })
      if (selectedColumns.project) columns.push({ header: 'Project / brand', key: 'projectBrand', width: 20 })
      if (selectedColumns.media) columns.push({ header: 'Media', key: 'media', width: 15 })
      if (selectedColumns.jobType) columns.push({ header: 'Job type', key: 'jobType', width: 20 })
      if (selectedColumns.hours) columns.push({ header: 'Hours', key: 'hours', width: 10 })
      if (selectedColumns.comments) columns.push({ header: 'Comments', key: 'comments', width: 25 })

      worksheet.columns = columns

      // Style the headers
      worksheet.getRow(1).font = { bold: true }
      worksheet.getRow(1).fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFE6E6E6' }
      }

      // Add data rows
      reportData.forEach((row, idx) => {
        console.log(`Processing row ${idx}:`, row);
        
        const rowData: any = {}
        if (selectedColumns.agency) rowData.agency = row.agency || '-'
        if (selectedColumns.fullName) rowData.fullName = row.fullName || '-'
        if (selectedColumns.date) rowData.date = row.date || '-'
        if (selectedColumns.market) rowData.market = row.market || '-'
        if (selectedColumns.company) rowData.company = row.company || '-'
        if (selectedColumns.client) rowData.client = row.client || '-'
        if (selectedColumns.project) rowData.projectBrand = row.projectBrand || row.project || '-'
        if (selectedColumns.media) rowData.media = row.media || '-'
        if (selectedColumns.jobType) rowData.jobType = row.jobType || '-'
        if (selectedColumns.hours) rowData.hours = row.hours
        if (selectedColumns.comments) rowData.comments = row.comments || '-'

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
      <DialogContent className="max-w-5xl">
        <DialogHeader>
          <DialogTitle>{t('admin.reports.downloadDialog.title')}</DialogTitle>
        </DialogHeader>
        <div className="py-4 overflow-x-auto">
          <p className="text-sm text-gray-500 mb-4">
            {t('admin.reports.downloadDialog.description')}
          </p>
          
          <div className="flex flex-row flex-wrap gap-4 mb-6">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="agency"
                checked={selectedColumns.agency}
                onCheckedChange={() => toggleColumn('agency')}
              />
              <label htmlFor="agency" className="text-sm font-medium">Agency</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="fullName"
                checked={selectedColumns.fullName}
                onCheckedChange={() => toggleColumn('fullName')}
              />
              <label htmlFor="fullName" className="text-sm font-medium">Name</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="date"
                checked={selectedColumns.date}
                onCheckedChange={() => toggleColumn('date')}
              />
              <label htmlFor="date" className="text-sm font-medium">Date</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="market"
                checked={selectedColumns.market}
                onCheckedChange={() => toggleColumn('market')}
              />
              <label htmlFor="market" className="text-sm font-medium">Market</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="company"
                checked={selectedColumns.company}
                onCheckedChange={() => toggleColumn('company')}
              />
              <label htmlFor="company" className="text-sm font-medium">Contracting Agency / Unit</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="client"
                checked={selectedColumns.client}
                onCheckedChange={() => toggleColumn('client')}
              />
              <label htmlFor="client" className="text-sm font-medium">Client</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="project"
                checked={selectedColumns.project}
                onCheckedChange={() => toggleColumn('project')}
              />
              <label htmlFor="project" className="text-sm font-medium">Project / brand</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="media"
                checked={selectedColumns.media}
                onCheckedChange={() => toggleColumn('media')}
              />
              <label htmlFor="media" className="text-sm font-medium">Media</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="jobType"
                checked={selectedColumns.jobType}
                onCheckedChange={() => toggleColumn('jobType')}
              />
              <label htmlFor="jobType" className="text-sm font-medium">Job type</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="hours"
                checked={selectedColumns.hours}
                onCheckedChange={() => toggleColumn('hours')}
              />
              <label htmlFor="hours" className="text-sm font-medium">Hours</label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="comments"
                checked={selectedColumns.comments}
                onCheckedChange={() => toggleColumn('comments')}
              />
              <label htmlFor="comments" className="text-sm font-medium">Comments</label>
            </div>
          </div>
          
          <h3 className="text-sm font-medium mb-2">{t('admin.reports.downloadDialog.tablePreview')}</h3>
          <div className="border rounded-md overflow-auto" style={{ maxHeight: "400px" }}>
            <Table>
              <TableHeader>
                <TableRow>
                  {selectedColumns.agency && <TableHead>Agency</TableHead>}
                  {selectedColumns.fullName && <TableHead>Name</TableHead>}
                  {selectedColumns.date && <TableHead>Date</TableHead>}
                  {selectedColumns.market && <TableHead>Market</TableHead>}
                  {selectedColumns.company && <TableHead>Contracting Agency / Unit</TableHead>}
                  {selectedColumns.client && <TableHead>Client</TableHead>}
                  {selectedColumns.project && <TableHead>Project / brand</TableHead>}
                  {selectedColumns.media && <TableHead>Media</TableHead>}
                  {selectedColumns.jobType && <TableHead>Job type</TableHead>}
                  {selectedColumns.hours && <TableHead>Hours</TableHead>}
                  {selectedColumns.comments && <TableHead>Comments</TableHead>}
                </TableRow>
              </TableHeader>
              <TableBody>
                {previewData.map((row, index) => {
                  // Log the row data for debugging
                  console.log(`Preview row ${index}:`, row);
                  
                  // Explicitly check for company field
                  if (!row.company) {
                    console.warn(`Row ${index} missing company field!`, row);
                  }
                  
                  return (
                    <TableRow key={index}>
                      {selectedColumns.agency && <TableCell>{row.agency || '-'}</TableCell>}
                      {selectedColumns.fullName && <TableCell>{row.fullName || '-'}</TableCell>}
                      {selectedColumns.date && <TableCell>{row.date || '-'}</TableCell>}
                      {selectedColumns.market && <TableCell>{row.market || '-'}</TableCell>}
                      {selectedColumns.company && <TableCell>{row.company || '-'}</TableCell>}
                      {selectedColumns.client && <TableCell>{row.client || '-'}</TableCell>}
                      {selectedColumns.project && <TableCell>{row.projectBrand || row.project || '-'}</TableCell>}
                      {selectedColumns.media && <TableCell>{row.media || '-'}</TableCell>}
                      {selectedColumns.jobType && <TableCell>{row.jobType || '-'}</TableCell>}
                      {selectedColumns.hours && <TableCell>{row.hours}</TableCell>}
                      {selectedColumns.comments && <TableCell>{row.comments || '-'}</TableCell>}
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            {t('admin.reports.downloadDialog.cancel')}
          </Button>
          <Button onClick={downloadReport}>
            {t('admin.reports.downloadDialog.download')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
} 