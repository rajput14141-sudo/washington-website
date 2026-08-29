import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { CalendarCheck, IndianRupee, MapPin, ShieldCheck, Sparkles } from 'lucide-react'
import { api } from '../api/client'
import mrWashingtonPoster from '../assets/mr-washington-car-wash.jpeg'

const washGallery = [
  {
    src: 'https://images.unsplash.com/photo-1520340356584-f9917d1eea6f?auto=format&fit=crop&w=1200&q=85',
    alt: 'A car receiving a thorough exterior wash',
    label: 'Exterior wash',
  },
  {
    src: 'https://images.unsplash.com/photo-1607860108855-64acf2078ed9?auto=format&fit=crop&w=1200&q=85',
    alt: 'A professional carefully detailing a vehicle',
    label: 'Hand detailing',
  },
  {
    src: 'https://images.unsplash.com/photo-1552930294-6b595f4c2974?auto=format&fit=crop&w=1200&q=85',
    alt: 'A clean polished car after professional care',
    label: 'Showroom finish',
  },
]

export default function Home() {
  const [siteRating, setSiteRating] = useState(4.9)

  useEffect(() => {
    const applySettings = (settings: { rating?: unknown }) => {
      const rating = Number(settings.rating)

      if (Number.isFinite(rating)) setSiteRating(rating)
    }

    const cached = localStorage.getItem('site-settings-cache')
    if (cached) {
      try {
        const parsed = JSON.parse(cached)
        applySettings(parsed.data ?? parsed)
        return
      } catch {
        localStorage.removeItem('site-settings-cache')
      }
    }

    api.get('/site-settings')
      .then(response => {
        applySettings(response.data)
        localStorage.setItem(
          'site-settings-cache',
          JSON.stringify({ data: response.data, timestamp: Date.now() })
        )
      })
      .catch(() => undefined)
  }, [])

  return (
    <>
      <section className="overflow-hidden bg-gradient-to-br from-teal-50 via-white to-amber-50">
        <div className="page-shell grid items-center gap-8 py-8 lg:grid-cols-[1.25fr_.75fr] lg:py-12">
          <div>
            <p className="mb-5 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Doorstep car care</p>
            <h1 className="max-w-4xl text-5xl font-black leading-[1.02] text-teal-900 sm:text-7xl lg:text-8xl">
              A spotless car, without leaving home.
            </h1>
            <p className="mt-7 max-w-2xl text-lg font-medium leading-8 text-slate-700 sm:text-xl">
              Book trusted car wash professionals and choose a convenient time for doorstep service.
            </p>
            <div className="mt-9 flex flex-wrap gap-4">
              <Link to="/services" className="primary-button">Book a Wash</Link>
              <Link to="/services" className="secondary-button">View Pricing</Link>
            </div>
            <div className="mt-6 grid gap-3 sm:grid-cols-3">
              {[
                [CalendarCheck, 'Easy booking', 'Choose a convenient time'],
                [MapPin, 'At your doorstep', 'No driving or waiting'],
                [IndianRupee, 'Clear pricing', 'Know the cost upfront'],
              ].map(([Icon, title, text]) => (
                <div key={String(title)} className="rounded-2xl border border-teal-100 bg-white/80 p-3 shadow-sm backdrop-blur-sm">
                  <Icon className="text-teal-700" size={20} aria-hidden="true" />
                  <p className="mt-2 font-extrabold text-slate-950">{String(title)}</p>
                  <p className="mt-1 text-sm leading-5 text-slate-600">{String(text)}</p>
                </div>
              ))}
            </div>
            <div className="mt-6 flex flex-wrap gap-x-8 gap-y-3 text-sm font-bold text-slate-700">
              <span>✓ Verified service</span>
              <span>✓ Upfront pricing</span>
              <span>✓ WhatsApp confirmation</span>
            </div>
          </div>

          <div className="relative overflow-hidden rounded-[28px] border border-white/15 bg-teal-950 text-white shadow-2xl shadow-teal-950/20">
            <div className="relative aspect-[1054/1494] overflow-hidden bg-white">
              <img
                src={mrWashingtonPoster}
                alt="Mr. Washington doorstep car wash and detailing service"
                className="h-full w-full object-contain"
              />
            </div>
            <div className="p-5">
              <h2 className="text-2xl font-extrabold text-white">What you get</h2>
              <div className="mt-4 space-y-2">
                {['Trained wash professionals', 'Doorstep service at your address', 'Eco-friendly cleaning process', 'Instant booking confirmation'].map(item => (
                  <div key={item} className="rounded-xl border border-white/15 bg-white/10 px-4 py-3 font-semibold text-white">
                    {item}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="page-shell">
        <div className="mb-10 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Simple and reliable</p>
            <h2 className="section-title">Car care made effortless</h2>
          </div>
          <Link to="/services" className="font-bold text-teal-700 hover:text-teal-900">Explore all services →</Link>
        </div>
        <div className="grid gap-5 md:grid-cols-3">
          {[
            ['01', 'Choose your wash', 'Compare transparent packages and select the care your car needs.'],
            ['02', 'Pick your time', 'Add your vehicle and address, then choose a convenient appointment.'],
            ['03', 'Confirm on WhatsApp', 'Send your booking details directly to our team for quick confirmation.']
          ].map(([number, title, text]) => (
            <article key={number} className="surface-card p-7">
              <span className="text-sm font-black text-teal-700">{number}</span>
              <h3 className="mt-6 text-xl font-extrabold text-slate-950">{title}</h3>
              <p className="mt-3 leading-7 text-slate-700">{text}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="border-y border-slate-200 bg-white">
        <div className="page-shell">
          <div className="mb-8 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
            <div>
              <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Care in every detail</p>
              <h2 className="section-title">A cleaner car, inside and out</h2>
            </div>
            <div className="flex items-center gap-2 text-sm font-bold text-slate-700">
              <ShieldCheck className="text-teal-700" size={20} aria-hidden="true" />
              Professional products and equipment
            </div>
          </div>
          <div className="grid gap-4 md:grid-cols-3">
            {washGallery.map(({ src, alt, label }, index) => (
              <figure key={label} className={`group relative min-h-72 overflow-hidden rounded-lg ${index === 0 ? 'md:col-span-1' : ''}`}>
                <img src={src} alt={alt} loading="lazy" className="absolute inset-0 h-full w-full object-cover transition duration-500 group-hover:scale-105" />
                <div className="absolute inset-0 bg-gradient-to-t from-teal-950/90 via-transparent to-transparent" />
                <figcaption className="absolute inset-x-0 bottom-0 flex items-center gap-3 p-5 text-lg font-extrabold text-white">
                  <Sparkles className="text-amber-300" size={22} aria-hidden="true" />
                  {label}
                </figcaption>
              </figure>
            ))}
          </div>
        </div>
      </section>

      <section className="border-y border-slate-200 bg-teal-950 text-white">
        <div className="page-shell grid gap-10 lg:grid-cols-[.8fr_1.2fr] lg:items-center">
          <div>
            <p className="mb-3 text-xs font-black uppercase tracking-[0.2em] text-teal-300">Service coverage</p>
            <h2 className="text-3xl font-extrabold sm:text-4xl">Now serving Delhi NCR</h2>
            <p className="mt-4 max-w-xl leading-7 text-teal-50/80">
              Doorstep car care across key neighborhoods, with service availability confirmed during booking.
            </p>
          </div>
          <div className="grid gap-3 sm:grid-cols-3">
            {[
              ['Noida', 'All major sectors'],
              ['Greater Noida', 'Residential and commercial zones'],
              ['Delhi', 'Selected NCR locations']
            ].map(([city, detail]) => (
              <div key={city} className="rounded-lg border border-white/15 bg-white/10 p-5">
                <p className="text-lg font-extrabold">{city}</p>
                <p className="mt-2 text-sm leading-6 text-teal-50/70">{detail}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="page-shell">
        <div className="flex flex-col justify-between gap-5 sm:flex-row sm:items-end">
          <div>
            <p className="mb-3 text-xs font-black uppercase tracking-[0.2em] text-teal-700">Customer feedback</p>
            <h2 className="section-title">Care people come back to</h2>
          </div>
          <div className="flex flex-wrap gap-x-6 gap-y-4 border-l-2 border-teal-600 pl-5">
            <div>
              <p className="text-2xl font-black text-slate-950">{siteRating.toFixed(1)}/5</p>
              <p className="text-xs font-bold text-slate-500">Customer rating</p>
            </div>
            <div>
              <p className="text-2xl font-black text-slate-950">Greater Noida</p>
              <p className="text-xs font-bold text-slate-500">Local coverage</p>
            </div>
            <div>
              <p className="text-2xl font-black text-slate-950">487</p>
              <p className="text-xs font-bold text-slate-500">Customer reviews</p>
            </div>
          </div>
        </div>
        <div className="mt-9 grid gap-4 md:grid-cols-3">
          {[
            ['Aarav Mehta', 'Noida', 'The team arrived on time and handled the interior carefully. Booking was simple from start to finish.'],
            ['Neha Sharma', 'Greater Noida', 'Clear pricing, polite staff, and a noticeably cleaner car without a trip to the service center.'],
            ['Rohan Verma', 'Delhi', 'The doorstep appointment fit easily into my workday. I would use the service again.']
          ].map(([name, city, review]) => (
            <figure key={name} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
              <div className="text-sm tracking-[0.15em] text-amber-500" aria-label="5 out of 5 stars">★★★★★</div>
              <blockquote className="mt-4 text-sm leading-7 text-slate-700">“{review}”</blockquote>
              <figcaption className="mt-5 border-t border-slate-100 pt-4">
                <p className="font-extrabold text-slate-950">{name}</p>
                <p className="mt-1 text-xs font-semibold text-slate-500">Sample review · {city}</p>
              </figcaption>
            </figure>
          ))}
        </div>
      </section>

    </>
  )
}
