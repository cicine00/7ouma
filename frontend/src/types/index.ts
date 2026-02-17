// ─── Auth ─────────────────────────────────────────────────────
export type UserRole = 'Client' | 'Provider' | 'Admin'

export interface User {
  id: string
  firstName: string
  lastName: string
  email: string
  phone: string
  role: UserRole
  isVerified: boolean
  avatarUrl?: string
  city?: string
  quarter?: string
  loyaltyPoints: number
  rating: number
  hasPremiumSubscription: boolean
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

// ─── Catalog ──────────────────────────────────────────────────
export interface Category {
  id: number
  name: string
  nameAr: string
  icon: string
  slug: string
}

export interface ServiceOffer {
  id: string
  providerId: string
  providerName: string
  providerAvatar: string
  providerRating: number
  providerReviews: number
  providerIsVerified: boolean
  categoryName: string
  categoryIcon: string
  title: string
  description: string
  basePrice: number
  maxPrice?: number
  city: string
  quarter: string
  latitude: number
  longitude: number
  distanceKm: number
  isUrgencyAvailable: boolean
  images: string[]
  createdAt: string
}

export interface PriceReference {
  categoryId: number
  serviceType: string
  minPrice: number
  maxPrice: number
  averagePrice: number
  city: string
}

// ─── Booking ──────────────────────────────────────────────────
export type BookingStatus =
  | 'Pending' | 'Accepted' | 'InProgress'
  | 'Completed' | 'Cancelled' | 'Disputed'

export type PaymentMethod = 'Online' | 'Cash'

export interface BookingRequest {
  id: string
  clientId: string
  categoryId: number
  title: string
  description: string
  clientLatitude: number
  clientLongitude: number
  clientAddress: string
  clientQuarter: string
  isUrgent: boolean
  preferredPayment: PaymentMethod
  status: BookingStatus
  scheduledAt?: string
  createdAt: string
  completedAt?: string
  photos: string[]
  quotes: BookingQuote[]
}

export interface BookingQuote {
  id: string
  bookingRequestId: string
  providerId: string
  providerName: string
  proposedPrice: number
  note?: string
  estimatedArrivalMinutes: number
  isAccepted: boolean
  createdAt: string
}

// ─── Tracking ─────────────────────────────────────────────────
export interface ProviderLocation {
  providerId: string
  latitude: number
  longitude: number
  timestamp: string
}

// ─── API Generic ──────────────────────────────────────────────
export interface ApiError {
  status: number
  message: string
  timestamp: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}
