import { CalendarDays, Check, MessageCircle, PartyPopper } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'

interface BookingSuccessState {
  bookingId?: number
  serviceName?: string
  scheduledAt?: string
  vehicleName?: string
  phoneNumber?: string
  address?: string
  city?: string
  pincode?: string
  notes?: string
}

const confetti = [
  ['left-[8%]', 'bg-teal-500', 'delay-100'],
  ['left-[18%]', 'bg-amber-400', 'delay-500'],
  ['left-[30%]', 'bg-rose-400', 'delay-300'],
  ['left-[43%]', 'bg-blue-500', 'delay-700'],
  ['left-[57%]', 'bg-emerald-500', 'delay-200'],
  ['left-[69%]', 'bg-amber-500', 'delay-1000'],
  ['left-[82%]', 'bg-rose-500', 'delay-500'],
  ['left-[92%]', 'bg-teal-400', 'delay-300']
]

export default function BookingSuccess() {
  const location = useLocation()
  const details = (location.state as BookingSuccessState | null) ?? {}
  const bookingDate = details.scheduledAt
    ? new Date(`${details.scheduledAt}T00:00:00`).toLocaleDateString('en-IN', {
        day: 'numeric',
        month: 'long',
        year: 'numeric'
      })
    : null
  const whatsAppNumber = '919220475319'
  const whatsAppMessage = [
    'Hello, I would like to confirm my car wash booking.',
    details.bookingId ? `Booking Number: #${details.bookingId}` : null,
    details.serviceName ? `Service: ${details.serviceName}` : null,
    details.vehicleName ? `Vehicle: ${details.vehicleName}` : null,
    bookingDate ? `Date: ${bookingDate}` : null,
    details.phoneNumber ? `Customer Phone: ${details.phoneNumber}` : null,
    details.address
      ? `Address: ${details.address}, ${details.city ?? ''} ${details.pincode ?? ''}`.trim()
      : null,
    `Notes: ${details.notes || 'N/A'}`
  ].filter(Boolean).join('\n')

  function handleWhatsAppClick() {
    const encodedMessage = encodeURIComponent(whatsAppMessage)
    const isMobileDevice = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent)
    window.location.href = isMobileDevice
      ? `whatsapp://send?phone=${whatsAppNumber}&text=${encodedMessage}`
      : `https://wa.me/${whatsAppNumber}?text=${encodedMessage}`
  }

  return (
    <main className="page-shell flex min-h-[70vh] items-center justify-center">
      <section className="celebration-panel relative w-full max-w-3xl overflow-hidden rounded-[28px] border border-teal-200 bg-white px-6 py-14 text-center shadow-xl shadow-teal-900/10 sm:px-12 sm:py-16">
        <div className="pointer-events-none absolute inset-x-0 top-0 h-44 overflow-hidden" aria-hidden="true">
          {confetti.map(([position, color, delay], index) => (
            <span
              key={`${position}-${color}`}
              className={`confetti-piece absolute top-[-20px] ${position} ${color} ${delay} ${index % 2 === 0 ? 'rotate-12' : '-rotate-12'}`}
            />
          ))}
        </div>

        <div className="celebration-pop relative mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-teal-700 text-white shadow-lg shadow-teal-900/20">
          <Check size={42} strokeWidth={3} aria-hidden="true" />
        </div>
        <div className="relative mt-7 flex items-center justify-center gap-2 text-sm font-black uppercase tracking-[0.2em] text-teal-700">
          <PartyPopper size={18} aria-hidden="true" />
          Booking received
        </div>
        <h1 className="relative mt-4 text-4xl font-black text-slate-950 sm:text-6xl">Thank you for booking!</h1>
        <p className="relative mx-auto mt-5 max-w-xl text-lg leading-8 text-slate-600">
          Your doorstep car wash request has been saved. Our team will confirm the appointment shortly.
        </p>

        {(details.bookingId || details.serviceName || bookingDate) && (
          <div className="relative mx-auto mt-8 grid max-w-xl gap-3 rounded-2xl border border-slate-200 bg-slate-50 p-5 text-left sm:grid-cols-2">
            {details.bookingId && (
              <div>
                <p className="text-xs font-extrabold uppercase text-slate-500">Booking number</p>
                <p className="mt-1 font-extrabold text-slate-950">#{details.bookingId}</p>
              </div>
            )}
            {details.serviceName && (
              <div>
                <p className="text-xs font-extrabold uppercase text-slate-500">Service</p>
                <p className="mt-1 font-extrabold text-slate-950">{details.serviceName}</p>
              </div>
            )}
            {bookingDate && (
              <div className="flex items-center gap-3 sm:col-span-2">
                <CalendarDays className="text-teal-700" size={20} aria-hidden="true" />
                <p className="font-bold text-slate-800">{bookingDate}</p>
              </div>
            )}
          </div>
        )}

        <div className="relative mx-auto mt-9 flex max-w-xl flex-col gap-3">
          <button
            type="button"
            onClick={handleWhatsAppClick}
            className="primary-button w-full gap-2 bg-[#128c7e] hover:bg-[#0e7469]"
          >
            <MessageCircle size={19} aria-hidden="true" />
            Click for Confirmation
          </button>
          <div className="flex flex-col gap-3 sm:flex-row">
            <Link to="/dashboard" className="primary-button flex-1">View My Bookings</Link>
            <Link to="/services" className="secondary-button flex-1">Browse Services</Link>
          </div>
        </div>
      </section>
    </main>
  )
}