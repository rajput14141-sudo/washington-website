import { FormEvent, useEffect, useState } from 'react'
import { Trash2 } from 'lucide-react'
import { api } from '../api/client'

interface Service {
  id: number
  name: string
  description: string
  price: number
}

interface Booking {
  id: number
  customerName: string
  vehicle: { make: string, model: string, licensePlate: string }
  service: { name: string, price: number }
  scheduledAt: string
  status: string
  address: string
  city: string
  pincode: string
  expireDate: string
}

interface Customer {
  id: string
  fullName: string
  email: string
  phoneNumber?: string
  address: string
}

interface ServiceLocation {
  id: number
  name: string
}

const STATUSES = ['Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled']

export default function AdminDashboard() {
  const [bookings, setBookings] = useState<Booking[]>([])
  const [services, setServices] = useState<Service[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [price, setPrice] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [customers, setCustomers] = useState<Customer[]>([])
  const [editingService, setEditingService] = useState<Service | null>(null)
  const [deletingBookingId, setDeletingBookingId] = useState<number | null>(null)
  const [deletingCustomerId, setDeletingCustomerId] = useState<string | null>(null)
  const [locations, setLocations] = useState<ServiceLocation[]>([])
  const [locationName, setLocationName] = useState('')
  const [editingLocation, setEditingLocation] = useState<ServiceLocation | null>(null)
  const [savingLocation, setSavingLocation] = useState(false)

  async function loadBookings() {
    const response = await api.get('/bookings')
    setBookings(response.data)
  }

  async function loadServices() {
    const response = await api.get('/services')
    setServices(response.data)
  }

  async function loadCustomers() {
    const response = await api.get('/customers')
    setCustomers(response.data)
  }

  async function loadLocations() {
    const response = await api.get<ServiceLocation[]>('/locations')
    setLocations(response.data)
  }

  useEffect(() => {
    Promise.all([loadBookings(), loadServices(), loadCustomers(), loadLocations()])
      .catch(() => setError('Your admin session has expired. Please log in again.'))
  }, [])

  async function updateStatus(id: number, status: string) {
    try {
      await api.put(`/bookings/${id}/status`, { status })
      await loadBookings()
    } catch {
      setError('Could not update the booking status. Please log in again if your session expired.')
    }
  }

  async function deleteBooking(booking: Booking) {
    if (!window.confirm(`Delete ${booking.customerName}'s booking? This cannot be undone.`))
      return

    setDeletingBookingId(booking.id)
    setError('')
    try {
      await api.delete(`/bookings/${booking.id}`)
      setBookings(current => current.filter(item => item.id !== booking.id))
    } catch {
      setError('Could not delete the booking. Please log in again if your session expired.')
    } finally {
      setDeletingBookingId(null)
    }
  }

  async function deleteCustomer(customer: Customer) {
    if (!window.confirm(`Delete ${customer.fullName}'s account? This cannot be undone.`))
      return

    setDeletingCustomerId(customer.id)
    setError('')
    try {
      await api.delete(`/customers/${customer.id}`)
      await Promise.all([loadCustomers(), loadBookings()])
    } catch {
      setError('Could not delete the customer. Remove their bookings first, then try again.')
    } finally {
      setDeletingCustomerId(null)
    }
  }

  async function publishService(event: FormEvent) {
    event.preventDefault()
    setError('')
    setMessage('')

    try {
      const serviceData = {
        id: editingService?.id ?? 0,
        name,
        description,
        price: Number(price)
      }

      if (editingService)
        await api.put(`/services/${editingService.id}`, serviceData)
      else
        await api.post('/services', serviceData)

      setName('')
      setDescription('')
      setPrice('')
      setMessage(editingService
        ? 'Service updated successfully.'
        : 'Service published. It is now visible on the Services page.')
      setEditingService(null)
      await loadServices()
    } catch {
      setError(`Could not ${editingService ? 'update' : 'publish'} the service. Check all values and try again.`)
    }
  }

  function startEditing(service: Service) {
    setEditingService(service)
    setName(service.name)
    setDescription(service.description)
    setPrice(String(service.price))
    setMessage('')
    setError('')
  }

  function cancelEditing() {
    setEditingService(null)
    setName('')
    setDescription('')
    setPrice('')
  }

  async function deactivateService(id: number) {
    try {
      await api.delete(`/services/${id}`)
      await loadServices()
    } catch {
      setError('Could not remove the service. Please log in again if your session expired.')
    }
  }

  async function saveLocation(event: FormEvent) {
    event.preventDefault()
    const trimmedName = locationName.trim()
    if (!trimmedName) return

    setSavingLocation(true)
    setError('')
    try {
      if (editingLocation)
        await api.put(`/locations/${editingLocation.id}`, { name: trimmedName })
      else
        await api.post('/locations', { name: trimmedName })

      setLocationName('')
      setEditingLocation(null)
      await loadLocations()
    } catch {
      setError('Could not save the location. Make sure the location name is not already in the list.')
    } finally {
      setSavingLocation(false)
    }
  }

  function startEditingLocation(location: ServiceLocation) {
    setEditingLocation(location)
    setLocationName(location.name)
    setError('')
  }

  function cancelEditingLocation() {
    setEditingLocation(null)
    setLocationName('')
  }

  async function deleteLocation(location: ServiceLocation) {
    if (!window.confirm(`Delete ${location.name} from service locations?`)) return

    setError('')
    try {
      await api.delete(`/locations/${location.id}`)
      if (editingLocation?.id === location.id) cancelEditingLocation()
      await loadLocations()
    } catch {
      setError('Could not delete the location. Please log in again if your session expired.')
    }
  }

  return (
    <div className="page-shell">
      <p className="mb-3 text-sm font-black uppercase tracking-[0.2em] text-teal-700">Operations</p>
      <h1 className="section-title mb-8">Admin dashboard</h1>

      <section className="mb-10 grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(320px,.8fr)]">
        <form onSubmit={publishService} className="surface-card p-6 sm:p-8">
          <h2 className="text-2xl font-extrabold text-slate-950">{editingService ? 'Edit service' : 'Publish a service'}</h2>
          <p className="mt-2 text-slate-600">{editingService
            ? 'Update the selected service details.'
            : 'New services appear immediately on the public Services page.'}</p>
          <div className="mt-6 grid gap-5">
            <label className="sm:col-span-2">
              <span className="form-label">Service name</span>
              <input className="form-control" value={name} onChange={event => setName(event.target.value)} required />
            </label>
            <label className="sm:col-span-2">
              <span className="form-label">Service details</span>
              <textarea className="form-control min-h-28 resize-y" value={description}
                onChange={event => setDescription(event.target.value)} required />
            </label>
            <label>
              <span className="form-label">Price (₹)</span>
              <input className="form-control" type="number" min="1" step="0.01" value={price}
                onChange={event => setPrice(event.target.value)} required />
            </label>
          </div>
          {message && <p className="mt-5 rounded-lg bg-teal-50 px-4 py-3 text-sm font-semibold text-teal-800">{message}</p>}
          {error && <p className="mt-5 text-sm font-medium text-red-600">{error}</p>}
          <div className="mt-6 flex flex-wrap gap-3">
            <button className="primary-button w-full sm:w-auto">{editingService ? 'Save Changes' : 'Publish Service'}</button>
            {editingService && (
              <button type="button" className="secondary-button w-full sm:w-auto" onClick={cancelEditing}>Cancel</button>
            )}
          </div>
        </form>

        <section className="surface-card p-6 sm:p-8">
          <h2 className="text-2xl font-extrabold text-slate-950">Available services</h2>
          <div className="mt-5 grid gap-3">
            {services.map(service => (
              <article key={service.id} className="rounded-lg border border-slate-200 p-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="min-w-0">
                    <h3 className="font-extrabold text-slate-950">{service.name}</h3>
                    <p className="mt-1 text-sm text-slate-600">₹{service.price}</p>
                  </div>
                  <div className="flex shrink-0 gap-3 text-sm font-bold">
                    <button type="button" onClick={() => startEditing(service)}
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
      </section>

      <section className="surface-card mb-10 p-6 sm:p-8">
        <div className="grid gap-6 lg:grid-cols-[minmax(280px,.7fr)_minmax(0,1fr)]">
          <form onSubmit={saveLocation}>
            <h2 className="text-2xl font-extrabold text-slate-950">
              {editingLocation ? 'Edit service location' : 'Add service location'}
            </h2>
            <label className="mt-5 block">
              <span className="form-label">Location name</span>
              <input
                className="form-control"
                value={locationName}
                onChange={event => setLocationName(event.target.value)}
                placeholder="Enter city or service area"
                maxLength={100}
                required
              />
            </label>
            <div className="mt-4 flex flex-wrap gap-3">
              <button className="primary-button" disabled={savingLocation}>
                {savingLocation ? 'Saving...' : editingLocation ? 'Save location' : 'Add location'}
              </button>
              {editingLocation && (
                <button type="button" className="secondary-button" onClick={cancelEditingLocation}>Cancel</button>
              )}
            </div>
          </form>

          <div>
            <h2 className="text-2xl font-extrabold text-slate-950">Current locations</h2>
            <div className="mt-5 grid gap-3 sm:grid-cols-2">
              {locations.map(location => (
                <div key={location.id} className="flex items-center justify-between gap-4 rounded-lg border border-slate-200 p-4">
                  <span className="font-bold text-slate-900">{location.name}</span>
                  <div className="flex shrink-0 items-center gap-3 text-sm font-bold">
                    <button type="button" className="text-teal-700 hover:text-teal-900"
                      onClick={() => startEditingLocation(location)}>Edit</button>
                    <button type="button" className="text-red-600 hover:text-red-800"
                      onClick={() => deleteLocation(location)}>Delete</button>
                  </div>
                </div>
              ))}
              {locations.length === 0 && <p className="text-sm text-slate-500">No service locations available.</p>}
            </div>
          </div>
        </div>
      </section>

      <h2 className="mb-5 text-2xl font-extrabold text-slate-950">All bookings</h2>
      <div className="surface-card overflow-x-auto">
      <table className="w-full min-w-[900px] overflow-hidden">
        <thead className="bg-teal-50 text-left text-sm text-teal-950">
          <tr>
            <th className="p-3">Customer</th>
            <th className="p-3">Vehicle</th>
            <th className="p-3">Service</th>
            <th className="p-3">Address</th>
            <th className="p-3">Time</th>
            <th className="bg-red-100 p-3 text-red-900">Expire date</th>
            <th className="p-3">Status</th>
            <th className="p-3 text-center">Delete</th>
          </tr>
        </thead>
        <tbody>
          {bookings.map(b => (
            <tr key={b.id} className="border-t border-slate-100 text-sm">
              <td className="p-3">{b.customerName}</td>
              <td className="p-3">{b.vehicle.make} {b.vehicle.model} ({b.vehicle.licensePlate})</td>
              <td className="p-3">{b.service.name} — ₹{b.service.price}</td>
              <td className="p-3">{b.address}, {b.city} - {b.pincode}</td>
              <td className="p-3">{new Date(b.scheduledAt).toLocaleString()}</td>
              <td className="bg-red-50 p-3 font-semibold text-red-800">
                {new Date(b.expireDate).toLocaleDateString()}
              </td>
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
                  className="inline-flex size-9 items-center justify-center rounded-lg text-red-600 hover:bg-red-50 hover:text-red-800 disabled:cursor-not-allowed disabled:opacity-50"
                  onClick={() => deleteBooking(b)}
                  disabled={deletingBookingId === b.id}
                  aria-label={`Delete ${b.customerName}'s booking`}
                  title="Delete booking"
                >
                  <Trash2 size={18} aria-hidden="true" />
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      </div>

      <h2 className="mb-5 mt-10 text-2xl font-extrabold text-slate-950">Customer details</h2>
      <div className="surface-card overflow-x-auto">
        <table className="w-full min-w-[760px]">
          <thead className="bg-teal-50 text-left text-sm text-teal-950">
            <tr>
              <th className="p-3">Name</th>
              <th className="p-3">Email</th>
              <th className="p-3">Phone number</th>
              <th className="p-3">Address</th>
              <th className="p-3 text-center">Delete</th>
            </tr>
          </thead>
          <tbody>
            {customers.map(customer => (
              <tr key={customer.id} className="border-t border-slate-100 text-sm">
                <td className="p-3 font-semibold">{customer.fullName}</td>
                <td className="p-3">{customer.email}</td>
                <td className="p-3">{customer.phoneNumber || 'Not provided'}</td>
                <td className="p-3">{customer.address || 'Not provided'}</td>
                <td className="p-3 text-center">
                  <button
                    type="button"
                    className="inline-flex size-9 items-center justify-center rounded-lg text-red-600 hover:bg-red-50 hover:text-red-800 disabled:cursor-not-allowed disabled:opacity-50"
                    onClick={() => deleteCustomer(customer)}
                    disabled={deletingCustomerId === customer.id}
                    aria-label={`Delete ${customer.fullName}`}
                    title="Delete customer"
                  >
                    <Trash2 size={18} aria-hidden="true" />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
