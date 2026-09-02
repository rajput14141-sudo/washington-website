import { FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../context/AuthContext'

export default function AdminAuth() {
  const { adminLogin, adminRegister } = useAuth()
  const navigate = useNavigate()
  const [mode, setMode] = useState<'login' | 'signup'>('login')
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    setMessage('')

    try {
      if (mode === 'signup') {
        await adminRegister(fullName, email, password)
        setMode('login')
        setPassword('')
        setMessage('Admin account created. Log in to open the dashboard.')
      } else {
        await adminLogin(email, password)
        navigate('/admin')
      }
    } catch (requestError) {
      if (mode === 'signup' && axios.isAxiosError(requestError)) {
        const details = requestError.response?.data
        const messages = Array.isArray(details) ? details.join(' ') : ''
        setError(messages.includes('already taken')
          ? 'This email is already registered. Use a different email for the admin account, or log in if it is already an admin.'
          : messages || 'Could not create the admin account. Use an 8+ character password.')
      } else {
        setError('Invalid admin email or password.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page-shell flex justify-center">
      <section className="surface-card w-full max-w-md p-7 sm:p-10">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Administration</p>
        <h1 className="section-title mb-3">Admin access</h1>
        <p className="mb-7 text-slate-600">Sign in to manage bookings and publish wash services.</p>

        <div className="mb-7 grid grid-cols-2 rounded-lg bg-slate-100 p-1" aria-label="Admin access mode">
          {(['login', 'signup'] as const).map(option => (
            <button
              key={option}
              type="button"
              onClick={() => { setMode(option); setError(''); setMessage('') }}
              className={`rounded-md px-4 py-2.5 text-sm font-bold transition ${
                mode === option ? 'bg-white text-teal-800 shadow-sm' : 'text-slate-600'
              }`}
            >
              {option === 'login' ? 'Log in' : 'Sign up'}
            </button>
          ))}
        </div>

        {message && <p className="mb-5 rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold text-teal-800">{message}</p>}
        <form onSubmit={handleSubmit} className="space-y-5">
          {mode === 'signup' && (
            <input className="form-control" placeholder="Admin name" value={fullName}
              onChange={event => setFullName(event.target.value)} required />
          )}
          <input className="form-control" type="email" placeholder="Admin email" value={email}
            onChange={event => setEmail(event.target.value)} required />
          <input className="form-control" type="password" placeholder="Password" value={password}
            onChange={event => setPassword(event.target.value)} minLength={8} required />
          {error && <p className="text-sm font-medium text-red-600">{error}</p>}
          <button className="primary-button w-full" disabled={submitting}>
            {submitting ? 'Please wait...' : mode === 'login' ? 'Open Admin Dashboard' : 'Create Admin Account'}
          </button>
        </form>
      </section>
    </div>
  )
}