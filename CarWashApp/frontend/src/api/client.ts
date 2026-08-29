import axios from 'axios'

const configuredApiUrl = import.meta.env.VITE_API_BASE_URL?.trim()

export const api = axios.create({
  baseURL: configuredApiUrl || (
    import.meta.env.DEV
      ? 'https://localhost:5001/api'
      : '/api'
  ),
  timeout: 15000,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const isUnauthorized = error.response?.status === 401
    const isAuthRequest = String(error.config?.url ?? '').includes('/auth/')

    if (isUnauthorized && !isAuthRequest) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')

      const loginPath = window.location.pathname.startsWith('/admin')
        ? '/admin-access'
        : '/login'

      window.location.assign(loginPath)
    }

    return Promise.reject(error)
  }
)