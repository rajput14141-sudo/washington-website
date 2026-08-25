import { createContext, useContext, useState, ReactNode } from 'react'
import { api } from '../api/client'

interface AuthUser {
  email: string
  fullName: string
  roles: string[]
}

interface AuthContextType {
  user: AuthUser | null
  login: (email: string, password: string) => Promise<void>
  register: (fullName: string, email: string, phoneNumber: string, address: string) => Promise<void>
  adminLogin: (email: string, password: string) => Promise<void>
  adminRegister: (fullName: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const raw = localStorage.getItem('user')
    return raw ? JSON.parse(raw) : null
  })

  function persist(token: string, u: AuthUser) {
    localStorage.setItem('token', token)
    localStorage.setItem('user', JSON.stringify(u))
    setUser(u)
  }

  async function login(email: string, password: string) {
    const { data } = await api.post('/auth/login', { email, password })
    persist(data.token, { email: data.email, fullName: data.fullName, roles: data.roles })
  }

  async function register(fullName: string, email: string, phoneNumber: string, address: string) {
    await api.post('/auth/register', { fullName, email, phoneNumber, address })
  }

  async function adminLogin(email: string, password: string) {
    const { data } = await api.post('/auth/admin/login', { email, password })
    persist(data.token, { email: data.email, fullName: data.fullName, roles: data.roles })
  }

  async function adminRegister(fullName: string, email: string, password: string) {
    await api.post('/auth/admin/register', { fullName, email, password })
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, login, register, adminLogin, adminRegister, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
