import i18n from "i18next"
import Backend from "i18next-http-backend"
import { initReactI18next } from "react-i18next"

// Initialize i18next with HTTP backend and React binding
i18n
  .use(Backend)
  .use(initReactI18next)
  .init({
    lng: "en", // default language
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
  })

export default i18n 