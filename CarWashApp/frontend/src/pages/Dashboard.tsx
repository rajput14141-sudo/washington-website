import { useEffect, useState } from 'react'
import { api } from '../api/client'

interface Booking {
  id: number
  vehicle: { make: string, model: string }
  service: { name: string, price: number }
  scheduledAt: string
  expireDate: string
  status: string
  address: string
  city: string
  pincode: string
}

export default function Dashboard() {
  const [bookings, setBookings] = useState<Booking[]>([])

  useEffect(() => {
    api.get('/bookings/my').then(res => setBookings(res.data))
  }, [])

  return (
    <div className="page-shell max-w-5xl">
      <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Your account</p>
      <h2 className="section-title mb-8">My bookings</h2>
      {bookings.length === 0 && <div className="surface-card p-8 text-slate-500">No bookings yet.</div>}
      <div className="space-y-4">
        {bookings.map(b => (
          <div key={b.id} className="surface-card flex flex-col justify-between gap-5 p-6 sm:flex-row sm:items-center">
            <div>
              <p className="font-semibold">{b.service.name} — {b.vehicle.make} {b.vehicle.model}</p>
              <p className="mt-2 text-sm text-slate-500">{new Date(b.scheduledAt).toLocaleDateString()}</p>
              <p className="mt-2 inline-flex rounded-lg bg-red-50 px-3 py-2 text-sm font-semibold text-red-800">
                Expires: {new Date(b.expireDate).toLocaleDateString()}
              </p>
              <p className="mt-1 text-sm text-slate-500">{b.address}, {b.city} - {b.pincode}</p>
            </div>
            <span className="rounded-full bg-teal-50 px-4 py-2 text-sm font-bold text-teal-800">
              {b.status === 'Pending' ? 'Your booking confirmed' : b.status}
            </span>
          </div>
        ))}
      </div>
    </div>
  )
}
