import { useState, useEffect } from 'react'
import { useSearchParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { MapPin, Star, Shield, Award, ChevronRight, Filter, Loader2 } from 'lucide-react'
import { catalogApi } from '@/services/api'
import { ServiceCategory, CATEGORY_META, BOOKING_STATUS_LABEL, ProviderSearchResult } from '@/types'

export default function SearchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [selectedCategory, setSelectedCategory] = useState<ServiceCategory | undefined>(
    searchParams.get('category') ? Number(searchParams.get('category')) : undefined
  )
  const [userLat] = useState(Number(searchParams.get('lat')) || 33.5731)
  const [userLng] = useState(Number(searchParams.get('lng')) || -7.5898)

  const { data, isLoading, error } = useQuery({
    queryKey: ['providers', selectedCategory, userLat, userLng],
    queryFn: () => catalogApi.searchNearby({
      lat: userLat, lng: userLng,
      category: selectedCategory,
      radius: 5,
      limit: 20
    }).then(r => r.data),
    enabled: true
  })

  const providers: ProviderSearchResult[] = data?.results || []

  return (
    <div className="min-h-screen bg-gray-50">

      {/* ─── HEADER ──────────────────────────────── */}
      <div className="bg-white sticky top-0 z-10 shadow-sm">
        <div className="max-w-lg mx-auto px-4 py-3">
          <div className="flex items-center gap-2 mb-3">
            <button onClick={() => navigate(-1)} className="text-gray-500 hover:text-gray-700">
              ←
            </button>
            <h1 className="font-bold text-gray-800">
              {selectedCategory !== undefined
                ? CATEGORY_META[selectedCategory].label
                : 'Tous les services'}
            </h1>
            <span className="ml-auto text-sm text-gray-500">{providers.length} résultats</span>
          </div>

          {/* Filtres catégories - scroll horizontal */}
          <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
            <button
              onClick={() => setSelectedCategory(undefined)}
              className={`flex-shrink-0 px-3 py-1.5 rounded-full text-xs font-medium transition
                ${selectedCategory === undefined
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}
            >
              Tous
            </button>
            {Object.entries(CATEGORY_META).map(([catValue, meta]) => (
              <button
                key={catValue}
                onClick={() => setSelectedCategory(Number(catValue) as ServiceCategory)}
                className={`flex-shrink-0 px-3 py-1.5 rounded-full text-xs font-medium flex items-center gap-1 transition
                  ${selectedCategory === Number(catValue)
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}
              >
                <span>{meta.icon}</span>
                <span>{meta.label}</span>
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* ─── ESTIMATION PRIX ─────────────────────── */}
      {selectedCategory !== undefined && (
        <PriceEstimateBar category={selectedCategory} />
      )}

      {/* ─── RÉSULTATS ───────────────────────────── */}
      <div className="max-w-lg mx-auto px-4 py-4">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center py-16 text-gray-400">
            <Loader2 className="w-8 h-8 animate-spin mb-3" />
            <p className="text-sm">Recherche dans ton quartier...</p>
          </div>
        ) : providers.length === 0 ? (
          <div className="text-center py-16">
            <div className="text-5xl mb-4">🔍</div>
            <p className="text-gray-600 font-medium">Aucun prestataire trouvé</p>
            <p className="text-gray-400 text-sm mt-1">Essaie d'élargir la zone de recherche</p>
          </div>
        ) : (
          <div className="space-y-3">
            {providers.map(provider => (
              <ProviderCard
                key={provider.id}
                provider={provider}
                onSelect={() => navigate(`/booking/new?providerId=${provider.id}&category=${selectedCategory}`)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

// ─── COMPOSANT ESTIMATION PRIX ─────────────────────────────────────────────
function PriceEstimateBar({ category }: { category: ServiceCategory }) {
  const { data } = useQuery({
    queryKey: ['price-estimate', category],
    queryFn: () => catalogApi.estimatePrice(category).then(r => r.data),
    staleTime: 5 * 60 * 1000
  })

  if (!data) return null

  return (
    <div className="max-w-lg mx-auto px-4 py-2">
      <div className="bg-blue-50 border border-blue-100 rounded-xl px-4 py-3 flex items-center gap-3">
        <div className="text-2xl">🤖</div>
        <div>
          <p className="text-xs text-blue-600 font-medium">Estimation IA</p>
          <p className="text-sm font-bold text-blue-800">
            {data.minEstimate} - {data.maxEstimate} DH
          </p>
          <p className="text-xs text-blue-500">{data.note}</p>
        </div>
      </div>
    </div>
  )
}

// ─── COMPOSANT PROVIDER CARD ────────────────────────────────────────────────
function ProviderCard({ provider, onSelect }: {
  provider: ProviderSearchResult
  onSelect: () => void
}) {
  return (
    <div
      onClick={onSelect}
      className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100 hover:border-blue-200 hover:shadow-md transition-all cursor-pointer active:scale-98"
    >
      <div className="flex items-start gap-3">

        {/* Avatar */}
        <div className="relative flex-shrink-0">
          <div className="w-14 h-14 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-xl font-bold">
            {provider.avatarUrl
              ? <img src={provider.avatarUrl} alt="" className="w-full h-full rounded-full object-cover" />
              : provider.fullName.charAt(0).toUpperCase()
            }
          </div>
          {provider.isVerified && (
            <div className="absolute -bottom-1 -right-1 bg-green-500 rounded-full p-0.5">
              <Shield className="w-3 h-3 text-white" />
            </div>
          )}
        </div>

        {/* Info */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <h3 className="font-bold text-gray-800 text-sm">{provider.businessName}</h3>
            {provider.isPremium && (
              <span className="bg-yellow-100 text-yellow-700 text-xs px-1.5 py-0.5 rounded-full flex items-center gap-1">
                <Award className="w-3 h-3" /> Premium
              </span>
            )}
          </div>
          <p className="text-gray-500 text-xs mt-0.5 truncate">{provider.fullName}</p>

          <div className="flex items-center gap-3 mt-2">
            {/* Rating */}
            <div className="flex items-center gap-1">
              <Star className="w-3.5 h-3.5 text-yellow-400 fill-yellow-400" />
              <span className="text-xs font-medium text-gray-700">{provider.rating.toFixed(1)}</span>
              <span className="text-xs text-gray-400">({provider.totalReviews})</span>
            </div>

            {/* Distance */}
            {provider.distanceKm !== undefined && (
              <div className="flex items-center gap-1">
                <MapPin className="w-3.5 h-3.5 text-blue-400" />
                <span className="text-xs text-gray-600">{provider.distanceKm.toFixed(1)} km</span>
              </div>
            )}

            {/* Jobs */}
            <span className="text-xs text-gray-400">{provider.completedJobs} missions</span>
          </div>
        </div>

        {/* Prix + flèche */}
        <div className="flex flex-col items-end gap-1 flex-shrink-0">
          {provider.minPrice && (
            <span className="text-sm font-bold text-gray-800">
              {provider.minPrice} DH
            </span>
          )}
          <ChevronRight className="w-5 h-5 text-gray-300" />
        </div>
      </div>

      {/* Categories */}
      <div className="flex gap-1 mt-3 flex-wrap">
        {provider.categories.slice(0, 3).map(cat => (
          <span key={cat} className="bg-gray-100 text-gray-600 text-xs px-2 py-0.5 rounded-full">
            {CATEGORY_META[cat]?.icon} {CATEGORY_META[cat]?.label}
          </span>
        ))}
      </div>
    </div>
  )
}
