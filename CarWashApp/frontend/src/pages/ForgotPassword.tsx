import { FormEvent, useState } from 'react'
import axios from 'axios'
import { Link } from 'react-router-dom'
import { api } from '../api/client'

export default function ForgotPassword() {
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setMessage('')
    setError('')

    try {
      const response = await api.post('/auth/forgot-password', { email })
      setMessage(response.data.message)
    } catch (requestError) {
      if (axios.isAxiosError(requestError) && requestError.response?.status === 503) {
        setError('Password reset email is not configured. Please contact support.')
      } else {
        setError('Could not send the reset link. Please try again.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page-shell flex justify-center">
      <section className="surface-card w-full max-w-md p-7 sm:p-10">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Account recovery</p>
        <h1 className="section-title mb-3">Forgot password</h1>
        <p className="mb-8 text-slate-600">Enter your registered email address to receive a password reset link.</p>

        {message ? (
          <div className="space-y-5">
            <p className="rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold leading-6 text-teal-800">{message}</p>
            <Link to="/login" className="secondary-button w-full">Back to Login</Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="form-label" htmlFor="reset-email">Registered Email Address *</label>
              <input id="reset-email" className="form-control" type="email" placeholder="Enter your registered email"
                value={email} onChange={event => setEmail(event.target.value)} autoComplete="email" required />
            </div>
            {error && <p className="text-sm font-medium text-red-600">{error}</p>}
            <button className="primary-button w-full" disabled={submitting}>
              {submitting ? 'Sending...' : 'Send Reset Link'}
            </button>
            <Link to="/login" className="secondary-button w-full">Back to Login</Link>
          </form>
        )}
      </section>
    </div>
  )
}