import { useEffect, useRef, useState } from 'react'
import * as signalR from '@microsoft/signalr'
import type { ProviderLocation } from '../types'

const HUB_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5003'}/hubs/tracking`

export function useTracking(bookingId: string | null) {
  const hubRef = useRef<signalR.HubConnection | null>(null)
  const [providerLocation, setProviderLocation] = useState<ProviderLocation | null>(null)
  const [arrivalMessage, setArrivalMessage] = useState<string | null>(null)
  const [bookingStatus, setBookingStatus] = useState<string | null>(null)
  const [isConnected, setIsConnected] = useState(false)

  useEffect(() => {
    if (!bookingId) return

    const token = localStorage.getItem('access_token')

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => token ?? '',
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 1000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    // Écouter la position du prestataire
    connection.on('ProviderLocationUpdated', (data: ProviderLocation) => {
      setProviderLocation(data)
    })

    // Écouter les annonces d'arrivée
    connection.on('ProviderArriving', (data: { message: string; minutesAway: number }) => {
      setArrivalMessage(data.message)
    })

    // Écouter les changements de statut
    connection.on('BookingStatusChanged', (data: { status: string }) => {
      setBookingStatus(data.status)
    })

    connection.onreconnected(() => {
      setIsConnected(true)
      connection.invoke('JoinBookingRoom', bookingId).catch(console.error)
    })

    connection.onclose(() => setIsConnected(false))

    const start = async () => {
      try {
        await connection.start()
        await connection.invoke('JoinBookingRoom', bookingId)
        setIsConnected(true)
      } catch (err) {
        console.error('SignalR connection failed:', err)
      }
    }

    start()
    hubRef.current = connection

    return () => {
      connection.invoke('LeaveBookingRoom', bookingId).catch(() => {})
      connection.stop()
    }
  }, [bookingId])

  return { providerLocation, arrivalMessage, bookingStatus, isConnected }
}

// Hook pour les prestataires - envoyer sa position
export function useProviderTracking(bookingId: string | null) {
  const hubRef = useRef<signalR.HubConnection | null>(null)
  const watchIdRef = useRef<number | null>(null)

  const startTracking = async () => {
    if (!bookingId || !hubRef.current) return

    watchIdRef.current = navigator.geolocation.watchPosition(
      async (pos) => {
        try {
          await hubRef.current?.invoke(
            'UpdateLocation', bookingId,
            pos.coords.latitude, pos.coords.longitude
          )
        } catch (err) {
          console.error('Failed to send location:', err)
        }
      },
      (err) => console.error('Geolocation error:', err),
      { enableHighAccuracy: true, maximumAge: 5000, timeout: 10000 }
    )
  }

  const stopTracking = () => {
    if (watchIdRef.current !== null) {
      navigator.geolocation.clearWatch(watchIdRef.current)
      watchIdRef.current = null
    }
  }

  const announceArrival = (minutesAway: number) => {
    if (!bookingId) return
    hubRef.current?.invoke('AnnounceArrival', bookingId, minutesAway).catch(console.error)
  }

  return { startTracking, stopTracking, announceArrival }
}
