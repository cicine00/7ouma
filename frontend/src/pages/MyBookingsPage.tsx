import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { bookingApi } from '@/services/api'
import { BookingRequest, BookingStatus, BOOKING_STATUS_LABEL, CATEGORY_META } from '@/types'
import { Clock, MapPin, Star, Shield, Check, ChevronRight, Loader2 } from 'lucide-react'

export default function MyBookingsPage() {
  const navigate = useNavigate()
  const [filter, setFilter] = useState<BookingStatus | 'all'>('all')

  const { data: bookings = [], isLoading } = useQuery<BookingRequest[]>({
    queryKey: ['my-bookings'],
    queryFn: () => bookingApi.getMyBookings().then(r => r.data),
    refetchInterval: 30000 // Refresh toutes les 30s
  })

  const filtered = filter === 'all'
    ? bookings
    : bookings.filter(b => b.status === filter)

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="bg-white sticky top-0 z-10 shadow-sm">
        <div className="max-w-lg mx-auto px-4 py-3">
          <h1 className="font-bold text-gray-800 text-lg mb-3">Mes demandes</h1>

          {/* Filtres statut */}
          <div className="flex gap-2 overflow-x-auto pb-1">
            {([
              ['all', 'Toutes'],
              [BookingStatus.Pending, 'En attente'],
              [BookingStatus.QuotesReceived, 'Devis reçus'],
              [BookingStatus.Accepted, 'Accepté'],
              [BookingStatus.InProgress, 'En cours'],
              [BookingStatus.Completed, 'Terminé'],
            ] as const).map(([status, label]) => (
              <button
                key={status}
                onClick={() => setFilter(status as any)}
                className={`flex-shrink-0 px-3 py-1.5 rounded-full text-xs font-medium transition
                  ${filter === status ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600'}`}
              >
                {label}
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="max-w-lg mx-auto px-4 py-4">
        {isLoading ? (
          <div className="flex justify-center py-16">
            <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
          </div>
        ) : filtered.length === 0 ? (
          <div className="text-center py-16">
            <div className="text-5xl mb-4">📋</div>
            <p className="text-gray-600 font-medium">Aucune demande</p>
            <button
              onClick={() => navigate('/search')}
              className="mt-4 bg-blue-600 text-white px-6 py-2 rounded-xl text-sm font-medium"
            >
              Chercher un service
            </button>
          </div>
        ) : (
          <div className="space-y-3">
            {filtered.map(booking => (
              <BookingCard
                key={booking.id}
                booking={booking}
                onClick={() => navigate(`/bookings/${booking.id}`)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

// ─── BOOKING CARD ───────────────────────────────────────────────────────────
function BookingCard({ booking, onClick }: { booking: BookingRequest; onClick: () => void }) {
  const statusMeta = BOOKING_STATUS_LABEL[booking.status]
  const catMeta = CATEGORY_META[booking.category]
  const quotesCount = booking.quotes.length

  const statusColors: Record<string, string> = {
    yellow: 'bg-yellow-100 text-yellow-700',
    blue: 'bg-blue-100 text-blue-700',
    indigo: 'bg-indigo-100 text-indigo-700',
    orange: 'bg-orange-100 text-orange-700',
    green: 'bg-green-100 text-green-700',
    red: 'bg-red-100 text-red-700',
  }

  return (
    <div
      onClick={onClick}
      className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100 hover:border-blue-200 hover:shadow-md transition-all cursor-pointer"
    >
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-2">
          <span className="text-2xl">{catMeta.icon}</span>
          <div>
            <p className="font-bold text-gray-800 text-sm">{booking.title}</p>
            <p className="text-gray-400 text-xs">{catMeta.label}</p>
          </div>
        </div>
        <span className={`text-xs px-2 py-1 rounded-full font-medium ${statusColors[statusMeta.color] || 'bg-gray-100 text-gray-600'}`}>
          {statusMeta.label}
        </span>
      </div>

      {/* Devis reçus */}
      {quotesCount > 0 && (
        <div className="bg-blue-50 rounded-xl p-3 mb-3">
          <p className="text-xs font-medium text-blue-700 mb-2">
            {quotesCount}/3 devis reçus
          </p>
          <div className="space-y-1.5">
            {booking.quotes.slice(0, 3).map(quote => (
              <div key={quote.id} className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  {quote.providerIsVerified && <Shield className="w-3 h-3 text-green-500" />}
                  <span className="text-xs text-gray-700">{quote.providerName}</span>
                  <div className="flex items-center gap-0.5">
                    <Star className="w-3 h-3 text-yellow-400 fill-yellow-400" />
                    <span className="text-xs text-gray-500">{quote.providerRating}</span>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-xs text-gray-500">{quote.estimatedArrivalMinutes} min</span>
                  <span className="text-sm font-bold text-blue-700">{quote.proposedPrice} DH</span>
                  {quote.isAccepted && <Check className="w-4 h-4 text-green-500" />}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-1 text-gray-400">
            <MapPin className="w-3.5 h-3.5" />
            <span className="text-xs">{booking.clientDistrict || booking.clientAddress.slice(0, 20)}</span>
          </div>
          <div className="flex items-center gap-1 text-gray-400">
            <Clock className="w-3.5 h-3.5" />
            <span className="text-xs">
              {new Date(booking.createdAt).toLocaleDateString('fr-MA')}
            </span>
          </div>
          {booking.isUrgent && (
            <span className="bg-red-100 text-red-600 text-xs px-1.5 py-0.5 rounded-full">⚡ Urgent</span>
          )}
        </div>
        <ChevronRight className="w-4 h-4 text-gray-300" />
      </div>
    </div>
  )
}
