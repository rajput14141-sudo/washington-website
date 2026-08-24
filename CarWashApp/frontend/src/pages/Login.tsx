import { FormEvent, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const loginState = location.state as { registered?: boolean, email?: string, from?: string } | null
  const [email, setEmail] = useState(loginState?.email ?? '')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')

    try {
      await login(email, phoneNumber)
      navigate(loginState?.from ?? '/services', { replace: true })
    } catch {
      setError('Invalid Gmail or phone number.')
    }
  }

  return (
    <div className="page-shell flex justify-center">
      <section className="surface-card w-full max-w-md p-7 sm:p-10">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Customer access</p>
        <h1 className="section-title mb-3">Customer login</h1>
        <p className="mb-8 text-slate-600">Use your registered Gmail and phone number.</p>
        {loginState?.registered && (
          <p className="mb-5 rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold text-teal-800">
            Registration successful. Log in to continue.
          </p>
        )}
        <form onSubmit={handleSubmit} className="space-y-5">
          <input className="form-control" type="email" placeholder="Gmail (User ID)"
            value={email} onChange={event => setEmail(event.target.value)} required />
          <input className="form-control" type="password" placeholder="Phone number (Password)"
            value={phoneNumber} onChange={event => setPhoneNumber(event.target.value)} required />
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