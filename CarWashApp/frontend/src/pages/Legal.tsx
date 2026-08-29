import { useEffect } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'

const policies = {
  terms: {
    title: 'Terms & Conditions',
    intro: 'These Terms & Conditions govern your use of the Mr.WashingTon website and our doorstep car-wash and vehicle-care services. By creating an account or booking a service, you agree to these terms.',
    sections: [
      ['Account and booking details', 'You must provide accurate contact, address, vehicle, and booking information. You are responsible for keeping your account credentials secure and for bookings made through your account.'],
      ['Service availability', 'Appointments are subject to team availability and service coverage in Greater Noida and other listed locations. A submitted booking is not guaranteed until it is confirmed by our team.'],
      ['Prices and payment', 'The price displayed when you book is based on the selected package and information provided. Additional work will only be carried out after the revised scope and price are communicated to you. Any online payment is also subject to the payment provider’s terms.'],
      ['Your responsibilities', 'Please provide safe and lawful access to the vehicle, sufficient working space, and access to water or electricity when the selected service requires it. Remove cash, valuables, and fragile personal items before the appointment.'],
      ['Vehicle condition', 'Please tell us about existing damage, loose parts, modifications, sensitive electronics, or other conditions that may affect safe cleaning. We may decline or stop work where the vehicle or location presents a safety risk.'],
      ['Delays and rescheduling', 'Arrival times are estimates. Weather, traffic, access restrictions, or operational issues may require us to delay or reschedule an appointment. We will try to notify you using the contact details supplied with the booking.'],
      ['Cancellations and refunds', 'Contact us as early as possible to cancel or reschedule. Refund eligibility depends on the booking and service status and is explained in our Cancellation & Refund Policy.'],
      ['Service concerns', 'Inspect the vehicle when the service is completed and report any concern promptly. We will review reasonable quality concerns and may offer an appropriate remedy after assessing the circumstances.'],
      ['Liability', 'To the extent permitted by law, we are not responsible for pre-existing damage, undisclosed defects, unsecured belongings, or losses caused by circumstances outside our reasonable control. Nothing in these terms excludes rights or liabilities that cannot legally be excluded.'],
      ['Website use', 'Do not misuse the website, attempt unauthorized access, interfere with its operation, submit false bookings, or use its content for unlawful purposes. We may restrict access where misuse or fraud is suspected.'],
      ['Changes and contact', 'We may update these terms as our services change. The version shown on this page applies from its publication date. For questions, call +91 92204 75319.']
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

  useEffect(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }, [policy])

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