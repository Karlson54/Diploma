"use client"

import React from "react"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Command, CommandEmpty, CommandGroup, CommandItem, CommandList } from "@/components/ui/command"
import { Check } from "lucide-react"
import { cn } from "@/lib/utils"
import { useTranslation } from "react-i18next"

interface DayEntryFormProps {
  date: Date
  fields?: {
    market?: boolean
    contractingAgency?: boolean
    client?: boolean
    projectBrand?: boolean
    media?: boolean
    jobType?: boolean
    comments?: boolean
    hours?: boolean
  }
  compact?: boolean
  initialValues?: any
  filterStartsWith?: boolean
  showInputInField?: boolean
  onClose: () => void
  onSave: (data: any) => void
}

export function DayEntryForm({
  date,
  fields = {},
  compact = false,
  initialValues,
  filterStartsWith = false,
  showInputInField = false,
  onClose,
  onSave,
}: DayEntryFormProps) {
  // i18n
  const { t } = useTranslation()

  // Список клієнтів
  const clients = [
    { id: "1", name: "All clients" },
    { id: "2", name: "NewBiz" },
    { id: "3", name: "Adidas/Reebook" },
    { id: "4", name: "Adobe" },
    { id: "5", name: "Akzo Nobel" },
    { id: "6", name: "Asahi" },
    { id: "7", name: "Asatsu-DK (ADK)" },
    { id: "8", name: "Audemars Piguet" },
    { id: "9", name: "AXA" },
    { id: "10", name: "Bayer" },
    { id: "11", name: "Beiersdorf" },
    { id: "12", name: "Beko" },
    { id: "13", name: "Benetton" },
    { id: "14", name: "BGL Group" },
    { id: "15", name: "Blackrock" },
    { id: "16", name: "Booking.com" },
    { id: "17", name: "BP" },
    { id: "18", name: "Bridgestone" },
    { id: "19", name: "Bumble Holdings" },
    { id: "20", name: "Calvin Klein" },
    { id: "21", name: "Campari" },
    { id: "22", name: "Cartier" },
    { id: "23", name: "Chevron" },
    { id: "24", name: "Coca-Cola" },
    { id: "25", name: "Colgate-Palmolive" },
    { id: "26", name: "Collistar" },
    { id: "27", name: "Continental" },
    { id: "28", name: "Danone" },
    { id: "29", name: "Darnitsa" },
    { id: "30", name: "Deloitte" },
    { id: "31", name: "Deutsche Post DHL" },
    { id: "32", name: "Deutsche Telekom" },
    { id: "33", name: "Discovery" },
    { id: "34", name: "DoorDash, Inc." },
    { id: "35", name: "Dr Oetker" },
    { id: "36", name: "Duracell" },
    { id: "37", name: "EA GAMES" },
    { id: "38", name: "Edgewell - Global" },
    { id: "39", name: "Energizer" },
    { id: "40", name: "Erste Bank" },
    { id: "41", name: "Essilor" },
    { id: "42", name: "Falcon and Associates FZ" },
    { id: "43", name: "Ford" },
    { id: "44", name: "Formula 1" },
    { id: "45", name: "Fozzy Group" },
    { id: "46", name: "FrieslandCampina" },
    { id: "47", name: "Garden Care Bidco Limited" },
    { id: "48", name: "GE" },
    { id: "49", name: "Geberit" },
    { id: "50", name: "General Mills" },
    { id: "51", name: "GoDaddy.com" },
    { id: "52", name: "Haribo" },
    { id: "53", name: "Hasbro" },
    { id: "54", name: "Hawley & Hazel (H&H)" },
    { id: "55", name: "HBO" },
    { id: "56", name: "Helly Hansen" },
    { id: "57", name: "Henkel" },
    { id: "58", name: "Honor Global Master Purchase" },
    { id: "59", name: "Huawei" },
    { id: "60", name: "Husqvarna" },
    { id: "61", name: "IAG (British Airways)" },
    { id: "62", name: "IBM" },
    { id: "63", name: "IHG" },
    { id: "64", name: "IKEA" },
    { id: "65", name: "Jaco" },
    { id: "66", name: "Jetstar" },
    { id: "67", name: "Johnson and Johnson" },
    { id: "68", name: "Kapp-Ahl" },
    { id: "69", name: "Karcher" },
    { id: "70", name: "Kimberly-Clark" },
    { id: "71", name: "Kingfisher" },
    { id: "72", name: "Lavazza" },
    { id: "73", name: "Lektravy" },
    { id: "74", name: "Lombard Odier" },
    { id: "75", name: "L'Oreal" },
    { id: "76", name: "Lufthansa" },
    { id: "77", name: "LVMH" },
    { id: "78", name: "MARS" },
    { id: "79", name: "McArthurGlen" },
    { id: "80", name: "Menarini" },
    { id: "81", name: "Mondelez" },
    { id: "82", name: "MSC Cruises" },
    { id: "83", name: "Nestlé" },
    { id: "84", name: "Netflix" },
    { id: "85", name: "Next" },
    { id: "86", name: "NFL Gamepass (Overtier)" },
    { id: "87", name: "Nike" },
    { id: "88", name: "Osram" },
    { id: "89", name: "P&G" },
    { id: "90", name: "Paramount" },
    { id: "91", name: "Pepsi" },
    { id: "92", name: "Perfetti Van Melle" },
    { id: "93", name: "Perrigo" },
    { id: "94", name: "Pfizer" },
    { id: "95", name: "Pripravka" },
    { id: "96", name: "Recordati" },
    { id: "97", name: "Rolex" },
    { id: "98", name: "Royal Canin" },
    { id: "99", name: "Savencia" },
    { id: "100", name: "Shell" },
    { id: "101", name: "Skechers" },
    { id: "102", name: "Sony Playstation" },
    { id: "103", name: "Subaru/Nissan" },
    { id: "104", name: "Suntory" },
    { id: "105", name: "Tata Global Beverages" },
    { id: "106", name: "Tiffany" },
    { id: "107", name: "Total Energy" },
    { id: "108", name: "Toyota" },
    { id: "109", name: "Trading" },
    { id: "110", name: "Triumph" },
    { id: "111", name: "Ubisoft" },
    { id: "112", name: "UIP" },
    { id: "113", name: "Unilever" },
    { id: "114", name: "Versace" },
    { id: "115", name: "Vodafone" },
    { id: "116", name: "Volvo" },
    { id: "117", name: "Weightwatchers" },
    { id: "118", name: "Whirlpool" },
    { id: "119", name: "Xerox" },
    { id: "120", name: "Xiaomi" },
    { id: "121", name: "Yum!" },
    { id: "122", name: "Indeed" },
    { id: "123", name: "Innocent" },
    { id: "124", name: "Vacheron" },
    { id: "125", name: "Klarna" },
    { id: "126", name: "Breuninger" },
    { id: "127", name: "CCHBC" },
  ]

  // Приклад списку ринків
  const markets = [
    { id: "1", name: "Україна" },
    { id: "2", name: "Європа" },
    { id: "3", name: "США" },
    { id: "4", name: "Азія" },
    { id: "5", name: "Глобальний" },
  ]

  // Список агентств
  const agencies = [
    { id: "1", name: "GroupM" },
    { id: "2", name: "MediaCom" },
    { id: "3", name: "Mindshare" },
    { id: "4", name: "Wavemaker" },
  ]

  // Список типів медіа
  const mediaTypes = [
    { id: "1", name: "OOH" },
    { id: "2", name: "Other" },
    { id: "3", name: "Print" },
    { id: "4", name: "Radio" },
    { id: "5", name: "Research" },
    { id: "6", name: "Trading" },
    { id: "7", name: "TV" },
    { id: "8", name: "TVs" },
    { id: "9", name: "Digital - all" },
    { id: "10", name: "Digital - Paid Social" },
    { id: "11", name: "Digital - Paid Search" },
    { id: "12", name: "Digital - Display" },
    { id: "13", name: "Digital - Video" },
    { id: "14", name: "Digital SP" },
    { id: "15", name: "Digital Commerce" },
    { id: "16", name: "Digital Influencers" },
    { id: "17", name: "Digital other" },
  ]

  // Список типів робіт
  const jobTypes = [
    { id: "1", name: "Strategy planning" },
    { id: "2", name: "Media plans" },
    { id: "3", name: "Campaign running" },
    { id: "4", name: "Reporting" },
    { id: "5", name: "Docs and finances" },
    { id: "6", name: "Research" },
    { id: "7", name: "Self education" },
    { id: "8", name: "Vacation" },
    { id: "9", name: "Other" },
  ]

  // Определяем, находимся ли мы в режиме редактирования
  const isEditMode = initialValues !== null && initialValues !== undefined;

  // Начальные значения для formData
  const defaultFormValues = {
    market: "",
    contractingAgency: "",
    client: "",
    projectBrand: "",
    media: "",
    jobType: "",
    comments: "",
    hours: "60", // 60 минут = 1 час по умолчанию
  };

  // Функция для преобразования ID в имя элемента
  const getNameFromId = (id: string, items: Array<{ id: string, name: string }>) => {
    const item = items.find(item => item.id === id);
    return item ? item.name : "";
  };

  // Используем ключ (key), чтобы пересоздавать компонент при переключении между режимами.
  // Это гарантирует сброс всех внутренних состояний.
  const formKey = isEditMode ? `edit-${initialValues?.id}` : 'new-entry';

  // Состояния формы - инициализируем сразу с нужными значениями
  const [formData, setFormData] = useState(isEditMode ? {
    market: initialValues?.market || "",
    contractingAgency: initialValues?.contractingAgency || "",
    client: initialValues?.client || "",
    projectBrand: initialValues?.projectBrand || "",
    media: initialValues?.media || "",
    jobType: initialValues?.jobType || "",
    comments: initialValues?.comments || "",
    hours: initialValues?.hours ? initialValues.hours.toString() : "60",
  } : defaultFormValues);
  
  // Состояния для инпутов (текст в полях селектов)
  const [marketInput, setMarketInput] = useState(
    isEditMode ? getNameFromId(initialValues?.market, markets) || initialValues?.market || "" : ""
  );
  
  const [agencyInput, setAgencyInput] = useState(
    isEditMode ? getNameFromId(initialValues?.contractingAgency, agencies) || initialValues?.contractingAgency || "" : ""
  );
  
  const [clientInput, setClientInput] = useState(
    isEditMode ? getNameFromId(initialValues?.client, clients) || initialValues?.client || "" : ""
  );
  
  const [mediaInput, setMediaInput] = useState(
    isEditMode ? getNameFromId(initialValues?.media, mediaTypes) || initialValues?.media || "" : ""
  );
  
  const [jobTypeInput, setJobTypeInput] = useState(
    isEditMode ? getNameFromId(initialValues?.jobType, jobTypes) || initialValues?.jobType || "" : ""
  );

  // Состояния для выпадающих списков
  const [clientPopoverOpen, setClientPopoverOpen] = useState(false);
  const [marketPopoverOpen, setMarketPopoverOpen] = useState(false);
  const [agencyPopoverOpen, setAgencyPopoverOpen] = useState(false);
  const [mediaPopoverOpen, setMediaPopoverOpen] = useState(false);
  const [jobTypePopoverOpen, setJobTypePopoverOpen] = useState(false);

  // Обработчик клика вне компонента для закрытия выпадающих списков
  React.useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (!event.target || !(event.target instanceof Element)) return;

      if (clientPopoverOpen && !event.target.closest("[data-client-dropdown]")) {
        setClientPopoverOpen(false);
      }
      if (marketPopoverOpen && !event.target.closest("[data-market-dropdown]")) {
        setMarketPopoverOpen(false);
      }
      if (agencyPopoverOpen && !event.target.closest("[data-agency-dropdown]")) {
        setAgencyPopoverOpen(false);
      }
      if (mediaPopoverOpen && !event.target.closest("[data-media-dropdown]")) {
        setMediaPopoverOpen(false);
      }
      if (jobTypePopoverOpen && !event.target.closest("[data-jobtype-dropdown]")) {
        setJobTypePopoverOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [clientPopoverOpen, marketPopoverOpen, agencyPopoverOpen, mediaPopoverOpen, jobTypePopoverOpen]);

  // Обработчик отправки формы
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave({
      date: date,
      ...formData,
    });
  };

  // Рендеринг компактных полей формы
  const renderField = (id: string, label: string, component: React.ReactNode) => {
    return (
      <div className="space-y-1">
        <Label htmlFor={id} className="text-xs">
          {label}
        </Label>
        {component}
      </div>
    );
  };

  return (
    <form key={formKey} onSubmit={handleSubmit} className="space-y-4">
      {compact ? (
        <>
          {/* Компактная форма в три ряда */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            {fields.market &&
              renderField(
                "market",
                t('calendar.market'),
                <div className="relative w-full" data-market-dropdown>
                  <Input
                    className="h-8 text-sm"
                    placeholder={t('calendar.selectMarket')}
                    value={marketInput}
                    onChange={(e) => {
                      setMarketInput(e.target.value);
                      setMarketPopoverOpen(true);
                    }}
                    onFocus={() => setMarketPopoverOpen(true)}
                  />
                  <div
                    className={
                      marketPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                    }
                  >
                    <Command>
                      <CommandEmpty>{t('calendar.marketNotFound')}</CommandEmpty>
                      <CommandGroup>
                        <CommandList className="max-h-[200px] overflow-y-auto">
                          {markets
                            .filter((market) =>
                              filterStartsWith
                                ? market.name.toLowerCase().startsWith(marketInput.toLowerCase())
                                : market.name.toLowerCase().includes(marketInput.toLowerCase())
                            )
                            .map((market) => (
                              <CommandItem
                                key={market.id}
                                value={market.name}
                                onSelect={() => {
                                  setFormData({ ...formData, market: market.id });
                                  setMarketInput(market.name);
                                  setMarketPopoverOpen(false);
                                }}
                              >
                                {market.name}
                                <Check
                                  className={cn(
                                    "ml-auto h-4 w-4",
                                    formData.market === market.id ? "opacity-100" : "opacity-0"
                                  )}
                                />
                              </CommandItem>
                            ))}
                        </CommandList>
                      </CommandGroup>
                    </Command>
                  </div>
                </div>
              )}

            {fields.contractingAgency &&
              renderField(
                "contractingAgency",
                t('calendar.agency'),
                <div className="relative w-full" data-agency-dropdown>
                  <Input
                    className="h-8 text-sm"
                    placeholder={t('calendar.selectAgency')}
                    value={agencyInput}
                    onChange={(e) => {
                      setAgencyInput(e.target.value)
                      setAgencyPopoverOpen(true)
                    }}
                    onFocus={() => setAgencyPopoverOpen(true)}
                  />
                  <div
                    className={
                      agencyPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                    }
                  >
                    <Command>
                      <CommandEmpty>{t('calendar.agencyNotFound')}</CommandEmpty>
                      <CommandGroup>
                        <CommandList className="max-h-[200px] overflow-y-auto">
                          {agencies
                            .filter((agency) =>
                              filterStartsWith
                                ? agency.name.toLowerCase().startsWith(agencyInput.toLowerCase())
                                : agency.name.toLowerCase().includes(agencyInput.toLowerCase()),
                            )
                            .map((agency) => (
                              <CommandItem
                                key={agency.id}
                                value={agency.name}
                                onSelect={() => {
                                  setFormData({ ...formData, contractingAgency: agency.id })
                                  setAgencyInput(agency.name)
                                  setAgencyPopoverOpen(false)
                                }}
                              >
                                {agency.name}
                                <Check
                                  className={cn(
                                    "ml-auto h-4 w-4",
                                    formData.contractingAgency === agency.id ? "opacity-100" : "opacity-0",
                                  )}
                                />
                              </CommandItem>
                            ))}
                        </CommandList>
                      </CommandGroup>
                    </Command>
                  </div>
                </div>,
              )}

            {fields.client &&
              renderField(
                "client",
                t('calendar.client'),
                <div className="relative w-full" data-client-dropdown>
                  <Input
                    className="h-8 text-sm"
                    placeholder={t('calendar.selectClient')}
                    value={clientInput}
                    onChange={(e) => {
                      setClientInput(e.target.value)
                      setClientPopoverOpen(true) // Открываем попап при вводе текста
                    }}
                    onFocus={() => setClientPopoverOpen(true)}
                  />
                  <div
                    className={
                      clientPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                    }
                  >
                    <Command>
                      <CommandEmpty>{t('calendar.clientNotFound')}</CommandEmpty>
                      <CommandGroup>
                        <CommandList className="max-h-[200px] overflow-y-auto">
                          {clients
                            .filter((client) =>
                              filterStartsWith
                                ? client.name.toLowerCase().startsWith(clientInput.toLowerCase())
                                : client.name.toLowerCase().includes(clientInput.toLowerCase()),
                            )
                            .map((client) => (
                              <CommandItem
                                key={client.id}
                                value={client.name}
                                onSelect={() => {
                                  setFormData({ ...formData, client: client.id })
                                  setClientInput(client.name)
                                  setClientPopoverOpen(false)
                                }}
                              >
                                {client.name}
                                <Check
                                  className={cn(
                                    "ml-auto h-4 w-4",
                                    formData.client === client.id ? "opacity-100" : "opacity-0",
                                  )}
                                />
                              </CommandItem>
                            ))}
                        </CommandList>
                      </CommandGroup>
                    </Command>
                  </div>
                </div>,
              )}
          </div>

          {/* Второй ряд: Project/Brand, Media, Job Type, Hours */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
            {fields.media &&
              renderField(
                "media",
                t('calendar.media'),
                <div className="relative w-full" data-media-dropdown>
                  <Input
                    className="h-8 text-sm"
                    placeholder={t('calendar.selectMedia')}
                    value={mediaInput}
                    onChange={(e) => {
                      setMediaInput(e.target.value)
                      setMediaPopoverOpen(true)
                    }}
                    onFocus={() => setMediaPopoverOpen(true)}
                  />
                  <div
                    className={
                      mediaPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                    }
                  >
                    <Command>
                      <CommandEmpty>{t('calendar.mediaNotFound')}</CommandEmpty>
                      <CommandGroup>
                        <CommandList className="max-h-[200px] overflow-y-auto">
                          {mediaTypes
                            .filter((media) =>
                              filterStartsWith
                                ? media.name.toLowerCase().startsWith(mediaInput.toLowerCase())
                                : media.name.toLowerCase().includes(mediaInput.toLowerCase()),
                            )
                            .map((media) => (
                              <CommandItem
                                key={media.id}
                                value={media.name}
                                onSelect={() => {
                                  setFormData({ ...formData, media: media.id })
                                  setMediaInput(media.name)
                                  setMediaPopoverOpen(false)
                                }}
                              >
                                {media.name}
                                <Check
                                  className={cn(
                                    "ml-auto h-4 w-4",
                                    formData.media === media.id ? "opacity-100" : "opacity-0",
                                  )}
                                />
                              </CommandItem>
                            ))}
                        </CommandList>
                      </CommandGroup>
                    </Command>
                  </div>
                </div>,
              )}

            {fields.jobType &&
              renderField(
                "jobType",
                t('calendar.jobType'),
                <div className="relative w-full" data-jobtype-dropdown>
                  <Input
                    className="h-8 text-sm"
                    placeholder={t('calendar.selectJobType')}
                    value={jobTypeInput}
                    onChange={(e) => {
                      setJobTypeInput(e.target.value)
                      setJobTypePopoverOpen(true)
                    }}
                    onFocus={() => setJobTypePopoverOpen(true)}
                  />
                  <div
                    className={
                      jobTypePopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                    }
                  >
                    <Command>
                      <CommandEmpty>{t('calendar.jobTypeNotFound')}</CommandEmpty>
                      <CommandGroup>
                        <CommandList className="max-h-[200px] overflow-y-auto">
                          {jobTypes
                            .filter((jobType) =>
                              filterStartsWith
                                ? jobType.name.toLowerCase().startsWith(jobTypeInput.toLowerCase())
                                : jobType.name.toLowerCase().includes(jobTypeInput.toLowerCase()),
                            )
                            .map((jobType) => (
                              <CommandItem
                                key={jobType.id}
                                value={jobType.name}
                                onSelect={() => {
                                  setFormData({ ...formData, jobType: jobType.id })
                                  setJobTypeInput(jobType.name)
                                  setJobTypePopoverOpen(false)
                                }}
                              >
                                {jobType.name}
                                <Check
                                  className={cn(
                                    "ml-auto h-4 w-4",
                                    formData.jobType === jobType.id ? "opacity-100" : "opacity-0",
                                  )}
                                />
                              </CommandItem>
                            ))}
                        </CommandList>
                      </CommandGroup>
                    </Command>
                  </div>
                </div>,
              )}

            {fields.hours &&
              renderField(
                "hours",
                t('calendar.spentTime'),
                <Input
                  id="hours"
                  type="number"
                  min="1"
                  step="1"
                  className="h-8 text-sm"
                  placeholder={t('calendar.minutesHint')}
                  value={formData.hours}
                  onChange={(e) => setFormData({ ...formData, hours: e.target.value })}
                  required
                />,
              )}
          </div>

          {/* Project/Brand и Comments в одном ряду */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            {fields.projectBrand &&
              renderField(
                "projectBrand",
                t('calendar.projectBrand'),
                <Input
                  id="projectBrand"
                  className="h-16 text-sm"
                  value={formData.projectBrand}
                  onChange={(e) => setFormData({ ...formData, projectBrand: e.target.value })}
                  required
                />,
              )}

            {fields.comments &&
              renderField(
                "comments",
                t('calendar.comments'),
                <Textarea
                  id="comments"
                  placeholder={t('calendar.commentsPlaceholder')}
                  className="min-h-[64px] text-sm"
                  value={formData.comments}
                  onChange={(e) => setFormData({ ...formData, comments: e.target.value })}
                />,
              )}
          </div>
        </>
      ) : (
        // Оригінальний вертикальний режим відображення
        <>
          {fields.market && (
            <div className="space-y-2">
              <Label htmlFor="market">Market</Label>
              <div className="relative w-full" data-market-dropdown>
                <Input
                  className="w-full"
                  placeholder="Select market"
                  value={marketInput}
                  onChange={(e) => {
                    setMarketInput(e.target.value)
                    setMarketPopoverOpen(true)
                  }}
                  onFocus={() => setMarketPopoverOpen(true)}
                />
                <div
                  className={
                    marketPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                  }
                >
                  <Command>
                    <CommandEmpty>Market not found.</CommandEmpty>
                    <CommandGroup>
                      <CommandList className="max-h-[300px] overflow-y-auto">
                        {markets
                          .filter((market) =>
                            filterStartsWith
                              ? market.name.toLowerCase().startsWith(marketInput.toLowerCase())
                              : market.name.toLowerCase().includes(marketInput.toLowerCase()),
                          )
                          .map((market) => (
                            <CommandItem
                              key={market.id}
                              value={market.name}
                              onSelect={() => {
                                setFormData({ ...formData, market: market.id })
                                setMarketInput(market.name)
                                setMarketPopoverOpen(false)
                              }}
                            >
                              {market.name}
                              <Check
                                className={cn(
                                  "ml-auto h-4 w-4",
                                  formData.market === market.id ? "opacity-100" : "opacity-0",
                                )}
                              />
                            </CommandItem>
                          ))}
                      </CommandList>
                    </CommandGroup>
                  </Command>
                </div>
              </div>
            </div>
          )}

          {fields.contractingAgency && (
            <div className="space-y-2">
              <Label htmlFor="contractingAgency">Contracting Agency/Unit</Label>
              <div className="relative w-full" data-agency-dropdown>
                <Input
                  className="w-full"
                  placeholder="Select agency"
                  value={agencyInput}
                  onChange={(e) => {
                    setAgencyInput(e.target.value)
                    setAgencyPopoverOpen(true)
                  }}
                  onFocus={() => setAgencyPopoverOpen(true)}
                />
                <div
                  className={
                    agencyPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                  }
                >
                  <Command>
                    <CommandEmpty>Agency not found.</CommandEmpty>
                    <CommandGroup>
                      <CommandList className="max-h-[300px] overflow-y-auto">
                        {agencies
                          .filter((agency) =>
                            filterStartsWith
                              ? agency.name.toLowerCase().startsWith(agencyInput.toLowerCase())
                              : agency.name.toLowerCase().includes(agencyInput.toLowerCase()),
                          )
                          .map((agency) => (
                            <CommandItem
                              key={agency.id}
                              value={agency.name}
                              onSelect={() => {
                                setFormData({ ...formData, contractingAgency: agency.id })
                                setAgencyInput(agency.name)
                                setAgencyPopoverOpen(false)
                              }}
                            >
                              {agency.name}
                              <Check
                                className={cn(
                                  "ml-auto h-4 w-4",
                                  formData.contractingAgency === agency.id ? "opacity-100" : "opacity-0",
                                )}
                              />
                            </CommandItem>
                          ))}
                      </CommandList>
                    </CommandGroup>
                  </Command>
                </div>
              </div>
            </div>
          )}

          {fields.client && (
            <div className="space-y-2">
              <Label htmlFor="client">Client</Label>
              <div className="relative w-full" data-client-dropdown>
                <Input
                  className="w-full"
                  placeholder="Select client"
                  value={clientInput}
                  onChange={(e) => {
                    setClientInput(e.target.value)
                    setClientPopoverOpen(true) // Открываем попап при вводе текста
                  }}
                  onFocus={() => setClientPopoverOpen(true)}
                />
                <div
                  className={
                    clientPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                  }
                >
                  <Command>
                    <CommandEmpty>Client not found.</CommandEmpty>
                    <CommandGroup>
                      <CommandList className="max-h-[300px] overflow-y-auto">
                        {clients
                          .filter((client) =>
                            filterStartsWith
                              ? client.name.toLowerCase().startsWith(clientInput.toLowerCase())
                              : client.name.toLowerCase().includes(clientInput.toLowerCase()),
                          )
                          .map((client) => (
                            <CommandItem
                              key={client.id}
                              value={client.name}
                              onSelect={() => {
                                setFormData({ ...formData, client: client.id })
                                setClientInput(client.name)
                                setClientPopoverOpen(false)
                              }}
                            >
                              {client.name}
                              <Check
                                className={cn(
                                  "ml-auto h-4 w-4",
                                  formData.client === client.id ? "opacity-100" : "opacity-0",
                                )}
                              />
                            </CommandItem>
                          ))}
                      </CommandList>
                    </CommandGroup>
                  </Command>
                </div>
              </div>
            </div>
          )}

          {fields.projectBrand && (
            <div className="space-y-2">
              <Label htmlFor="projectBrand">Project/Brand</Label>
              <Input
                id="projectBrand"
                value={formData.projectBrand}
                onChange={(e) => setFormData({ ...formData, projectBrand: e.target.value })}
                required
              />
            </div>
          )}

          {fields.media && (
            <div className="space-y-2">
              <Label htmlFor="media">Media</Label>
              <div className="relative w-full" data-media-dropdown>
                <Input
                  className="w-full"
                  placeholder="Select media type"
                  value={mediaInput}
                  onChange={(e) => {
                    setMediaInput(e.target.value)
                    setMediaPopoverOpen(true)
                  }}
                  onFocus={() => setMediaPopoverOpen(true)}
                />
                <div
                  className={
                    mediaPopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                  }
                >
                  <Command>
                    <CommandEmpty>Media type not found.</CommandEmpty>
                    <CommandGroup>
                      <CommandList className="max-h-[300px] overflow-y-auto">
                        {mediaTypes
                          .filter((media) =>
                            filterStartsWith
                              ? media.name.toLowerCase().startsWith(mediaInput.toLowerCase())
                              : media.name.toLowerCase().includes(mediaInput.toLowerCase()),
                          )
                          .map((media) => (
                            <CommandItem
                              key={media.id}
                              value={media.name}
                              onSelect={() => {
                                setFormData({ ...formData, media: media.id })
                                setMediaInput(media.name)
                                setMediaPopoverOpen(false)
                              }}
                            >
                              {media.name}
                              <Check
                                className={cn(
                                  "ml-auto h-4 w-4",
                                  formData.media === media.id ? "opacity-100" : "opacity-0",
                                )}
                              />
                            </CommandItem>
                          ))}
                      </CommandList>
                    </CommandGroup>
                  </Command>
                </div>
              </div>
            </div>
          )}

          {fields.jobType && (
            <div className="space-y-2">
              <Label htmlFor="jobType">Job Type</Label>
              <div className="relative w-full" data-jobtype-dropdown>
                <Input
                  className="w-full"
                  placeholder="Select job type"
                  value={jobTypeInput}
                  onChange={(e) => {
                    setJobTypeInput(e.target.value)
                    setJobTypePopoverOpen(true)
                  }}
                  onFocus={() => setJobTypePopoverOpen(true)}
                />
                <div
                  className={
                    jobTypePopoverOpen ? "absolute w-full z-50 mt-1 bg-white border rounded-md shadow-lg" : "hidden"
                  }
                >
                  <Command>
                    <CommandEmpty>Job type not found.</CommandEmpty>
                    <CommandGroup>
                      <CommandList className="max-h-[300px] overflow-y-auto">
                        {jobTypes
                          .filter((jobType) =>
                            filterStartsWith
                              ? jobType.name.toLowerCase().startsWith(jobTypeInput.toLowerCase())
                              : jobType.name.toLowerCase().includes(jobTypeInput.toLowerCase()),
                          )
                          .map((jobType) => (
                            <CommandItem
                              key={jobType.id}
                              value={jobType.name}
                              onSelect={() => {
                                setFormData({ ...formData, jobType: jobType.id })
                                setJobTypeInput(jobType.name)
                                setJobTypePopoverOpen(false)
                              }}
                            >
                              {jobType.name}
                              <Check
                                className={cn(
                                  "ml-auto h-4 w-4",
                                  formData.jobType === jobType.id ? "opacity-100" : "opacity-0",
                                )}
                              />
                            </CommandItem>
                          ))}
                      </CommandList>
                    </CommandGroup>
                  </Command>
                </div>
              </div>
            </div>
          )}

          {fields.comments && (
            <div className="space-y-2">
              <Label htmlFor="comments">Comments</Label>
              <Textarea
                id="comments"
                placeholder="Additional comments..."
                rows={3}
                value={formData.comments}
                onChange={(e) => setFormData({ ...formData, comments: e.target.value })}
              />
            </div>
          )}

          {fields.hours && (
            <div className="space-y-2">
              <Label htmlFor="hours">Time spent (minutes)</Label>
              <Input
                id="hours"
                type="number"
                min="1"
                step="1"
                placeholder="90 = 1:30 h"
                value={formData.hours}
                onChange={(e) => setFormData({ ...formData, hours: e.target.value })}
                required
              />
              <p className="text-xs text-muted-foreground">
                Enter total time in minutes (for example, 90 minutes = 1:30 hours)
              </p>
            </div>
          )}
        </>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <Button variant="outline" type="button" onClick={onClose}>
          {t('calendar.cancel')}
        </Button>
        <Button type="submit">{t('calendar.save')}</Button>
      </div>
    </form>
  )
}
