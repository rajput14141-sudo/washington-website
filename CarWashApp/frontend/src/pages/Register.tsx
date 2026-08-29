import { FormEvent, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../context/AuthContext'
import { INDIAN_MOBILE_PATTERN, sanitizeIndianMobile } from '../utils/phone'

export default function Register() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const registration = location.state as { from?: string } | null
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [address, setAddress] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    if (password !== confirmPassword) {
      setError('Password and re-entered password do not match.')
      return
    }

    try {
      await register(fullName, email, phoneNumber, address, password, confirmPassword)
      navigate('/login', {
        replace: true,
        state: { registered: true, phoneNumber, from: registration?.from }
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
        <div>
          <label className="form-label" htmlFor="customer-name">Name *</label>
          <input id="customer-name" className="form-control" placeholder="Enter your full name"
            value={fullName} onChange={e => setFullName(e.target.value)} autoComplete="name" required />
        </div>
        <div>
          <label className="form-label" htmlFor="customer-address">Address *</label>
          <textarea id="customer-address" className="form-control min-h-24 resize-y" placeholder="Enter your complete address"
            value={address} onChange={e => setAddress(e.target.value)} autoComplete="street-address" required />
        </div>
        <div>
          <label className="form-label" htmlFor="customer-mobile">Mobile Number *</label>
          <input id="customer-mobile" className="form-control" type="tel" placeholder="10-digit number starting with 7, 8, or 9"
            value={phoneNumber} onChange={e => setPhoneNumber(sanitizeIndianMobile(e.target.value))}
            inputMode="numeric" pattern={INDIAN_MOBILE_PATTERN} minLength={10} maxLength={10}
            title="Enter a 10-digit mobile number starting with 7, 8, or 9"
            autoComplete="tel" required />
        </div>
        <div>
          <label className="form-label" htmlFor="customer-email">Email Address *</label>
          <input id="customer-email" className="form-control" type="email" placeholder="Enter your email address"
            value={email} onChange={e => setEmail(e.target.value)} autoComplete="email" required />
        </div>
        <div>
          <label className="form-label" htmlFor="customer-password">Create Password *</label>
          <input id="customer-password" className="form-control" type="password" placeholder="Minimum 8 characters"
            value={password} onChange={e => setPassword(e.target.value)} minLength={8}
            autoComplete="new-password" required />
        </div>
        <div>
          <label className="form-label" htmlFor="customer-confirm-password">Re-enter Password *</label>
          <input id="customer-confirm-password" className="form-control" type="password" placeholder="Enter the same password again"
            value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} minLength={8}
            autoComplete="new-password" required />
        </div>
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <button className="primary-button w-full">Register Customer</button>
      </form>
      </div>
    </div>
  )
}
