import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Phone } from 'lucide-react'
import { api } from '../api/client'
import { formatPrice } from '../utils/price'

interface Service {
  id: number
  name: string
  description: string
  price: string
  phoneNumber?: string
}

export default function Services() {
  const [services, setServices] = useState<Service[]>([])

 useEffect(() => {
  api.get('/services')
    .then((res) => {
      console.log('API Response:', res.data);

      const data = Array.isArray(res.data)
        ? res.data
        : res.data?.$values || [];

      setServices(data);
    })
    .catch((err) => {
      console.error('Services API Error:', err);
    });
}, []);

  return (
    <div className="page-shell">
      <div className="mx-auto max-w-6xl">
      <div className="mb-8 max-w-2xl">
        <p className="mb-2 text-xs font-black uppercase tracking-[0.2em] text-teal-700">Wash packages</p>
        <h2 className="section-title">Car wash services we offer</h2>
        <p className="mt-3 text-base leading-7 text-slate-600">Professional care, transparent pricing, and convenient doorstep service.</p>
      </div>
      <div className="grid items-stretch gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {services.map(s => (
          <article key={s.id} className="group flex flex-col rounded-lg border border-slate-200 bg-white p-5 shadow-sm transition duration-300 hover:-translate-y-1 hover:border-teal-200 hover:shadow-xl hover:shadow-slate-900/10 sm:p-6">
            <div>
              <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-teal-50 text-xl transition group-hover:bg-teal-100">
                {s.id === 1 ? '💧' : s.id === 2 ? '🧽' : '✨'}
              </div>
            </div>
            <h3 className="mt-5 text-xl font-extrabold leading-7 text-slate-950">{s.name}</h3>
            <p className="mt-2 flex-1 text-sm leading-6 text-slate-600">{s.description}</p>
            <div className="mt-5 flex items-center justify-between gap-4 border-t border-slate-100 pt-4">
              <p className="min-w-0 truncate text-xl font-black text-teal-800">{formatPrice(s.price)}</p>
              {s.phoneNumber ? (
                <a href={`tel:${s.phoneNumber.replace(/[^+\d]/g, '')}`}
                  className="inline-flex h-10 shrink-0 items-center justify-center gap-2 rounded-full bg-teal-700 px-5 text-sm font-bold text-white shadow-md shadow-teal-900/10 transition hover:bg-teal-800 focus:outline-none focus:ring-4 focus:ring-teal-200">
                  <Phone size={17} aria-hidden="true" />
                  Call now
                </a>
              ) : (
                <Link
                  to={`/book/${s.id}`}
                  className="inline-flex h-10 shrink-0 items-center justify-center rounded-full bg-teal-700 px-5 text-sm font-bold text-white shadow-md shadow-teal-900/10 transition hover:bg-teal-800 focus:outline-none focus:ring-4 focus:ring-teal-200"
                >
                  Book now
                </Link>
              )}
            </div>
          </article>
        ))}
      </div>
      </div>
    </div>
  )
}
