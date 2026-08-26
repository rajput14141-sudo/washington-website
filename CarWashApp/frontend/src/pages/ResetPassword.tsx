import { FormEvent, useState } from 'react'
import axios from 'axios'
import { Link, useSearchParams } from 'react-router-dom'
import { api } from '../api/client'

export default function ResetPassword() {
  const [searchParams] = useSearchParams()
  const email = searchParams.get('email') ?? ''
  const token = searchParams.get('token') ?? ''
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const hasValidLink = Boolean(email && token)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setMessage('')
    setError('')

    if (newPassword !== confirmPassword) {
      setError('Password and re-entered password do not match.')
      return
    }

    setSubmitting(true)
    try {
      const response = await api.post('/auth/reset-password', {
        email,
        token,
        newPassword,
        confirmPassword
      })
      setMessage(response.data.message)
    } catch (requestError) {
      if (axios.isAxiosError(requestError)) {
        const details = requestError.response?.data
        const validationMessages = Array.isArray(details)
          ? details.join(' ')
          : details?.errors
            ? Object.values(details.errors).flat().join(' ')
            : undefined
        setError(validationMessages || 'The reset link is invalid or expired. Request a new link.')
      } else {
        setError('Could not reset the password. Please try again.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page-shell flex justify-center">
      <section className="surface-card w-full max-w-md p-7 sm:p-10">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Account recovery</p>
        <h1 className="section-title mb-3">Reset password</h1>

        {!hasValidLink ? (
          <div className="space-y-5">
            <p className="text-slate-600">This password reset link is incomplete. Please request a new link.</p>
            <Link to="/forgot-password" className="primary-button w-full">Request New Link</Link>
          </div>
        ) : message ? (
          <div className="space-y-5">
            <p className="rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold leading-6 text-teal-800">{message}</p>
            <Link to="/login" className="primary-button w-full">Customer Login</Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="mt-8 space-y-5">
            <div>
              <label className="form-label" htmlFor="new-password">New Password *</label>
              <input id="new-password" className="form-control" type="password" placeholder="Minimum 8 characters"
                value={newPassword} onChange={event => setNewPassword(event.target.value)}
                minLength={8} autoComplete="new-password" required />
            </div>
            <div>
              <label className="form-label" htmlFor="confirm-new-password">Re-enter New Password *</label>
              <input id="confirm-new-password" className="form-control" type="password" placeholder="Enter the same password again"
                value={confirmPassword} onChange={event => setConfirmPassword(event.target.value)}
                minLength={8} autoComplete="new-password" required />
            </div>
            {error && <p className="text-sm font-medium text-red-600">{error}</p>}
            <button className="primary-button w-full" disabled={submitting}>
              {submitting ? 'Resetting...' : 'Reset Password'}
            </button>
          </form>
        )}
      </section>
    </div>
  )
}