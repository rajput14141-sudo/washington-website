import { Route, Routes } from 'react-router-dom'
import Navbar from './components/Navbar'
import Home from './pages/Home'
import Services from './pages/Services'
import AdminDashboard from './pages/AdminDashboard'
import PublicBooking from './pages/PublicBooking'
import AdminAuth from './pages/AdminAuth'
import { useAuth } from './context/AuthContext'
import { Navigate } from 'react-router-dom'

function RequireAuth({ children, adminOnly = false }: { children: JSX.Element, adminOnly?: boolean }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  if (adminOnly && !user.roles.includes('Admin')) return <Navigate to="/" replace />
  return children
}

export default function App() {
  return (
    <div className="min-h-screen bg-[#f6f9f9]">
      <Navbar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/services" element={<Services />} />
        <Route path="/admin-access" element={<AdminAuth />} />
        <Route path="/book/:serviceId" element={<PublicBooking />} />
        <Route path="/admin" element={<RequireAuth adminOnly><AdminDashboard /></RequireAuth>} />
      </Routes>
    </div>
  )
}
