import axios, { type AxiosError } from 'axios'
import type { AuthResponse } from '../types'

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

export const api = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 10000,
})

// JWT auto-attach
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Auto refresh token on 401
let isRefreshing = false
let failedQueue: Array<{ resolve: (v: string) => void; reject: (e: unknown) => void }> = []
const processQueue = (error: unknown, token: string | null) => {
  failedQueue.forEach(({ resolve, reject }) => (error ? reject(error) : resolve(token!)))
  failedQueue = []
}

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as typeof error.config & { _retry?: boolean }
    if (error.response?.status === 401 && !original?._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => { failedQueue.push({ resolve, reject }) })
          .then((token) => { original!.headers!.Authorization = `Bearer ${token}`; return api(original!) })
      }
      original!._retry = true; isRefreshing = true
      try {
        const refreshToken = localStorage.getItem('refresh_token')
        if (!refreshToken) throw new Error('No refresh token')
        const { data } = await axios.post<AuthResponse>(`${BASE_URL}/api/auth/refresh`, { refreshToken })
        localStorage.setItem('access_token', data.accessToken)
        localStorage.setItem('refresh_token', data.refreshToken)
        processQueue(null, data.accessToken)
        original!.headers!.Authorization = `Bearer ${data.accessToken}`
        return api(original!)
      } catch (err) {
        processQueue(err, null); localStorage.clear(); window.location.href = '/login'
        return Promise.reject(err)
      } finally { isRefreshing = false }
    }
    return Promise.reject(error)
  }
)

export const authApi = {
  registerClient: (data: unknown) => api.post<AuthResponse>('/api/auth/register/client', data),
  registerProvider: (data: unknown) => api.post<AuthResponse>('/api/auth/register/provider', data),
  login: (data: unknown) => api.post<AuthResponse>('/api/auth/login', data),
  logout: (refreshToken: string) => api.post('/api/auth/logout', { refreshToken }),
  getProfile: () => api.get('/api/auth/profile'),
  updateProfile: (data: unknown) => api.put('/api/auth/profile', data),
}

export const catalogApi = {
  getCategories: () => api.get('/api/catalog/categories'),
  search: (params: Record<string, unknown>) => api.get('/api/catalog/search', { params }),
  getOffer: (id: string) => api.get(`/api/catalog/offers/${id}`),
  createOffer: (data: unknown) => api.post('/api/catalog/offers', data),
  getMyOffers: () => api.get('/api/catalog/offers/mine'),
  getPriceReference: (categoryId: number, city?: string) =>
    api.get(`/api/catalog/pricing/${categoryId}`, { params: { city } }),
}

export const bookingApi = {
  create: (data: unknown) => api.post('/api/bookings', data),
  getById: (id: string) => api.get(`/api/bookings/${id}`),
  getMyClientBookings: (status?: string) => api.get('/api/bookings/my/client', { params: { status } }),
  getMyProviderBookings: (status?: string) => api.get('/api/bookings/my/provider', { params: { status } }),
  getNearbyRequests: (lat: number, lng: number) => api.get('/api/bookings/nearby-requests', { params: { lat, lng } }),
  submitQuote: (id: string, data: unknown) => api.post(`/api/bookings/${id}/quotes`, data),
  acceptQuote: (bookingId: string, quoteId: string) => api.post(`/api/bookings/${bookingId}/quotes/${quoteId}/accept`),
  cancel: (id: string, reason: string) => api.post(`/api/bookings/${id}/cancel`, { reason }),
  complete: (id: string) => api.post(`/api/bookings/${id}/complete`),
}
