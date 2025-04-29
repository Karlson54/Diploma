"use client"
import { format } from "date-fns"
import { uk } from "date-fns/locale"
import { CalendarIcon } from "lucide-react"
import type { DateRange } from "react-day-picker"

import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Calendar } from "@/components/ui/calendar"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"

interface DatePickerWithRangeProps {
  date: DateRange | undefined
  setDate: (date: DateRange | undefined) => void
}

export function DatePickerWithRange({ date, setDate }: DatePickerWithRangeProps) {
  return (
    <div className="grid gap-2">
      <Popover>
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
              <span>Виберіть період</span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            initialFocus
            mode="range"
            defaultMonth={date?.from}
            selected={date}
            onSelect={setDate}
            numberOfMonths={2}
            locale={uk}
            weekStartsOn={1}
            showOutsideDays={true}
            classNames={{
              caption_label: "text-sm font-medium",
              head_cell: "text-muted-foreground rounded-md w-9 font-normal text-[0.8rem] uppercase",
              day: cn(
                "h-9 w-9 p-0 font-normal text-center text-sm"
              ),
              day_today: "bg-accent text-accent-foreground font-medium",
              day_selected: "bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground focus:bg-primary focus:text-primary-foreground"
            }}
          />
        </PopoverContent>
      </Popover>
    </div>
  )
}
