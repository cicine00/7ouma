import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Eye, EyeOff, Loader2 } from 'lucide-react'
import toast from 'react-hot-toast'
import { authApi } from '@/services/api'
import { useAuthStore } from '@/stores/authStore'

const schema = z.object({
  email: z.string().email('Email invalide'),
  password: z.string().min(6, 'Minimum 6 caractères'),
})

type FormData = z.infer<typeof schema>

export default function LoginPage() {
  const navigate = useNavigate()
  const { login } = useAuthStore()
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema)
  })

  const onSubmit = async (data: FormData) => {
    setIsLoading(true)
    try {
      const response = await authApi.loginClient(data.email, data.password)
      const { user, accessToken, refreshToken } = response.data
      login(user, accessToken, refreshToken)
      toast.success(`Bienvenue ${user.firstName} ! 👋`)
      navigate('/')
    } catch (err: any) {
      // Erreur gérée par intercepteur axios
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">

      {/* Header */}
      <div className="bg-blue-600 px-4 py-8 text-white text-center">
        <h1 className="text-3xl font-bold">7<span className="text-blue-200">OUMA</span></h1>
        <p className="text-blue-200 mt-1 text-sm">الحومة • Ton quartier, tes services</p>
      </div>

      <div className="flex-1 px-4 py-8 max-w-md mx-auto w-full">
        <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
          <h2 className="text-xl font-bold text-gray-800 mb-1">Connexion</h2>
          <p className="text-gray-500 text-sm mb-6">Content de te revoir !</p>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">

            {/* Email */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                {...register('email')}
                type="email"
                placeholder="vous@example.com"
                className={`w-full px-4 py-3 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 transition
                  ${errors.email ? 'border-red-300 bg-red-50' : 'border-gray-200'}`}
              />
              {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
            </div>

            {/* Mot de passe */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Mot de passe</label>
              <div className="relative">
                <input
                  {...register('password')}
                  type={showPassword ? 'text' : 'password'}
                  placeholder="••••••••"
                  className={`w-full px-4 py-3 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 transition pr-12
                    ${errors.password ? 'border-red-300 bg-red-50' : 'border-gray-200'}`}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-3.5 text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                </button>
              </div>
              {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
            </div>

            {/* Submit */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-blue-600 text-white py-3 rounded-xl font-semibold text-sm hover:bg-blue-700 active:scale-98 transition disabled:opacity-50 flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <><Loader2 className="w-4 h-4 animate-spin" /> Connexion...</>
              ) : 'Se connecter'}
            </button>

          </form>

          <div className="mt-4 text-center">
            <p className="text-gray-500 text-sm">
              Pas encore de compte ?{' '}
              <Link to="/register" className="text-blue-600 font-medium hover:underline">
                S'inscrire
              </Link>
            </p>
          </div>
        </div>

        {/* Points fidélité promo */}
        <div className="mt-4 bg-gradient-to-r from-purple-50 to-blue-50 rounded-xl p-4 border border-purple-100 text-center">
          <p className="text-sm text-gray-700">
            🎁 Nouveau ? <span className="font-bold text-blue-700">+50 points de bienvenue</span> offerts à l'inscription
          </p>
        </div>
      </div>
    </div>
  )
}
