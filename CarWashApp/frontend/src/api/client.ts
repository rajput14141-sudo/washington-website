import axios from 'axios'

const configuredApiUrl = import.meta.env.VITE_API_BASE_URL?.trim()

export const api = axios.create({
  baseURL: configuredApiUrl || (import.meta.env.DEV ? 'https://localhost:5001/api' : '/api'),
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})
