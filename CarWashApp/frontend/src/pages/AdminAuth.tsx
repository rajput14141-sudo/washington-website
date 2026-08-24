import { FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function AdminAuth() {
  const { adminLogin } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setError('')

    try {
      await adminLogin(email, password)
      navigate('/admin')
    } catch {
      setError('Invalid admin email or password.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page-shell flex justify-center">
      <section className="surface-card w-full max-w-md p-7 sm:p-10">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Administration</p>
        <h1 className="section-title mb-3">Admin access</h1>
        <p className="mb-7 text-slate-600">Only administrators listed in the admin signup database can sign in.</p>
        <form onSubmit={handleSubmit} className="space-y-5">
          <input className="form-control" type="email" placeholder="Admin email" value={email}
            onChange={event => setEmail(event.target.value)} required />
          <input className="form-control" type="password" placeholder="Password" value={password}
            onChange={event => setPassword(event.target.value)} minLength={8} required />
          {error && <p className="text-sm font-medium text-red-600">{error}</p>}
          <button className="primary-button w-full" disabled={submitting}>
            {submitting ? 'Please wait...' : 'Open Admin Dashboard'}
          </button>
        </form>
      </section>
    </div>
  )
}