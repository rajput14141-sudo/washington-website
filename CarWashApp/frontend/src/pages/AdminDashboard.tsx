import { FormEvent, useEffect, useState } from 'react'
import axios from 'axios'
import { Trash2 } from 'lucide-react'
import { useSearchParams } from 'react-router-dom'
import { api } from '../api/client'
import { formatPrice } from '../utils/price'

interface Service {
  id: number
  name: string
  description: string
  price: string
  phoneNumber?: string
}

interface Booking {
  id: number
  customerName: string
  vehicle: { make: string, model: string, licensePlate: string }
  service: { name: string, price: string }
  scheduledAt: string
  status: string
  address: string
  city: string
  pincode: string
  phoneNumber: string
  expireDate: string
}

interface Customer {
  id: string
  fullName: string
  email: string
  phoneNumber: string
  address: string
}

interface AdminSummary {
  totalBookings: number
  pendingBookings: number
  confirmedBookings: number
}

const STATUSES = ['Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled']
const PAGE_SIZE = 8
export default function AdminDashboard() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [bookings, setBookings] = useState<Booking[]>([])
  const [services, setServices] = useState<Service[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [price, setPrice] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [adminName, setAdminName] = useState('')
  const [adminEmail, setAdminEmail] = useState('')
  const [adminPassword, setAdminPassword] = useState('')
  const [adminConfirmPassword, setAdminConfirmPassword] = useState('')
  const [adminMessage, setAdminMessage] = useState('')
  const [adminError, setAdminError] = useState('')
  const [addingAdmin, setAddingAdmin] = useState(false)
  const [adminPanelOpen, setAdminPanelOpen] = useState(false)
  const [customerPanelOpen, setCustomerPanelOpen] = useState(false)
  const [customers, setCustomers] = useState<Customer[]>([])
  const [customersLoading, setCustomersLoading] = useState(false)
  const [customersError, setCustomersError] = useState('')
  const [summary, setSummary] = useState<AdminSummary>({
    totalBookings: 0,
    pendingBookings: 0,
    confirmedBookings: 0
  })
  const [editingService, setEditingService] = useState<Service | null>(null)
  const [editName, setEditName] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [editPrice, setEditPrice] = useState('')
  const [editPhoneNumber, setEditPhoneNumber] = useState('')
  const [editError, setEditError] = useState('')
  const [savingService, setSavingService] = useState(false)
  const [bookingSearch, setBookingSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('All')
  const [dateFilter, setDateFilter] = useState('')
  const [bookingPage, setBookingPage] = useState(1)
  const [deletingBookingId, setDeletingBookingId] = useState<number | null>(null)
  const [bookingActionError, setBookingActionError] = useState('')
  const [siteRating, setSiteRating] = useState(4.8)
  const [ratingMessage, setRatingMessage] = useState('')
  const [ratingError, setRatingError] = useState('')
  const [peopleCount, setPeopleCount] = useState(0)
  const [peopleMessage, setPeopleMessage] = useState('')
  const [peopleError, setPeopleError] = useState('')

  function loadBookings() {
    api.get('/bookings').then(res => setBookings(res.data))
  }

  function loadServices() {
    api.get('/services').then(res => setServices(res.data))
  }

  function loadSummary() {
    api.get('/bookings/summary').then(res => setSummary(res.data))
  }

  function loadSiteSettings() {
    api.get('/site-settings').then(res => {
      setSiteRating(Number(res.data.rating))
      setPeopleCount(Number(res.data.peopleCount))
    })
  }

  useEffect(() => {
    loadBookings()
    loadServices()
    loadSummary()
    loadSiteSettings()
  }, [])

  useEffect(() => {
    if (searchParams.get('view') === 'customers')
      openCustomerDetails()
  }, [searchParams])

  async function updateStatus(id: number, status: string) {
    await api.put(`/bookings/${id}/status`, { status })
    loadBookings()
    loadSummary()
  }

  async function deleteBooking(booking: Booking) {
    if (!window.confirm(`Delete ${booking.customerName}'s booking? This cannot be undone.`)) return

    setDeletingBookingId(booking.id)
    setBookingActionError('')
    try {
      await api.delete(`/bookings/${booking.id}`)
      loadBookings()
      loadSummary()
    } catch {
      setBookingActionError('Could not delete this booking. Please try again.')
    } finally {
      setDeletingBookingId(null)
    }
  }

  async function saveSiteRating(event: FormEvent) {
    event.preventDefault()
    setRatingMessage('')
    setRatingError('')

    try {
      const response = await api.put('/site-settings/rating', { rating: siteRating })
      setSiteRating(Number(response.data.rating))
      setRatingMessage('Homepage rating updated.')
    } catch {
      setRatingError('Could not update the rating. Enter a value from 0 to 5.')
    }
  }

  async function savePeopleCount(event: FormEvent) {
    event.preventDefault()
    setPeopleMessage('')
    setPeopleError('')

    try {
      const response = await api.put('/site-settings/people-count', { peopleCount })
      setPeopleCount(Number(response.data.peopleCount))
      setPeopleMessage('People served updated.')
    } catch {
      setPeopleError('Could not update people served. Enter zero or a positive whole number.')
    }
  }

  async function publishService(event: FormEvent) {
    event.preventDefault()
    setError('')
    setMessage('')

    try {
      await api.post('/services', {
        id: 0,
        name,
        description,
        price,
        phoneNumber: phoneNumber || null
      })
      setName('')
      setDescription('')
      setPrice('')
      setPhoneNumber('')
      setMessage('Service published. It is now visible on the Services page.')
      loadServices()
    } catch {
      setError('Could not publish the service. Check all values and try again.')
    }
  }

  async function deactivateService(id: number) {
    await api.delete(`/services/${id}`)
    loadServices()
  }

  function openServiceEditor(service: Service) {
    setEditingService(service)
    setEditName(service.name)
    setEditDescription(service.description)
    setEditPrice(service.price)
    setEditPhoneNumber(service.phoneNumber ?? '')
    setEditError('')
  }

  async function updateService(event: FormEvent) {
    event.preventDefault()
    if (!editingService) return

    setSavingService(true)
    setEditError('')
    try {
      await api.put(`/services/${editingService.id}`, {
        id: editingService.id,
        name: editName,
        description: editDescription,
        price: editPrice,
        phoneNumber: editPhoneNumber || null
      })
      setEditingService(null)
      loadServices()
      loadBookings()
    } catch {
      setEditError('Could not update this service. Check the entered values and try again.')
    } finally {
      setSavingService(false)
    }
  }

  async function addAdmin(event: FormEvent) {
    event.preventDefault()
    setAdminError('')
    setAdminMessage('')

    if (adminPassword !== adminConfirmPassword) {
      setAdminError('Password and confirm password must match.')
      return
    }

    setAddingAdmin(true)
    try {
      await api.post('/auth/admin', {
        fullName: adminName,
        email: adminEmail,
        password: adminPassword,
        confirmPassword: adminConfirmPassword
      })
      setAdminName('')
      setAdminEmail('')
      setAdminPassword('')
      setAdminConfirmPassword('')
      setAdminMessage('Admin created. They can now log in from Admin Login.')
    } catch (requestError) {
      if (axios.isAxiosError(requestError)) {
        const details = requestError.response?.data
        const messages = Array.isArray(details) ? details.join(' ') : details?.detail
        setAdminError(messages || 'Could not create the admin account.')
      } else {
        setAdminError('Could not create the admin account.')
      }
    } finally {
      setAddingAdmin(false)
    }
  }

  async function openCustomerDetails() {
    setCustomerPanelOpen(true)
    setCustomersLoading(true)
    setCustomersError('')

    try {
      const response = await api.get('/customers')
      setCustomers(response.data)
    } catch (requestError: unknown) {
      if (axios.isAxiosError(requestError) && requestError.response?.status === 401) {
        setCustomersError('Your admin session has expired. Log in again to view customer details.')
      } else if (axios.isAxiosError(requestError) && requestError.response?.status === 403) {
        setCustomersError('Only administrators can view customer details.')
      } else {
        setCustomersError('Could not load customer details.')
      }
    } finally {
      setCustomersLoading(false)
    }
  }

  const normalizedSearch = bookingSearch.trim().toLowerCase()
  const filteredBookings = bookings.filter(booking => {
    const matchesSearch = normalizedSearch === '' || [
      booking.customerName,
      booking.vehicle.make,
      booking.vehicle.model,
      booking.vehicle.licensePlate,
      booking.service.name,
      booking.phoneNumber,
      booking.city,
      booking.pincode
    ].some(value => value.toLowerCase().includes(normalizedSearch))
    const matchesStatus = statusFilter === 'All' || booking.status === statusFilter
    const matchesDate = dateFilter === '' || booking.scheduledAt.slice(0, 10) === dateFilter
    return matchesSearch && matchesStatus && matchesDate
  })
  const totalBookingPages = Math.max(1, Math.ceil(filteredBookings.length / PAGE_SIZE))
  const currentBookingPage = Math.min(bookingPage, totalBookingPages)
  const visibleBookings = filteredBookings.slice(
    (currentBookingPage - 1) * PAGE_SIZE,
    currentBookingPage * PAGE_SIZE
  )
  return (
    <div className="page-shell">
      <div className="mb-8 flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between">
        <div className="min-w-0">
          <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Operations</p>
          <h1 className="section-title">Admin dashboard</h1>
        </div>
        <div className="grid grid-cols-2 gap-3 sm:flex sm:shrink-0 sm:flex-wrap sm:justify-end">
          <button type="button" className="secondary-button min-w-0 px-3 py-2.5 text-sm sm:px-4" onClick={openCustomerDetails}>
            Customer Details
          </button>
          <button type="button" className="primary-button min-w-0 px-3 py-2.5 text-sm sm:px-4" onClick={() => {
            setAdminError('')
            setAdminMessage('')
            setAdminPanelOpen(true)
          }}>
            Add Admin
          </button>
        </div>
      </div>

      <section className="mb-10 grid gap-4 sm:grid-cols-3" aria-label="Booking analytics">
        {[
          ['Total bookings', summary.totalBookings.toString(), 'All appointments'],
          ['Pending', summary.pendingBookings.toString(), 'Awaiting confirmation'],
          ['Confirmed', summary.confirmedBookings.toString(), 'Active and completed']
        ].map(([label, value, detail]) => (
          <article key={label} className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-xs font-extrabold uppercase text-slate-500">{label}</p>
            <p className="mt-3 text-3xl font-black text-slate-950">{value}</p>
            <p className="mt-2 text-sm text-slate-500">{detail}</p>
          </article>
        ))}
      </section>

      <section className="surface-card mb-10 p-6 sm:p-8">
        <h2 className="text-2xl font-extrabold text-slate-950">Homepage metrics</h2>
        <p className="mt-2 text-slate-600">Adjust the rating and people served shown on the main page.</p>
        <form onSubmit={saveSiteRating} className="mt-6">
          <h3 className="text-base font-extrabold text-slate-950">Customer rating</h3>
          <div className="mt-3 flex flex-wrap items-center gap-3">
          <button type="button" aria-label="Decrease rating"
            className="flex h-12 w-12 items-center justify-center rounded-full border border-slate-200 bg-white text-2xl font-bold text-slate-800 hover:bg-slate-50"
            onClick={() => setSiteRating(rating => Math.max(0, Number((rating - 0.1).toFixed(1))))}>
            −
          </button>
          <input className="form-control w-28 text-center text-lg font-bold" type="number" min="0" max="5" step="0.1"
            aria-label="Homepage rating" value={siteRating}
            onChange={event => setSiteRating(Number(event.target.value))} />
          <button type="button" aria-label="Increase rating"
            className="flex h-12 w-12 items-center justify-center rounded-full border border-slate-200 bg-white text-2xl font-bold text-slate-800 hover:bg-slate-50"
            onClick={() => setSiteRating(rating => Math.min(5, Number((rating + 0.1).toFixed(1))))}>
            +
          </button>
          <button className="primary-button ml-0 sm:ml-2">Save rating</button>
          </div>
          {ratingMessage && <p className="mt-4 text-sm font-semibold text-teal-700">{ratingMessage}</p>}
          {ratingError && <p className="mt-4 text-sm font-semibold text-red-600">{ratingError}</p>}
        </form>
        <form onSubmit={savePeopleCount} className="mt-7 border-t border-slate-200 pt-6">
          <h3 className="text-base font-extrabold text-slate-950">People served</h3>
          <div className="mt-3 flex flex-wrap items-center gap-3">
            <button type="button" aria-label="Decrease people served"
              className="flex h-12 w-12 items-center justify-center rounded-full border border-slate-200 bg-white text-2xl font-bold text-slate-800 hover:bg-slate-50"
              onClick={() => setPeopleCount(count => Math.max(0, count - 1))}>
              −
            </button>
            <input className="form-control w-32 text-center text-lg font-bold" type="number" min="0" step="1"
              aria-label="People served" value={peopleCount}
              onChange={event => setPeopleCount(Math.max(0, Math.trunc(Number(event.target.value))))} />
            <button type="button" aria-label="Increase people served"
              className="flex h-12 w-12 items-center justify-center rounded-full border border-slate-200 bg-white text-2xl font-bold text-slate-800 hover:bg-slate-50"
              onClick={() => setPeopleCount(count => count + 1)}>
              +
            </button>
            <button className="primary-button ml-0 sm:ml-2">Save people</button>
          </div>
          {peopleMessage && <p className="mt-4 text-sm font-semibold text-teal-700">{peopleMessage}</p>}
          {peopleError && <p className="mt-4 text-sm font-semibold text-red-600">{peopleError}</p>}
        </form>
      </section>

      <section className="mb-10">
        <form onSubmit={publishService} className="surface-card p-6 sm:p-8">
          <h2 className="text-2xl font-extrabold text-slate-950">Publish a service</h2>
          <p className="mt-2 text-slate-600">New services appear immediately on the public Services page.</p>
          <div className="mt-6 grid gap-5 sm:grid-cols-2">
            <label className="sm:col-span-2">
              <span className="form-label">Service name</span>
              <input className="form-control" value={name} onChange={event => setName(event.target.value)} required />
            </label>
            <label className="sm:col-span-2">
              <span className="form-label">Service details</span>
              <textarea className="form-control min-h-28 resize-y" value={description}
                onChange={event => setDescription(event.target.value)} required />
            </label>
            <label className="sm:col-span-2">
              <span className="form-label">Price or pricing text</span>
              <div className="relative">
                <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-5 text-lg font-bold text-slate-700">₹</span>
                <input className="form-control pl-12" type="text" placeholder="200 or Chargeable" value={price}
                  onChange={event => setPrice(event.target.value)} required />
              </div>
            </label>
            <label className="sm:col-span-2">
              <span className="form-label">Customer call number (optional)</span>
              <input className="form-control" type="tel" maxLength={30} placeholder="+91 98765 43210"
                value={phoneNumber} onChange={event => setPhoneNumber(event.target.value)} />
            </label>
          </div>
          {message && <p className="mt-5 rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold text-teal-800">{message}</p>}
          {error && <p className="mt-5 text-sm font-medium text-red-600">{error}</p>}
          <button className="primary-button mt-6 w-full sm:w-auto">Publish Service</button>
        </form>
      </section>

      <section className="surface-card mb-10 p-6 sm:p-8">
        <h2 className="text-2xl font-extrabold text-slate-950">Available services</h2>
        <div className="mt-5 grid gap-3 md:grid-cols-2 lg:grid-cols-3">
          {services.map(service => (
            <article key={service.id} className="rounded-lg border border-slate-200 p-4">
              <div className="flex items-start justify-between gap-4">
                <div className="min-w-0">
                  <h3 className="font-extrabold text-slate-950">{service.name}</h3>
                  <p className="mt-1 text-sm text-slate-600">{formatPrice(service.price)}</p>
                  {service.phoneNumber && <p className="mt-1 text-sm font-semibold text-teal-700">{service.phoneNumber}</p>}
                </div>
                <div className="flex shrink-0 gap-3 text-sm font-bold">
                  <button type="button" onClick={() => openServiceEditor(service)}
                    className="text-teal-700 hover:text-teal-900">
                    Edit
                  </button>
                  <button type="button" onClick={() => deactivateService(service.id)}
                    className="text-red-600 hover:text-red-800">
                    Remove
                  </button>
                </div>
              </div>
            </article>
          ))}
        </div>
      </section>

      <div className="mb-5 flex flex-col justify-between gap-3 sm:flex-row sm:items-end">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-950">All bookings</h2>
          <p className="mt-1 text-sm text-slate-500">{filteredBookings.length} of {bookings.length} bookings</p>
        </div>
        {(bookingSearch || statusFilter !== 'All' || dateFilter) && (
          <button type="button" className="text-sm font-bold text-teal-700 hover:text-teal-900"
            onClick={() => {
              setBookingSearch('')
              setStatusFilter('All')
              setDateFilter('')
              setBookingPage(1)
            }}>
            Clear filters
          </button>
        )}
      </div>
      <div className="mb-4 grid gap-3 rounded-lg border border-slate-200 bg-white p-4 md:grid-cols-[1fr_220px_190px]">
        <label>
          <span className="form-label">Search bookings</span>
          <input className="form-control" type="search" placeholder="Customer, vehicle, service, city..."
            value={bookingSearch} onChange={event => { setBookingSearch(event.target.value); setBookingPage(1) }} />
        </label>
        <label>
          <span className="form-label">Status</span>
          <select className="form-control" value={statusFilter}
            onChange={event => { setStatusFilter(event.target.value); setBookingPage(1) }}>
            <option value="All">All statuses</option>
            {STATUSES.map(status => <option key={status} value={status}>{status}</option>)}
          </select>
        </label>
        <label>
          <span className="form-label">Service date</span>
          <input className="form-control" type="date" value={dateFilter}
            onChange={event => { setDateFilter(event.target.value); setBookingPage(1) }} />
        </label>
      </div>
      {bookingActionError && <p className="mb-4 text-sm font-semibold text-red-600">{bookingActionError}</p>}
      <div className="surface-card overflow-hidden">
      <div className="overflow-x-auto">
      <table className="w-full min-w-[1260px] overflow-hidden">
        <thead className="bg-teal-50 text-left text-sm text-teal-950">
          <tr>
            <th className="p-3">Customer</th>
            <th className="p-3">Phone No.</th>
            <th className="p-3">Vehicle</th>
            <th className="p-3">Service</th>
            <th className="p-3">Address</th>
            <th className="p-3">Time</th>
            <th className="bg-red-100 p-3 text-red-950">Expire Time</th>
            <th className="p-3">Status</th>
            <th className="p-3 text-center">Actions</th>
          </tr>
        </thead>
        <tbody>
          {visibleBookings.map(b => {
            return (
            <tr key={b.id} className="border-t border-slate-100 text-sm">
              <td className="p-3">{b.customerName}</td>
              <td className="p-3"><a className="font-semibold text-teal-700 hover:text-teal-900" href={`tel:${b.phoneNumber}`}>{b.phoneNumber || '—'}</a></td>
              <td className="p-3">{b.vehicle.make} {b.vehicle.model} ({b.vehicle.licensePlate})</td>
              <td className="p-3">{b.service.name} — {formatPrice(b.service.price)}</td>
              <td className="p-3">{b.address}, {b.city} - {b.pincode}</td>
              <td className="p-3">{new Date(b.scheduledAt).toLocaleDateString()}</td>
              <td className="bg-red-50 p-3 text-red-950">{new Date(b.expireDate).toLocaleDateString()}</td>
              <td className="p-3">
                <select
                  className="rounded-xl border border-slate-200 bg-white p-2 outline-none focus:border-teal-600"
                  value={b.status}
                  onChange={e => updateStatus(b.id, e.target.value)}
                >
                  {STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                </select>
              </td>
              <td className="p-3 text-center">
                <button
                  type="button"
                  className="inline-flex h-10 w-10 items-center justify-center rounded-full text-red-600 transition hover:bg-red-50 focus:outline-none focus:ring-4 focus:ring-red-100 disabled:cursor-not-allowed disabled:opacity-50"
                  onClick={() => deleteBooking(b)}
                  disabled={deletingBookingId === b.id}
                  aria-label={`Delete ${b.customerName}'s booking`}
                  title="Delete booking"
                >
                  <Trash2 size={19} aria-hidden="true" />
                </button>
              </td>
            </tr>
            )
          })}
        </tbody>
      </table>
      </div>
      {visibleBookings.length === 0 && (
        <p className="border-t border-slate-100 px-6 py-10 text-center text-sm font-semibold text-slate-500">
          No bookings match the selected filters.
        </p>
      )}
      <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-slate-500">
          Page {currentBookingPage} of {totalBookingPages}
        </p>
        <div className="flex gap-2">
          <button type="button" className="secondary-button px-4 py-2 text-sm"
            disabled={currentBookingPage === 1}
            onClick={() => setBookingPage(page => Math.max(1, page - 1))}>
            Previous
          </button>
          <button type="button" className="secondary-button px-4 py-2 text-sm"
            disabled={currentBookingPage === totalBookingPages}
            onClick={() => setBookingPage(page => Math.min(totalBookingPages, page + 1))}>
            Next
          </button>
        </div>
      </div>
      </div>

      {editingService && (
        <div className="fixed inset-0 z-[60] flex items-start justify-center overflow-y-auto bg-slate-950/50 px-5 py-8 sm:items-center"
          role="dialog" aria-modal="true" aria-labelledby="edit-service-title">
          <form onSubmit={updateService} className="relative w-full max-w-xl rounded-lg bg-white p-6 shadow-2xl sm:p-8">
            <button type="button" aria-label="Close edit service form" onClick={() => setEditingService(null)}
              className="absolute right-4 top-4 flex h-10 w-10 items-center justify-center rounded-full text-2xl text-slate-500 hover:bg-slate-100 hover:text-slate-900">
              ×
            </button>
            <h2 id="edit-service-title" className="pr-12 text-2xl font-extrabold text-slate-950">Edit service</h2>
            <p className="mt-2 text-slate-600">Changes appear immediately on the public Services page.</p>
            <div className="mt-6 grid gap-5">
              <label>
                <span className="form-label">Service name</span>
                <input className="form-control" value={editName}
                  onChange={event => setEditName(event.target.value)} required autoFocus />
              </label>
              <label>
                <span className="form-label">Service details</span>
                <textarea className="form-control min-h-32 resize-y" value={editDescription}
                  onChange={event => setEditDescription(event.target.value)} required />
              </label>
              <label>
                <span className="form-label">Price or pricing text</span>
                <input className="form-control" value={editPrice} placeholder="200 or Chargeable"
                  onChange={event => setEditPrice(event.target.value)} required />
              </label>
              <label>
                <span className="form-label">Customer call number (optional)</span>
                <input className="form-control" type="tel" maxLength={30} placeholder="+91 98765 43210"
                  value={editPhoneNumber} onChange={event => setEditPhoneNumber(event.target.value)} />
              </label>
            </div>
            {editError && <p className="mt-5 text-sm font-semibold text-red-600">{editError}</p>}
            <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
              <button type="button" className="secondary-button" onClick={() => setEditingService(null)}>Cancel</button>
              <button className="primary-button" disabled={savingService}>
                {savingService ? 'Saving...' : 'Save changes'}
              </button>
            </div>
          </form>
        </div>
      )}

      {adminPanelOpen && (
        <div className="fixed inset-0 z-[60] flex items-start justify-center overflow-y-auto bg-slate-950/50 px-5 py-8 sm:items-center"
          role="dialog" aria-modal="true" aria-labelledby="add-admin-title">
          <form onSubmit={addAdmin} className="relative w-full max-w-lg rounded-lg bg-white p-6 shadow-2xl sm:p-8">
            <button type="button" aria-label="Close add admin form" onClick={() => setAdminPanelOpen(false)}
              className="absolute right-4 top-4 flex h-10 w-10 items-center justify-center rounded-full text-2xl text-slate-500 hover:bg-slate-100 hover:text-slate-900">
              ×
            </button>
            <h2 id="add-admin-title" className="pr-12 text-2xl font-extrabold text-slate-950">Add admin</h2>
            <p className="mt-2 text-slate-600">Create another administrator with dashboard access.</p>
            <div className="mt-6 grid gap-5">
              <label>
                <span className="form-label">Admin name</span>
                <input className="form-control" value={adminName}
                  onChange={event => setAdminName(event.target.value)} autoFocus required />
              </label>
              <label>
                <span className="form-label">Admin email</span>
                <input className="form-control" type="email" value={adminEmail}
                  onChange={event => setAdminEmail(event.target.value)} required />
              </label>
              <div className="grid gap-5 sm:grid-cols-2">
                <label>
                  <span className="form-label">Password</span>
                  <input className="form-control" type="password" minLength={8} value={adminPassword}
                    onChange={event => setAdminPassword(event.target.value)} required />
                </label>
                <label>
                  <span className="form-label">Confirm password</span>
                  <input className="form-control" type="password" minLength={8} value={adminConfirmPassword}
                    onChange={event => setAdminConfirmPassword(event.target.value)} required />
                </label>
              </div>
            </div>
            {adminMessage && <p className="mt-5 rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold text-teal-800">{adminMessage}</p>}
            {adminError && <p className="mt-5 text-sm font-medium text-red-600">{adminError}</p>}
            <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
              <button type="button" className="secondary-button" onClick={() => setAdminPanelOpen(false)}>Cancel</button>
              <button className="primary-button" disabled={addingAdmin}>
                {addingAdmin ? 'Creating admin...' : 'Add Admin'}
              </button>
            </div>
          </form>
        </div>
      )}

      {customerPanelOpen && (
        <div className="fixed inset-0 z-[60] flex items-start justify-center overflow-y-auto bg-slate-950/50 px-5 py-8 sm:items-center"
          role="dialog" aria-modal="true" aria-labelledby="customer-details-title">
          <section className="relative w-full max-w-5xl overflow-hidden rounded-lg bg-white shadow-2xl">
            <div className="flex items-start justify-between gap-5 border-b border-slate-200 p-6 sm:p-8">
              <div>
                <h2 id="customer-details-title" className="text-2xl font-extrabold text-slate-950">Customer details</h2>
                <p className="mt-2 text-slate-600">Customers registered with an account.</p>
              </div>
              <button type="button" aria-label="Close customer details" onClick={() => {
                setCustomerPanelOpen(false)
                setSearchParams({})
              }}
                className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full text-2xl text-slate-500 hover:bg-slate-100 hover:text-slate-900">
                ×
              </button>
            </div>
            <div className="max-h-[65vh] overflow-auto">
              {customersLoading && <p className="p-8 text-center font-semibold text-slate-600">Loading customers...</p>}
              {customersError && <p className="m-6 rounded-lg bg-red-50 p-4 font-semibold text-red-700">{customersError}</p>}
              {!customersLoading && !customersError && customers.length === 0 && (
                <p className="p-8 text-center font-semibold text-slate-600">No customers have signed up yet.</p>
              )}
              {!customersLoading && !customersError && customers.length > 0 && (
                <table className="w-full min-w-[760px] text-left text-sm">
                  <thead className="sticky top-0 bg-teal-50 text-teal-950">
                    <tr>
                      <th className="p-4">ID</th>
                      <th className="p-4">Name</th>
                      <th className="p-4">Email</th>
                      <th className="p-4">Phone number</th>
                      <th className="p-4">Address</th>
                    </tr>
                  </thead>
                  <tbody>
                    {customers.map(customer => (
                      <tr key={customer.id} className="border-t border-slate-100 align-top">
                        <td className="p-4 text-slate-500">{customer.id}</td>
                        <td className="p-4 font-bold text-slate-950">{customer.fullName}</td>
                        <td className="p-4 text-slate-700">{customer.email}</td>
                        <td className="p-4 text-slate-700">{customer.phoneNumber || 'Not provided'}</td>
                        <td className="max-w-sm whitespace-normal p-4 text-slate-700">{customer.address || 'Not provided'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </section>
        </div>
      )}
    </div>
  )
}
