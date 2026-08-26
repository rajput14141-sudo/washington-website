import { FormEvent, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import axios from 'axios'
import { api } from '../api/client'

interface BookingResult {
  id: number
  serviceName: string
}

export default function Booking() {
  const { serviceId } = useParams()
  const navigate = useNavigate()
  const [vehicleName, setVehicleName] = useState('')
  const [scheduledAt, setScheduledAt] = useState('')
  const [notes, setNotes] = useState('')
  const [address, setAddress] = useState('')
  const [city, setCity] = useState('')
  const [pincode, setPincode] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    try {
      const { data: vehicle } = await api.post('/vehicles', {
        make: vehicleName.trim(),
        model: '',
        licensePlate: '',
        type: 'Car'
      })
      const { data } = await api.post<BookingResult>('/bookings', {
        vehicleId: vehicle.id,
        serviceId: Number(serviceId),
        scheduledAt,
        notes,
        address,
        city,
        pincode,
        phoneNumber
      })
      navigate('/booking-success', {
        replace: true,
        state: {
          bookingId: data.id,
          serviceName: data.serviceName,
          scheduledAt
        }
      })
    } catch (requestError: unknown) {
      if (axios.isAxiosError(requestError)) {
        const details = requestError.response?.data
        const validationMessages = details?.errors
          ? Object.values(details.errors).flat().join(' ')
          : undefined
        const message = typeof details === 'string'
          ? details
          : validationMessages || details?.detail || details?.title
        setError(message ?? 'Could not create booking. Check your inputs.')
      } else {
        setError('Could not create booking. Check your inputs.')
      }
    }
  }

  return (
    <div className="page-shell">
      <div className="mx-auto max-w-2xl">
      <div className="mb-8">
        <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Doorstep appointment</p>
        <h2 className="section-title">Book your service</h2>
        <p className="mt-3 text-slate-600">Tell us about your vehicle, location, and preferred time.</p>
      </div>

      <form onSubmit={handleSubmit} className="surface-card space-y-5 p-6 sm:p-8">
        <div>
          <label className="form-label">Vehicle Name *</label>
          <input className="form-control" value={vehicleName}
            onChange={e => setVehicleName(e.target.value)} maxLength={100} required />
        </div>
        <div>
          <label className="form-label">Address *</label>
          <textarea className="form-control min-h-24" value={address}
            onChange={e => setAddress(e.target.value)} maxLength={300} required />
        </div>
        <div>
          <label className="form-label">City *</label>
          <input className="form-control" value={city}
            onChange={e => setCity(e.target.value)} maxLength={100} required />
        </div>
        <div>
          <label className="form-label">Pincode *</label>
          <input className="form-control" value={pincode}
            onChange={e => setPincode(e.target.value)} maxLength={20}
            inputMode="numeric" autoComplete="postal-code" required />
        </div>
        <div>
          <label className="form-label">Phone Number *</label>
          <input className="form-control" type="tel" value={phoneNumber}
            onChange={e => setPhoneNumber(e.target.value.replace(/\D/g, '').slice(0, 10))}
            minLength={10} maxLength={10} pattern="[0-9]{10}"
            title="Enter exactly 10 digits" inputMode="numeric" autoComplete="tel" required />
        </div>
        <div>
          <label className="form-label">Date</label>
          <input className="form-control" type="date"
            value={scheduledAt} onChange={e => setScheduledAt(e.target.value)} required />
        </div>
        <div>
          <label className="form-label">Notes (optional)</label>
          <textarea className="form-control min-h-24" value={notes} onChange={e => setNotes(e.target.value)} />
        </div>
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <button className="primary-button w-full">Confirm Booking</button>
      </form>
      </div>
    </div>
  )
}
