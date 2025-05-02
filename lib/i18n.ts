import i18n from "i18next"
import Backend from "i18next-http-backend"
import { initReactI18next } from "react-i18next"
import LanguageDetector from "i18next-browser-languagedetector"

// Initialize i18next with HTTP backend, language detector, and React binding
i18n
  .use(Backend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: "en",
    debug: process.env.NODE_ENV === "development",
    ns: ["translation"],
    defaultNS: "translation",
    interpolation: {
      escapeValue: false, // not needed for React
    },
    backend: {
      loadPath: "/locales/{{lng}}/{{ns}}.json",
    },
    detection: {
      order: ['navigator', 'localStorage', 'querystring', 'cookie'],
      caches: ['localStorage'],
    },
  })

export default i18n 