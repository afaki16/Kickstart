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
    async setAuth(authData: LoginResponse) {
      console.log('setAuth called with:', authData)
      
      this.user = authData.user
      this.accessToken = authData.accessToken
      this.refreshToken = authData.refreshToken
      this.isAuthenticated = true
      this.permissions = authData.user.permissions.map(p => p.fullPermission)
      this.roles = authData.user.roles.map(r => r.name)

      console.log('Auth state after setAuth:', {
        isAuthenticated: this.isAuthenticated,
        hasAccessToken: !!this.accessToken,
        hasRefreshToken: !!this.refreshToken,
        user: this.user
      })

      // Store tokens in cookies
      const accessTokenCookie = useCookie('access_token', {
        default: () => null,
        maxAge: 60 * 60 * 24 * 7, // 7 days
        secure: true,
        sameSite: 'strict'
      })
      
      const refreshTokenCookie = useCookie('refresh_token', {
        default: () => null,
        maxAge: 60 * 60 * 24 * 30, // 30 days
        secure: true,
        sameSite: 'strict'
      })

      accessTokenCookie.value = authData.accessToken
      refreshTokenCookie.value = authData.refreshToken

      // Store user data in localStorage for persistence
      if (process.client) {
        localStorage.setItem('user', JSON.stringify(authData.user))
      }
    },

    setUser(user: User) {
      this.user = user
      this.permissions = user.permissions.map(p => p.fullPermission)
      this.roles = user.roles.map(r => r.name)

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
      accessTokenCookie.value = null
      refreshTokenCookie.value = null

      // Clear localStorage
      if (process.client) {
        localStorage.removeItem('user')
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

        // Restore user from localStorage
        const storedUser = localStorage.getItem('user')
        if (storedUser) {
          try {
            const user = JSON.parse(storedUser)
            this.setUser(user)
          } catch (error) {
            console.error('Error parsing stored user:', error)
            this.clearAuth()
          }
        }
      } else {
        // No tokens found, ensure we're logged out
        this.isAuthenticated = false
        this.accessToken = null
        this.refreshToken = null
      }
    }
  }
}) 