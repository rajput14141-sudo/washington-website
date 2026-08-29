import { FormEvent, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { INDIAN_MOBILE_PATTERN, sanitizeIndianMobile } from '../utils/phone'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const loginState = location.state as { registered?: boolean, phoneNumber?: string, from?: string } | null
  const [phoneNumber, setPhoneNumber] = useState(loginState?.phoneNumber ?? '')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')

    try {
      await login(phoneNumber, password)
      navigate(loginState?.from ?? '/services', { replace: true })
    } catch {
      setError('Invalid mobile number or password.')
    }
  }

  return (
    <div className="page-shell flex justify-center">
      <section className="surface-card w-full max-w-md p-7 sm:p-10">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Customer access</p>
        <h1 className="section-title mb-3">Customer login</h1>
        <p className="mb-8 text-slate-600">Use your registered mobile number and password.</p>
        {loginState?.registered && (
          <p className="mb-5 rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold text-teal-800">
            Registration successful. Log in to continue.
          </p>
        )}
        <form onSubmit={handleSubmit} className="space-y-5">
          <input className="form-control" type="tel"
            value={phoneNumber} onChange={event => setPhoneNumber(sanitizeIndianMobile(event.target.value))}
            inputMode="numeric" pattern={INDIAN_MOBILE_PATTERN} minLength={10} maxLength={10}
            title="Enter a 10-digit mobile number starting with 7, 8, or 9"
            autoComplete="username" required />
          <input className="form-control" type="password" placeholder="Password"
            value={password} onChange={event => setPassword(event.target.value)}
            autoComplete="current-password" required />
          <div className="text-right">
            <Link to="/forgot-password" className="text-sm font-bold text-teal-700 hover:text-teal-900">
              Forgot password?
            </Link>
          </div>
          {error && <p className="text-sm font-medium text-red-600">{error}</p>}
          <button className="primary-button w-full">Customer Login</button>
          <p className="text-center text-sm text-slate-600">
            New customer?{' '}
            <Link to="/register" state={{ from: loginState?.from }} className="font-bold text-teal-700 hover:text-teal-900">
              Register here
            </Link>
          </p>
        </form>
      </section>
    </div>
  )
}