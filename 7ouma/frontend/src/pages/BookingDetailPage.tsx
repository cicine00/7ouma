import { useEffect, useRef } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, MapPin, Clock, Phone, CheckCircle, XCircle, Loader2 } from 'lucide-react'
import toast from 'react-hot-toast'
import { bookingApi } from '@/services/api'
import { useTracking } from '@/hooks/useTracking'
import type { BookingRequest, BookingQuote } from '@/types'

const STATUS_LABELS: Record<string, { label: string; color: string }> = {
  Pending:    { label: 'En attente de devis',  color: 'text-orange-600 bg-orange-50' },
  Accepted:   { label: 'Prestataire confirmé', color: 'text-blue-600 bg-blue-50' },
  InProgress: { label: 'En cours',             color: 'text-purple-600 bg-purple-50' },
  Completed:  { label: 'Terminé ✓',            color: 'text-green-600 bg-green-50' },
  Cancelled:  { label: 'Annulé',               color: 'text-red-600 bg-red-50' },
}

export default function BookingDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const mapRef = useRef<HTMLDivElement>(null)

  const { data: booking, isLoading } = useQuery<BookingRequest>({
    queryKey: ['booking', id],
    queryFn: () => bookingApi.getById(id!).then(r => r.data),
    enabled: !!id,
    refetchInterval: 30000, // Poll every 30s
  })

  // Live tracking SignalR
  const { providerLocation, arrivalMessage } = useTracking(
    booking?.status === 'InProgress' ? id! : null
  )

  // Toast quand le prestataire arrive
  useEffect(() => {
    if (arrivalMessage) toast(arrivalMessage, { icon: '📍', duration: 5000 })
  }, [arrivalMessage])

  // Accepter un devis
  const acceptQuote = useMutation({
    mutationFn: (quoteId: string) => bookingApi.acceptQuote(id!, quoteId),
    onSuccess: () => {
      toast.success('Prestataire confirmé ! Il va arriver bientôt.')
      queryClient.invalidateQueries({ queryKey: ['booking', id] })
    },
    onError: () => toast.error('Erreur lors de la confirmation'),
  })

  // Terminer la réservation
  const completeBooking = useMutation({
    mutationFn: () => bookingApi.complete(id!),
    onSuccess: () => {
      toast.success('Service terminé ! Merci d\'avoir utilisé 7OUMA 🎉')
      queryClient.invalidateQueries({ queryKey: ['booking', id] })
    },
  })

  if (isLoading) return (
    <div className="flex items-center justify-center min-h-screen">
      <Loader2 size={32} className="animate-spin text-blue-600" />
    </div>
  )

  if (!booking) return (
    <div className="flex flex-col items-center justify-center min-h-screen gap-4">
      <p className="text-gray-500">Réservation introuvable</p>
      <button onClick={() => navigate('/bookings')} className="text-blue-600">Retour</button>
    </div>
  )

  const statusInfo = STATUS_LABELS[booking.status] ?? { label: booking.status, color: 'text-gray-600 bg-gray-50' }
  const acceptedQuote = booking.quotes.find(q => q.isAccepted)

  return (
    <div className="min-h-screen bg-gray-50 pb-24">

      {/* Header */}
      <div className="bg-white px-4 pt-12 pb-4 flex items-center gap-3 border-b">
        <button onClick={() => navigate(-1)}>
          <ArrowLeft size={24} className="text-gray-700" />
        </button>
        <div>
          <h1 className="font-bold text-gray-800">{booking.title}</h1>
          <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusInfo.color}`}>
            {statusInfo.label}
          </span>
        </div>
      </div>

      {/* Live tracking map placeholder */}
      {booking.status === 'InProgress' && (
        <div ref={mapRef} className="h-48 bg-blue-100 flex items-center justify-center relative">
          <div className="text-center">
            <MapPin size={32} className="text-blue-600 mx-auto" />
            <p className="text-blue-700 font-medium mt-2">Suivi en temps réel</p>
            {providerLocation && (
              <p className="text-blue-500 text-sm">
                Position mise à jour : {new Date(providerLocation.timestamp).toLocaleTimeString()}
              </p>
            )}
          </div>
        </div>
      )}

      <div className="px-4 mt-4 space-y-4">

        {/* Info réservation */}
        <div className="bg-white rounded-2xl p-4 shadow-sm">
          <h2 className="font-semibold text-gray-800 mb-3">Détails</h2>
          <div className="space-y-2 text-sm text-gray-600">
            <div className="flex gap-2"><MapPin size={16} className="text-blue-500 shrink-0 mt-0.5" /><span>{booking.clientAddress} - {booking.clientQuarter}</span></div>
            <div className="flex gap-2"><Clock size={16} className="text-blue-500 shrink-0 mt-0.5" /><span>{new Date(booking.createdAt).toLocaleDateString('fr-MA', { weekday: 'long', day: 'numeric', month: 'long' })}</span></div>
          </div>
          {booking.isUrgent && (
            <div className="mt-2 px-3 py-1.5 bg-orange-50 rounded-xl inline-flex items-center gap-2">
              <span className="text-orange-500 text-xs font-semibold">⚡ Mode Urgence</span>
            </div>
          )}
        </div>

        {/* Devis reçus */}
        {booking.status === 'Pending' && booking.quotes.length > 0 && (
          <div className="bg-white rounded-2xl p-4 shadow-sm">
            <h2 className="font-semibold text-gray-800 mb-3">
              {booking.quotes.length} devis reçus
            </h2>
            <div className="space-y-3">
              {booking.quotes.map((quote) => (
                <QuoteCard
                  key={quote.id}
                  quote={quote}
                  onAccept={() => acceptQuote.mutate(quote.id)}
                  isLoading={acceptQuote.isPending}
                />
              ))}
            </div>
          </div>
        )}

        {/* Prestataire accepté */}
        {acceptedQuote && booking.status !== 'Pending' && (
          <div className="bg-white rounded-2xl p-4 shadow-sm border-2 border-blue-100">
            <h2 className="font-semibold text-gray-800 mb-3">Votre prestataire</h2>
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center text-xl font-bold text-blue-600">
                {acceptedQuote.providerName[0]}
              </div>
              <div className="flex-1">
                <p className="font-medium text-gray-800">{acceptedQuote.providerName}</p>
                <p className="text-green-600 font-bold">{acceptedQuote.proposedPrice} DH</p>
              </div>
              <button className="w-10 h-10 bg-green-50 rounded-full flex items-center justify-center">
                <Phone size={18} className="text-green-600" />
              </button>
            </div>
          </div>
        )}

        {/* Actions */}
        {booking.status === 'InProgress' && (
          <button
            onClick={() => completeBooking.mutate()}
            disabled={completeBooking.isPending}
            className="w-full bg-green-500 text-white py-4 rounded-2xl font-semibold flex items-center justify-center gap-2"
          >
            {completeBooking.isPending
              ? <Loader2 size={20} className="animate-spin" />
              : <><CheckCircle size={20} /> Confirmer la fin du service</>
            }
          </button>
        )}
      </div>
    </div>
  )
}

function QuoteCard({ quote, onAccept, isLoading }: {
  quote: BookingQuote
  onAccept: () => void
  isLoading: boolean
}) {
  return (
    <div className="border border-gray-100 rounded-xl p-3">
      <div className="flex justify-between items-start mb-2">
        <div>
          <p className="font-medium text-gray-800 text-sm">{quote.providerName}</p>
          <p className="text-xs text-gray-500 flex items-center gap-1">
            <Clock size={12} />
            Arrivée ~{quote.estimatedArrivalMinutes} min
          </p>
        </div>
        <p className="text-blue-600 font-bold text-lg">{quote.proposedPrice} DH</p>
      </div>
      {quote.note && <p className="text-xs text-gray-500 mb-2 italic">"{quote.note}"</p>}
      <button
        onClick={onAccept}
        disabled={isLoading}
        className="w-full bg-blue-600 text-white py-2 rounded-xl text-sm font-medium flex items-center justify-center gap-2"
      >
        {isLoading ? <Loader2 size={16} className="animate-spin" /> : <><CheckCircle size={16} /> Choisir ce prestataire</>}
      </button>
    </div>
  )
}
