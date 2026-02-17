import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { UserProfile } from '@/types'

interface AuthState {
  user: UserProfile | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean

  // Actions
  setUser: (user: UserProfile) => void
  setTokens: (access: string, refresh: string) => void
  login: (user: UserProfile, accessToken: string, refreshToken: string) => void
  logout: () => void
  updateLoyaltyPoints: (points: number) => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setUser: (user) => set({ user }),

      setTokens: (accessToken, refreshToken) =>
        set({ accessToken, refreshToken }),

      login: (user, accessToken, refreshToken) =>
        set({ user, accessToken, refreshToken, isAuthenticated: true }),

      logout: () =>
        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false }),

      updateLoyaltyPoints: (points) =>
        set((state) => ({
          user: state.user ? { ...state.user, loyaltyPoints: points } : null
        })),
    }),
    {
      name: '7ouma-auth',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      })
    }
  )
)
