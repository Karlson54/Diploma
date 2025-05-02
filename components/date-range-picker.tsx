"use client"
import { useState } from "react"
import { format } from "date-fns"
import { CalendarIcon, ChevronLeft, ChevronRight, Check } from "lucide-react"
import type { DateRange } from "react-day-picker"
import { useTranslation } from "react-i18next"

import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Calendar } from "@/components/ui/calendar"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Label } from "@/components/ui/label"

interface DatePickerWithRangeProps {
  date: DateRange | undefined
  setDate: (date: DateRange | undefined) => void
}

export function DatePickerWithRange({ date, setDate }: DatePickerWithRangeProps) {
  const { t, i18n } = useTranslation()
  // State for current month view in the calendar
  const [currentMonth, setCurrentMonth] = useState<Date>(new Date());
  const [localDateRange, setLocalDateRange] = useState<DateRange | undefined>(date);
  const [isOpen, setIsOpen] = useState(false);
  
  const locale = i18n.language === 'uk' ? 'uk-UA' : 'en-US';
  const monthName = currentMonth.toLocaleDateString(locale, { month: 'long', year: 'numeric' });
  
  // Calendar helper functions
  const getDaysInMonth = (year: number, month: number) => {
    return new Date(year, month + 1, 0).getDate();
  };

  const getFirstDayOfMonth = (year: number, month: number) => {
    return new Date(year, month, 1).getDay();
  };

  const generateCalendarDays = (year: number, month: number) => {
    const daysInMonth = getDaysInMonth(year, month)
    const firstDay = getFirstDayOfMonth(year, month)

    // Get days from previous month to fill the first week
    const daysFromPrevMonth = firstDay === 0 ? 6 : firstDay - 1
    const prevMonth = month === 0 ? 11 : month - 1
    const prevMonthYear = month === 0 ? year - 1 : year
    const daysInPrevMonth = getDaysInMonth(prevMonthYear, prevMonth)

    const prevMonthDays = []
    for (let i = daysInPrevMonth - daysFromPrevMonth + 1; i <= daysInPrevMonth; i++) {
      prevMonthDays.push({
        day: i,
        month: prevMonth,
        year: prevMonthYear,
        isCurrentMonth: false,
      })
    }

    // Current month days
    const currentMonthDays = []
    for (let i = 1; i <= daysInMonth; i++) {
      currentMonthDays.push({
        day: i,
        month,
        year,
        isCurrentMonth: true,
      })
    }

    // Next month days to fill the last week
    const totalDaysShown = Math.ceil((daysFromPrevMonth + daysInMonth) / 7) * 7
    const daysFromNextMonth = totalDaysShown - (daysFromPrevMonth + daysInMonth)
    const nextMonth = month === 11 ? 0 : month + 1
    const nextMonthYear = month === 11 ? year + 1 : year

    const nextMonthDays = []
    for (let i = 1; i <= daysFromNextMonth; i++) {
      nextMonthDays.push({
        day: i,
        month: nextMonth,
        year: nextMonthYear,
        isCurrentMonth: false,
      })
    }

    return [...prevMonthDays, ...currentMonthDays, ...nextMonthDays]
  };

  const isDateSelected = (day: number, month: number, year: number) => {
    if (!localDateRange) return false;
    
    // Check if this date is within the selected range
    const currentDate = new Date(year, month, day);
    const from = localDateRange.from;
    const to = localDateRange.to || localDateRange.from;
    
    return from && to && currentDate >= from && currentDate <= to;
  };

  const isStartDate = (day: number, month: number, year: number) => {
    if (!localDateRange || !localDateRange.from) return false;
    
    const currentDate = new Date(year, month, day);
    const from = localDateRange.from;
    
    return currentDate.getDate() === from.getDate() && 
           currentDate.getMonth() === from.getMonth() && 
           currentDate.getFullYear() === from.getFullYear();
  };

  const isEndDate = (day: number, month: number, year: number) => {
    if (!localDateRange || !localDateRange.to) return false;
    
    const currentDate = new Date(year, month, day);
    const to = localDateRange.to;
    
    return currentDate.getDate() === to.getDate() && 
           currentDate.getMonth() === to.getMonth() && 
           currentDate.getFullYear() === to.getFullYear();
  };

  const handleDateSelection = (day: number, month: number, year: number) => {
    const selectedDate = new Date(year, month, day);
    
    if (!localDateRange || !localDateRange.from || localDateRange.to) {
      // New selection or complete selection - start new from date
      setLocalDateRange({ from: selectedDate, to: undefined });
    } else {
      // We have a from date but no to date
      const from = localDateRange.from;
      
      if (selectedDate < from) {
        // If clicking earlier than the from date, make it the new from
        setLocalDateRange({ from: selectedDate, to: from });
      } else {
        // Complete the range
        setLocalDateRange({ from, to: selectedDate });
      }
    }
  };

  const goToPreviousMonth = () => {
    const newMonth = new Date(currentMonth);
    newMonth.setMonth(newMonth.getMonth() - 1);
    setCurrentMonth(newMonth);
  };

  const goToNextMonth = () => {
    const newMonth = new Date(currentMonth);
    newMonth.setMonth(newMonth.getMonth() + 1);
    setCurrentMonth(newMonth);
  };

  const applyDateRange = () => {
    setDate(localDateRange);
    setIsOpen(false);
  };

  const clearSelection = () => {
    setLocalDateRange(undefined);
  };

  const handleOpenChange = (open: boolean) => {
    if (open) {
      setLocalDateRange(date);
    }
    setIsOpen(open);
  };

  const calendarDays = generateCalendarDays(currentMonth.getFullYear(), currentMonth.getMonth());

  return (
    <div className="grid gap-2">
      <Popover open={isOpen} onOpenChange={handleOpenChange}>
        <PopoverTrigger asChild>
          <Button
            id="date"
            variant={"outline"}
            className={cn("w-full justify-start text-left font-normal", !date && "text-muted-foreground")}
          >
            <CalendarIcon className="mr-2 h-4 w-4" />
            {date?.from ? (
              date.to ? (
                <>
                  {format(date.from, "dd.MM.yyyy")} - {format(date.to, "dd.MM.yyyy")}
                </>
              ) : (
                format(date.from, "dd.MM.yyyy")
              )
            ) : (
              <span>{t('dateRangePicker.placeholder')}</span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-[550px] p-4" align="start">
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <Button variant="outline" size="icon" onClick={goToPreviousMonth}>
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <div className="font-medium">{monthName}</div>
              <Button variant="outline" size="icon" onClick={goToNextMonth}>
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>

            <div className="grid grid-cols-7 gap-1 text-center mb-1">
              {Array.from({ length: 7 }, (_v, i) => (
                <div key={i} className="text-xs font-medium py-1">
                  {t(`calendar.weekdayShort.${i}`)}
                </div>
              ))}
            </div>

            <div className="grid grid-cols-7 gap-1">
              {calendarDays.map((day, i) => {
                const isStart = isStartDate(day.day, day.month, day.year);
                const isEnd = isEndDate(day.day, day.month, day.year);
                const isSelected = isDateSelected(day.day, day.month, day.year);
                
                return (
                  <Button
                    key={i}
                    variant={isSelected ? "default" : "outline"}
                    className={cn(
                      "h-9 p-0 relative",
                      !day.isCurrentMonth ? "text-gray-400" : "",
                      isStart && "bg-primary text-primary-foreground rounded-l-md rounded-r-none",
                      isEnd && "bg-primary text-primary-foreground rounded-r-md rounded-l-none",
                      isSelected && !isStart && !isEnd && "rounded-none"
                    )}
                    onClick={() => handleDateSelection(day.day, day.month, day.year)}
                  >
                    {day.day}
                    {isStart && (
                      <span className="absolute -bottom-1 left-1/2 transform -translate-x-1/2 text-[10px]">Від</span>
                    )}
                    {isEnd && (
                      <span className="absolute -bottom-1 left-1/2 transform -translate-x-1/2 text-[10px]">До</span>
                    )}
                  </Button>
                )
              })}
            </div>

            <div className="flex items-center space-x-2">
              {localDateRange?.from && (
                <div className="text-sm">
                  <span className="font-medium">Обраний період:</span>{" "}
                  {format(localDateRange.from, "dd.MM.yyyy")}
                  {localDateRange.to ? ` - ${format(localDateRange.to, "dd.MM.yyyy")}` : " (виберіть кінцеву дату)"}
                </div>
              )}
            </div>

            <div className="flex justify-between pt-2">
              <Button variant="outline" size="sm" onClick={clearSelection}>
                Очистити
              </Button>
              <Button size="sm" onClick={applyDateRange} disabled={!localDateRange?.from || !localDateRange?.to}>
                <Check className="h-4 w-4 mr-2" />
                Застосувати
              </Button>
            </div>
          </div>
        </PopoverContent>
      </Popover>
    </div>
  )
}
