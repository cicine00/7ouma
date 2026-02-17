import { NavLink } from 'react-router-dom'
import { Home, Search, ClipboardList, User } from 'lucide-react'
import { useAuthStore } from '@/stores/authStore'
import clsx from 'clsx'

export default function BottomNav() {
  const { isAuthenticated, user } = useAuthStore()

  const navItems = [
    { to: '/',         icon: Home,          label: 'Accueil' },
    { to: '/search',   icon: Search,        label: 'Rechercher' },
    { to: '/bookings', icon: ClipboardList, label: 'Demandes', protected: true },
    { to: '/profile',  icon: User,          label: isAuthenticated ? 'Profil' : 'Connexion' },
  ]

  return (
    <nav className="fixed bottom-0 left-0 right-0 max-w-lg mx-auto bg-white border-t border-gray-200 flex items-center safe-area-pb">
      {navItems.map(({ to, icon: Icon, label, protected: isProtected }) => {
        // Rediriger vers login si protégé
        const href = isProtected && !isAuthenticated ? '/login' : to

        return (
          <NavLink
            key={to}
            to={href}
            className={({ isActive }) => clsx(
              'flex-1 flex flex-col items-center py-3 gap-0.5 transition-colors',
              isActive ? 'text-blue-600' : 'text-gray-400 hover:text-gray-600'
            )}
          >
            {({ isActive }) => (
              <>
                <div className={clsx(
                  'p-1.5 rounded-xl transition-colors',
                  isActive && 'bg-blue-50'
                )}>
                  <Icon className="w-5 h-5" />
                </div>
                <span className="text-xs font-medium">{label}</span>
                {/* Badge points fidélité */}
                {to === '/profile' && isAuthenticated && user && user.loyaltyPoints > 0 && (
                  <span className="absolute top-1 right-1/4 bg-blue-600 text-white text-xs rounded-full w-4 h-4 flex items-center justify-center font-bold text-[10px]">
                    •
                  </span>
                )}
              </>
            )}
          </NavLink>
        )
      })}
    </nav>
  )
}
