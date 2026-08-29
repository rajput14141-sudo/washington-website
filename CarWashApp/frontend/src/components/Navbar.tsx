import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import mrWashingtonLogo from '../assets/mr-washington-logo.jpeg'

export default function Navbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [profileOpen, setProfileOpen] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const profileRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function closeProfile(event: MouseEvent) {
      if (profileRef.current && !profileRef.current.contains(event.target as Node))
        setProfileOpen(false)
    }

    document.addEventListener('mousedown', closeProfile)
    return () => document.removeEventListener('mousedown', closeProfile)
  }, [])

  return (
    <header className="sticky top-0 z-50 border-b border-slate-200/80 bg-white/95 backdrop-blur">
      <nav className="mx-auto flex min-h-20 max-w-7xl items-center justify-between gap-2 px-5 sm:gap-4 sm:px-8 lg:px-12">
        <Link to="/" onClick={() => setMobileOpen(false)} className="flex min-w-0 items-center gap-2 sm:gap-3">
          <img
            src={mrWashingtonLogo}
            alt="Mr. Washington Premium Car Care"
            className="h-11 w-11 shrink-0 rounded-full object-cover shadow-sm sm:h-14 sm:w-14"
          />
          <span className="flex min-w-0 flex-col">
            <span className="text-base font-black tracking-tight text-teal-900 sm:text-2xl">Mr.WashingTon</span>
            <span className="mt-0.5 text-[9px] font-extrabold uppercase tracking-[0.16em] text-blue-500 sm:text-[10px]">
              Care beyond the wash
            </span>
          </span>
        </Link>
        <div className="flex shrink-0 items-center gap-1 sm:hidden">
          <Link
            to="/"
            onClick={() => setMobileOpen(false)}
            className="px-2 py-3 text-sm font-bold text-slate-700 transition hover:text-teal-700"
          >
            Home
          </Link>
          <button
            type="button"
            aria-label={mobileOpen ? 'Close navigation menu' : 'Open navigation menu'}
            aria-expanded={mobileOpen}
            onClick={() => setMobileOpen(open => !open)}
            className="flex h-11 w-11 shrink-0 flex-col items-center justify-center gap-1.5 rounded-xl border border-slate-200 bg-white text-teal-900"
          >
            <span className="h-0.5 w-5 bg-current" />
            <span className="h-0.5 w-5 bg-current" />
            <span className="h-0.5 w-5 bg-current" />
          </button>
        </div>
        <div className="hidden items-center gap-6 text-sm font-bold text-slate-700 sm:flex">
          <Link className="transition hover:text-teal-700" to="/services">Services</Link>
          {user && !user.roles.includes('Admin') && <Link className="transition hover:text-teal-700" to="/dashboard">My Bookings</Link>}
          <a className="transition hover:text-teal-700" href="/#contact">Contact Us</a>
          {!user && <Link className="transition hover:text-teal-700" to="/admin-access">Admin</Link>}
          {user?.roles.includes('Admin') && <Link className="hidden transition hover:text-teal-700 md:block" to="/admin">Admin</Link>}
        {user ? (
          <div ref={profileRef} className="relative">
            <button
              type="button"
              onClick={() => setProfileOpen(open => !open)}
              aria-expanded={profileOpen}
              className="primary-button max-w-44 gap-2 px-5 py-2.5 text-sm"
            >
              <span className="truncate">{user.fullName}</span>
              <span aria-hidden="true" className="text-xs">▾</span>
            </button>
            {profileOpen && (
              <div className="absolute right-0 top-full mt-3 w-72 overflow-hidden rounded-3xl border border-slate-200 bg-white p-3 text-left shadow-2xl">
                <div className="rounded-2xl bg-teal-50 p-4">
                  <p className="font-extrabold text-slate-950">{user.fullName}</p>
                  <p className="mt-1 break-all text-xs font-medium text-slate-600">{user.email}</p>
                </div>
                <div className="mt-2 grid text-sm">
                  {!user.roles.includes('Admin') && (
                    <Link onClick={() => setProfileOpen(false)} to="/dashboard"
                      className="rounded-xl px-4 py-3 hover:bg-slate-50">
                      My Bookings
                    </Link>
                  )}
                  {user.roles.includes('Admin') && (
                    <Link onClick={() => setProfileOpen(false)} to="/admin"
                      className="rounded-xl px-4 py-3 hover:bg-slate-50">
                      Admin Dashboard
                    </Link>
                  )}
                  <button
                    type="button"
                    onClick={() => { logout(); setProfileOpen(false); navigate('/') }}
                    className="rounded-xl px-4 py-3 text-left text-red-600 hover:bg-red-50"
                  >
                    Logout
                  </button>
                </div>
              </div>
            )}
          </div>
        ) : (
          <Link to="/services" className="primary-button px-5 py-2.5 text-sm">Book a Wash</Link>
        )}
        </div>
      </nav>
      {mobileOpen && (
        <div className="border-t border-slate-100 bg-white px-5 py-4 sm:hidden">
          {user && (
            <div className="mb-3 rounded-lg bg-teal-50 p-3">
              <p className="font-extrabold text-slate-950">{user.fullName}</p>
              <p className="mt-1 break-all text-xs font-medium text-slate-600">{user.email}</p>
            </div>
          )}
          <div className="grid gap-1 text-sm font-bold text-slate-700">
            <Link onClick={() => setMobileOpen(false)} to="/services" className="rounded-lg px-3 py-3 hover:bg-slate-50">Services</Link>
            {user && !user.roles.includes('Admin') && (
              <Link onClick={() => setMobileOpen(false)} to="/dashboard" className="rounded-lg px-3 py-3 hover:bg-slate-50">My Bookings</Link>
            )}
            <a onClick={() => setMobileOpen(false)} href="/#contact" className="rounded-lg px-3 py-3 hover:bg-slate-50">Contact Us</a>
            {user ? (
              <>
                {user.roles.includes('Admin') && (
                  <Link onClick={() => setMobileOpen(false)} to="/admin" className="rounded-lg px-3 py-3 hover:bg-slate-50">Admin Dashboard</Link>
                )}
                <button
                  type="button"
                  onClick={() => { logout(); setMobileOpen(false); navigate('/') }}
                  className="rounded-lg px-3 py-3 text-left text-red-600 hover:bg-red-50"
                >
                  Logout
                </button>
              </>
            ) : (
              <>
                <Link onClick={() => setMobileOpen(false)} to="/admin-access" className="rounded-lg px-3 py-3 hover:bg-slate-50">Admin</Link>
                <Link onClick={() => setMobileOpen(false)} to="/services" className="primary-button mt-2 w-full">Book a Wash</Link>
              </>
            )}
          </div>
        </div>
      )}
    </header>
  )
}
