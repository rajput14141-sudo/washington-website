import { Link, Navigate, useParams } from 'react-router-dom'

const policies = {
  terms: {
    title: 'Terms of Service',
    intro: 'These terms explain how bookings and doorstep car-care services are provided.',
    sections: [
      ['Bookings', 'Appointments depend on service availability, location coverage, and the information supplied during booking.'],
      ['Customer responsibilities', 'Please provide safe access to the vehicle, accurate contact details, and a suitable place for the booked service.'],
      ['Service changes', 'Timing may change because of weather, traffic, vehicle condition, or other operational constraints. We will communicate material changes.']
    ]
  },
  privacy: {
    title: 'Privacy Policy',
    intro: 'We use customer information only to operate bookings and support.',
    sections: [
      ['Information collected', 'We may collect your name, email, phone number, address, vehicle details, and booking history.'],
      ['How it is used', 'Information is used to schedule services, send booking details, provide support, and maintain account security.'],
      ['Data protection', 'Passwords are stored as secure hashes. Booking information is shared only as needed to provide the requested service.']
    ]
  },
  refunds: {
    title: 'Refund Policy',
    intro: 'Car-wash appointments can be affected by travel, preparation, and weather, so refund timing depends on service status.',
    sections: [
      ['Before service begins', 'Contact support as early as possible if you need to cancel or reschedule.'],
      ['After work begins', 'Completed or partially completed services are generally not refundable. Report quality concerns promptly so they can be reviewed.'],
      ['Processing time', 'Our team will confirm eligible cancellation requests through the contact details on your booking.']
    ]
  }
} as const

export default function Legal() {
  const { policy } = useParams()
  const content = policies[policy as keyof typeof policies]
  if (!content) return <Navigate to="/" replace />

  return (
    <main className="page-shell max-w-4xl">
      <p className="text-xs font-black uppercase tracking-[0.2em] text-teal-700">Mr.WashingTon policies</p>
      <h1 className="section-title mt-3">{content.title}</h1>
      <p className="mt-5 max-w-2xl text-lg leading-8 text-slate-600">{content.intro}</p>
      <div className="mt-10 divide-y divide-slate-200 rounded-lg border border-slate-200 bg-white px-6 sm:px-8">
        {content.sections.map(([title, text]) => (
          <section key={title} className="py-7">
            <h2 className="text-lg font-extrabold text-slate-950">{title}</h2>
            <p className="mt-3 leading-7 text-slate-600">{text}</p>
          </section>
        ))}
      </div>
      <Link to="/" className="secondary-button mt-8">Back to home</Link>
    </main>
  )
}