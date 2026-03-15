import { defineStore } from 'pinia'
import type { User, LoginResponse, AuthState } from '~/types'
import { jwtDecode } from 'jwt-decode'

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    accessToken: null,
    refreshToken: null,
    isAuthenticated: false,
    isLoading: false,
    permissions: [],
    roles: []
  }),

  getters: {
    isLoggedIn: (state) => state.isAuthenticated && !!state.accessToken,
    userFullName: (state) => state.user ? `${state.user.firstName} ${state.user.lastName}` : '',
    userInitials: (state) => {
      if (!state.user) return ''
      return `${state.user.firstName.charAt(0)}${state.user.lastName.charAt(0)}`.toUpperCase()
    },
    hasPermission: (state) => (permission: string) => {
      return state.permissions.includes(permission)
    },
    hasRole: (state) => (role: string) => {
      return state.roles.includes(role)
    },
    hasAnyPermission: (state) => (permissions: string[]) => {
      return permissions.some(permission => state.permissions.includes(permission))
    },
    isTokenExpired: (state) => {
      if (!state.accessToken) return true
      
      try {
        const decoded: any = jwtDecode(state.accessToken)
        const currentTime = Date.now() / 1000
        return decoded.exp < currentTime
      } catch {
        return true
      }
    }
  },

  actions: {
    async setAuth(authData: LoginResponse, rememberMe?: boolean, deviceId?: string) {
      this.user = authData.user
      this.accessToken = authData.accessToken
      this.refreshToken = authData.refreshToken
      this.isAuthenticated = true
      this.permissions = (authData.user.permissions ?? []).map((p: { fullPermission?: string; name?: string }) => p?.fullPermission ?? p?.name).filter(Boolean)
      this.roles = authData.user.roles?.map(r => r.name) ?? []

      // Cookie süreleri: expiresAt varsa kullan (API'den), yoksa rememberMe'ye göre
      const accessTokenMaxAge = 60 * 60 * 2 // 2 saat (JWT süresine yakın)
      let refreshTokenMaxAge: number
      if (authData.expiresAt) {
        const expiresMs = new Date(authData.expiresAt).getTime() - Date.now()
        refreshTokenMaxAge = Math.max(60, Math.floor(expiresMs / 1000))
      } else {
        refreshTokenMaxAge = (rememberMe ?? false)
          ? 60 * 60 * 24 * 30  // 30 gün (Remember Me)
          : 60 * 60 * 24 * 7   // 7 gün (standart)
      }

      const accessTokenCookie = useCookie('access_token', {
        default: () => null,
        maxAge: accessTokenMaxAge,
        secure: true,
        sameSite: 'strict'
      })
      
      const refreshTokenCookie = useCookie('refresh_token', {
        default: () => null,
        maxAge: refreshTokenMaxAge,
        secure: true,
        sameSite: 'strict'
      })

      accessTokenCookie.value = authData.accessToken
      refreshTokenCookie.value = authData.refreshToken

      // deviceId ve rememberMe cookie'de sakla (localStorage clear'dan etkilenmesin)
      const cookieMaxAge = 60 * 60 * 24 * 30 // 30 gün
      if (deviceId !== undefined) {
        const deviceIdCookie = useCookie('device_id', { maxAge: cookieMaxAge, secure: true, sameSite: 'strict' })
        deviceIdCookie.value = deviceId
      }
      if (rememberMe !== undefined) {
        const rememberMeCookie = useCookie('remember_me', { maxAge: cookieMaxAge, secure: true, sameSite: 'strict' })
        rememberMeCookie.value = String(rememberMe)
      }

      // Store user data in localStorage for persistence
      if (process.client) {
        localStorage.setItem('user', JSON.stringify(authData.user))
        if (rememberMe !== undefined) localStorage.setItem('rememberMe', String(rememberMe))
        if (deviceId) localStorage.setItem('deviceId', deviceId)
      }
    },

    setUser(user: User) {
      this.user = user
      // fullPermission veya name kullan (API formatlarına uyum için)
      this.permissions = (user.permissions ?? []).map((p: { fullPermission?: string; name?: string }) => p?.fullPermission ?? p?.name).filter(Boolean)
      this.roles = user.roles?.map(r => r.name) ?? []

      if (process.client) {
        localStorage.setItem('user', JSON.stringify(user))
      }
    },

    clearAuth() {
      this.user = null
      this.accessToken = null
      this.refreshToken = null
      this.isAuthenticated = false
      this.permissions = []
      this.roles = []

      // Clear cookies
      const accessTokenCookie = useCookie('access_token')
      const refreshTokenCookie = useCookie('refresh_token')
      const deviceIdCookie = useCookie('device_id')
      const rememberMeCookie = useCookie('remember_me')
      accessTokenCookie.value = null
      refreshTokenCookie.value = null
      deviceIdCookie.value = null
      rememberMeCookie.value = null

      // Clear localStorage
      if (process.client) {
        localStorage.removeItem('user')
        localStorage.removeItem('rememberMe')
        localStorage.removeItem('deviceId')
      }
    },

    setLoading(loading: boolean) {
      this.isLoading = loading
    },

    async initializeAuth() {
      if (!process.client) return

      const accessTokenCookie = useCookie('access_token')
      const refreshTokenCookie = useCookie('refresh_token')
      
      if (accessTokenCookie.value && refreshTokenCookie.value) {
        this.accessToken = accessTokenCookie.value
        this.refreshToken = refreshTokenCookie.value
        this.isAuthenticated = true

        // Cookie'den deviceId ve rememberMe'yi localStorage'a restore et (clear sonrası)
        const deviceIdCookie = useCookie('device_id')
        const rememberMeCookie = useCookie('remember_me')
        if (deviceIdCookie.value) {
          localStorage.setItem('deviceId', deviceIdCookie.value)
        }
        if (rememberMeCookie.value !== undefined && rememberMeCookie.value !== null) {
          localStorage.setItem('rememberMe', rememberMeCookie.value)
        }

        // Restore user from localStorage
        const storedUser = localStorage.getItem('user')
        if (storedUser) {
          try {
            const user = JSON.parse(storedUser)
            this.setUser(user)
          } catch (error) {
            console.error('Error parsing stored user:', error)
            await this.fetchUserFromApi()
          }
        } else {
          // User yoksa API'den al (sidebar permissions için gerekli)
          await this.fetchUserFromApi()
        }
      } else {
        // No tokens found, ensure we're logged out
        this.isAuthenticated = false
        this.accessToken = null
        this.refreshToken = null
      }
    },

    async fetchUserFromApi(clearOnError = true) {
      const { $api } = useNuxtApp()
      try {
        const response = await $api.get('/api/auth/me')
        const user = response.data?.data || response.data?.value || response.data
        if (user) {
          this.setUser(user)
        } else if (clearOnError) {
          this.clearAuth()
        }
      } catch (error) {
        console.error('Error fetching user:', error)
        if (clearOnError) this.clearAuth()
      }
    }
  }
}) 