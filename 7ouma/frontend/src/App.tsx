import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'react-hot-toast'
import { useAuthStore } from '@/stores/authStore'

import HomePage from '@/pages/HomePage'
import LoginPage from '@/pages/LoginPage'
import SearchPage from '@/pages/SearchPage'
import MyBookingsPage from '@/pages/MyBookingsPage'
import BottomNav from '@/components/layout/BottomNav'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 min
      retry: 2,
    }
  }
})

// Guard pour routes protégées
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore()
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className="max-w-lg mx-auto relative min-h-screen bg-gray-50">
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/search" element={<SearchPage />} />
            <Route path="/bookings" element={
              <ProtectedRoute><MyBookingsPage /></ProtectedRoute>
            } />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>

          {/* Navigation basse mobile */}
          <BottomNav />
        </div>

        <Toaster
          position="top-center"
          toastOptions={{
            duration: 3000,
            style: { borderRadius: '12px', fontSize: '14px' }
          }}
        />
      </BrowserRouter>
    </QueryClientProvider>
  )
}
