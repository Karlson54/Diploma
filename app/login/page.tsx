import { LoginForm } from "@/components/login-form"

export default function LoginPage() {
  return (
    <div className="flex min-h-screen bg-gray-50 items-center justify-center">
      <div className="w-full max-w-md px-4 py-8 sm:px-6 sm:py-12 bg-white shadow-lg rounded-lg">
        <div className="flex justify-center">
          <div className="h-12 w-auto">
            <img
              src="https://hebbkx1anhila5yf.public.blob.vercel-storage.com/%D0%91%D0%B5%D0%B7%20%D0%B7%D0%B0%D0%B3%D0%BE%D0%BB%D0%BE%D0%B2%D0%BA%D0%B0-fUH90pbu2g9blr3Tk2CoJfZWlS4CiP.png"
              alt="Mediacom"
              className="h-full"
            />
          </div>
        </div>
        <h2 className="mt-6 text-center text-3xl font-bold tracking-tight text-gray-900">Вхід до системи</h2>
        <p className="mt-2 text-center text-sm text-gray-600">Система обліку робочого часу</p>

        <div className="mt-8">
          <LoginForm />
        </div>
      </div>
    </div>
  )
}
