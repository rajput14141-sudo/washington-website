import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import Navbar from './components/Navbar'
import Footer from './components/Footer'
import Home from './pages/Home'
import Services from './pages/Services'
import AdminDashboard from './pages/AdminDashboard'
import Booking from './pages/Booking'
import BookingSuccess from './pages/BookingSuccess'
import Dashboard from './pages/Dashboard'
import Login from './pages/Login'
import Register from './pages/Register'
import ForgotPassword from './pages/ForgotPassword'
import ResetPassword from './pages/ResetPassword'
import AdminAuth from './pages/AdminAuth'
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
    <div className="flex min-h-screen flex-col bg-[#f6f9f9]">
      <Navbar />
      <div className="flex-1">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/services" element={<Services />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/admin-access" element={<AdminAuth />} />
          <Route path="/policies/:policy" element={<Legal />} />
          <Route path="/book/:serviceId" element={<RequireAuth><Booking /></RequireAuth>} />
          <Route path="/booking-success" element={<RequireAuth><BookingSuccess /></RequireAuth>} />
          <Route path="/dashboard" element={<RequireAuth><Dashboard /></RequireAuth>} />
          <Route path="/admin" element={<RequireAuth adminOnly><AdminDashboard /></RequireAuth>} />
        </Routes>
      </div>
      <Footer />
    </div>
  )
}
