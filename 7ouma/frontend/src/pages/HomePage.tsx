import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { MapPin, Zap, Shield, Star, ChevronRight, Search } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { catalogApi } from '@/services/api'
import { ServiceCategory, CATEGORY_META } from '@/types'
import { useAuthStore } from '@/stores/authStore'

export default function HomePage() {
  const navigate = useNavigate()
  const { user } = useAuthStore()
  const [userLocation, setUserLocation] = useState<{ lat: number; lng: number } | null>(null)
  const [locationError, setLocationError] = useState(false)

  // Obtenir la géolocalisation
  useEffect(() => {
    navigator.geolocation.getCurrentPosition(
      pos => setUserLocation({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
      () => setLocationError(true)
    )
  }, [])

  // Catégories disponibles
  const { data: categoriesData } = useQuery({
    queryKey: ['categories'],
    queryFn: () => catalogApi.getCategories().then(r => r.data),
    staleTime: Infinity
  })

  const categories = categoriesData || []

  const handleCategorySelect = (category: ServiceCategory) => {
    if (!userLocation) {
      navigate(`/search?category=${category}`)
    } else {
      navigate(`/search?category=${category}&lat=${userLocation.lat}&lng=${userLocation.lng}`)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">

      {/* ─── HERO ─────────────────────────────────────────────── */}
      <div className="bg-gradient-to-br from-blue-600 to-blue-800 text-white">
        <div className="max-w-lg mx-auto px-4 py-8">

          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <div>
              <h1 className="text-2xl font-bold">
                7<span className="text-blue-200">OUMA</span>
              </h1>
              <p className="text-blue-200 text-sm">الحومة • Ton quartier</p>
            </div>
            {user && (
              <div className="flex items-center gap-2 bg-blue-700 rounded-full px-3 py-1">
                <Star className="w-4 h-4 text-yellow-300 fill-yellow-300" />
                <span className="text-sm font-medium">{user.loyaltyPoints} pts</span>
              </div>
            )}
          </div>

          {/* Greeting */}
          <div className="mb-6">
            <h2 className="text-xl font-semibold">
              {user ? `Bonjour ${user.firstName} 👋` : 'Bienvenue 👋'}
            </h2>
            <p className="text-blue-200 mt-1">Quel service cherches-tu aujourd'hui ?</p>
          </div>

          {/* Location indicator */}
          <div className="flex items-center gap-2 bg-blue-700/50 rounded-xl p-3 mb-4">
            <MapPin className="w-5 h-5 text-blue-200 flex-shrink-0" />
            {userLocation ? (
              <div>
                <p className="text-sm font-medium">{user?.district || 'Votre quartier'}</p>
                <p className="text-xs text-blue-300">Prestataires dans un rayon de 5km</p>
              </div>
            ) : (
              <div>
                <p className="text-sm font-medium">
                  {locationError ? 'Localisation non disponible' : 'Localisation en cours...'}
                </p>
                <p className="text-xs text-blue-300">
                  {locationError ? 'Activez la localisation pour de meilleurs résultats' : ''}
                </p>
              </div>
            )}
          </div>

          {/* Search bar */}
          <button
            onClick={() => navigate('/search')}
            className="w-full bg-white text-gray-500 rounded-xl px-4 py-3 flex items-center gap-3 shadow-sm"
          >
            <Search className="w-5 h-5 text-gray-400" />
            <span>Plombier, électricien, mécanicien...</span>
          </button>
        </div>
      </div>

      {/* ─── STATS RAPIDES ───────────────────────────────────────── */}
      <div className="max-w-lg mx-auto px-4 -mt-4">
        <div className="grid grid-cols-3 gap-3">
          {[
            { icon: '⚡', label: 'Devis en', value: '< 5 min' },
            { icon: '📍', label: 'Rayon', value: '5 km max' },
            { icon: '✅', label: 'Prestataires', value: 'Vérifiés' },
          ].map((stat, i) => (
            <div key={i} className="bg-white rounded-xl p-3 text-center shadow-sm border border-gray-100">
              <div className="text-xl mb-1">{stat.icon}</div>
              <div className="text-xs text-gray-500">{stat.label}</div>
              <div className="text-sm font-bold text-gray-800">{stat.value}</div>
            </div>
          ))}
        </div>
      </div>

      {/* ─── CATÉGORIES ──────────────────────────────────────────── */}
      <div className="max-w-lg mx-auto px-4 mt-6">
        <h3 className="text-lg font-bold text-gray-800 mb-3">Services disponibles</h3>
        <div className="grid grid-cols-4 gap-3">
          {Object.entries(CATEGORY_META).slice(0, 12).map(([catValue, meta]) => (
            <button
              key={catValue}
              onClick={() => handleCategorySelect(Number(catValue) as ServiceCategory)}
              className="bg-white rounded-xl p-3 flex flex-col items-center gap-1 shadow-sm border border-gray-100 hover:border-blue-300 hover:shadow-md transition-all active:scale-95"
            >
              <span className="text-2xl">{meta.icon}</span>
              <span className="text-xs text-gray-600 text-center leading-tight">{meta.label}</span>
            </button>
          ))}
        </div>
      </div>

      {/* ─── COMMENT ÇA MARCHE ───────────────────────────────────── */}
      <div className="max-w-lg mx-auto px-4 mt-8">
        <h3 className="text-lg font-bold text-gray-800 mb-4">Comment ça marche ?</h3>
        <div className="space-y-3">
          {[
            { step: '1', icon: '📸', title: 'Décris ton problème', desc: 'Ajoute photos + description' },
            { step: '2', icon: '💬', title: 'Reçois 3 devis', desc: 'En moins de 5 minutes' },
            { step: '3', icon: '✅', title: 'Choisis le meilleur', desc: 'Prix + note + distance' },
            { step: '4', icon: '🛵', title: 'Suivi en direct', desc: '"Ahmed arrive dans 5 min"' },
          ].map(item => (
            <div key={item.step} className="bg-white rounded-xl p-4 flex items-center gap-4 shadow-sm border border-gray-100">
              <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0">
                <span className="text-blue-700 font-bold text-sm">{item.step}</span>
              </div>
              <div className="text-2xl">{item.icon}</div>
              <div>
                <p className="font-semibold text-gray-800 text-sm">{item.title}</p>
                <p className="text-gray-500 text-xs">{item.desc}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* ─── USP BADGES ──────────────────────────────────────────── */}
      <div className="max-w-lg mx-auto px-4 mt-6 mb-8">
        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-2xl p-5 border border-blue-100">
          <div className="grid grid-cols-3 gap-4">
            {[
              { icon: <Zap className="w-6 h-6 text-yellow-500" />, label: 'Urgence', sub: '< 2h' },
              { icon: <Shield className="w-6 h-6 text-green-500" />, label: 'Vérifié', sub: 'KYC' },
              { icon: <Star className="w-6 h-6 text-purple-500" />, label: 'Points', sub: 'Fidélité' },
            ].map((item, i) => (
              <div key={i} className="flex flex-col items-center text-center">
                {item.icon}
                <p className="font-semibold text-gray-800 text-sm mt-1">{item.label}</p>
                <p className="text-gray-500 text-xs">{item.sub}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* ─── CTA si non connecté ─────────────────────────────────── */}
      {!user && (
        <div className="max-w-lg mx-auto px-4 pb-8">
          <div className="bg-blue-600 rounded-2xl p-5 text-white text-center">
            <p className="font-bold text-lg mb-1">Rejoins 7OUMA !</p>
            <p className="text-blue-200 text-sm mb-4">+50 points de bienvenue offerts 🎁</p>
            <div className="flex gap-3">
              <button
                onClick={() => navigate('/register')}
                className="flex-1 bg-white text-blue-600 rounded-xl py-2 font-bold text-sm"
              >
                S'inscrire
              </button>
              <button
                onClick={() => navigate('/login')}
                className="flex-1 border-2 border-white/40 text-white rounded-xl py-2 font-bold text-sm"
              >
                Se connecter
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  )
}
