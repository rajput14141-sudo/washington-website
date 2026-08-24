import { FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { api } from '../api/client'

export default function PublicBooking() {
  const navigate = useNavigate()
  const [customerName, setCustomerName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [address, setAddress] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')
    setSubmitting(true)

    try {
      await api.post('/bookings/register', { name: customerName, email, phone, address })
      navigate('/services', { replace: true })
    } catch (requestError: unknown) {
      if (axios.isAxiosError(requestError)) {
        const details = requestError.response?.data
        const message = typeof details === 'string'
          ? details
          : Array.isArray(details) ? details.join(' ') : details?.detail
        setError(message ?? 'Could not register your booking. Check all fields.')
      } else {
        setError('Could not register your booking. Check all fields.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page-shell">
      <div className="mx-auto max-w-3xl">
        <div className="mb-8">
          <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">No account required</p>
          <h1 className="section-title">Customer details</h1>
          <p className="mt-3 text-slate-600">Enter your contact details to register.</p>
        </div>

        <form onSubmit={handleSubmit} className="surface-card space-y-8 p-6 sm:p-8">
          <fieldset>
            <legend className="mb-5 text-xl font-extrabold text-slate-950">Contact details</legend>
            <div className="grid gap-5 sm:grid-cols-2">
              <label><span className="form-label">Full name *</span><input className="form-control" value={customerName} onChange={event => setCustomerName(event.target.value)} required /></label>
              <label><span className="form-label">Phone number *</span><input className="form-control" type="tel" value={phone} onChange={event => setPhone(event.target.value)} required /></label>
              <label className="sm:col-span-2"><span className="form-label">Email *</span><input className="form-control" type="email" value={email} onChange={event => setEmail(event.target.value)} required /></label>
              <label className="sm:col-span-2"><span className="form-label">Address *</span><textarea className="form-control min-h-24" value={address} onChange={event => setAddress(event.target.value)} maxLength={300} required /></label>
            </div>
          </fieldset>

          {error && <p className="text-sm font-medium text-red-600">{error}</p>}
          <button className="primary-button w-full" disabled={submitting}>{submitting ? 'Registering...' : 'Register'}</button>
        </form>
      </div>
    </div>
  )
}