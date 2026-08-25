import { FormEvent, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../context/AuthContext'

export default function Register() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const registration = location.state as { from?: string } | null
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [address, setAddress] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    try {
      await register(fullName, email, phoneNumber, address)
      navigate('/login', {
        replace: true,
        state: { registered: true, email, from: registration?.from }
      })
    } catch (requestError) {
      if (axios.isAxiosError(requestError)) {
        const details = requestError.response?.data
        const messages = Array.isArray(details) ? details.join(' ') : details?.detail
        setError(messages || 'Could not create account. Check the entered details.')
      } else {
        setError('Could not create account. Check the entered details.')
      }
    }
  }

  return (
    <div className="page-shell flex justify-center">
      <div className="surface-card w-full max-w-md p-7 sm:p-10">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Get started</p>
        <h2 className="section-title mb-3">Customer registration</h2>
        <p className="mb-8 text-slate-600">Register your details to book a doorstep car wash.</p>
      <form onSubmit={handleSubmit} className="space-y-5">
        <input className="form-control" placeholder="Full name"
          value={fullName} onChange={e => setFullName(e.target.value)} required />
        <input className="form-control" type="email" placeholder="Email address"
          value={email} onChange={e => setEmail(e.target.value)} required />
        <input className="form-control" type="tel" placeholder="10-digit phone number"
          value={phoneNumber} onChange={e => setPhoneNumber(e.target.value.replace(/\D/g, '').slice(0, 10))}
          inputMode="numeric" pattern="[0-9]{10}" minLength={10} maxLength={10}
          autoComplete="tel" required />
        <textarea className="form-control min-h-24 resize-y" placeholder="Address"
          value={address} onChange={e => setAddress(e.target.value)} required />
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <button className="primary-button w-full">Register Customer</button>
      </form>
      </div>
    </div>
  )
}
