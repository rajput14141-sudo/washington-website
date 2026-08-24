import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import Navbar from './components/Navbar'
import Footer from './components/Footer'
import Home from './pages/Home'
import Services from './pages/Services'
import AdminDashboard from './pages/AdminDashboard'
import Booking from './pages/Booking'
import BookingSuccess from './pages/BookingSuccess'
import AdminAuth from './pages/AdminAuth'
import Login from './pages/Login'
import Register from './pages/Register'
import Dashboard from './pages/Dashboard'
import Legal from './pages/Legal'
import { useAuth } from './context/AuthContext'

function RequireAuth({ children, adminOnly = false }: { children: JSX.Element, adminOnly?: boolean }) {
  const { user } = useAuth()
  const location = useLocation()
  if (!user) return <Navigate to="/login" replace state={{ from: `${location.pathname}${location.search}` }} />
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
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/admin-access" element={<AdminAuth />} />
        <Route path="/book/:serviceId" element={<RequireAuth><Booking /></RequireAuth>} />
        <Route path="/booking-success" element={<RequireAuth><BookingSuccess /></RequireAuth>} />
        <Route path="/dashboard" element={<RequireAuth><Dashboard /></RequireAuth>} />
        <Route path="/admin" element={<RequireAuth adminOnly><AdminDashboard /></RequireAuth>} />
        <Route path="/legal/:policy" element={<Legal />} />
      </Routes>
      <Footer />
    </div>
  )
}
