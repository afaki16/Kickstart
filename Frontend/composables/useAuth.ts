import type { LoginRequest, RegisterRequest, LoginResponse, User } from '~/types'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'
import { useRouter } from 'nuxt/app'
import { useAuthStore } from '~/stores/auth'
import { useApi } from './useApi'

export const useAuth = () => {
  const api = useApi()
  const authStore = useAuthStore()
  const router = useRouter()

  // Device ID oluştur
  const generateDeviceId = (): string => {
    // Client-side kontrolü
    if (process.client) {
      // Local storage'dan mevcut device ID'yi al veya yeni oluştur
      let deviceId = localStorage.getItem('deviceId')
      if (!deviceId) {
        deviceId = 'device_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
        localStorage.setItem('deviceId', deviceId)
      }
      return deviceId
    }
    // Server-side için fallback
    return 'device_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
  }

  // Device name al
  const getDeviceName = (): string => {
    // Client-side kontrolü
    if (process.client) {
      const userAgent = navigator.userAgent
      const platform = navigator.platform
      
      if (userAgent.includes('Windows')) {
        return 'Windows Device'
      } else if (userAgent.includes('Mac')) {
        return 'Mac Device'
      } else if (userAgent.includes('Linux')) {
        return 'Linux Device'
      } else if (userAgent.includes('Android')) {
        return 'Android Device'
      } else if (userAgent.includes('iOS')) {
        return 'iOS Device'
      } else {
        return 'Unknown Device'
      }
    }
    // Server-side için fallback
    return 'Unknown Device'
  }

  const login = async (credentials: LoginRequest) => {
    try {
      authStore.setLoading(true)
      
      // Device bilgilerini ekle
      const requestData = {
        ...credentials,
        deviceId: credentials.deviceId || generateDeviceId(),
        deviceName: credentials.deviceName || getDeviceName()
      }
      
      const response = await api.post<LoginResponse>(API_ENDPOINTS.AUTH.LOGIN, requestData)
      
      console.log('Login Response:', response)
      
      // Farklı response formatlarını destekle
      let loginData = null
      
      if (response.success && response.data) {
        // Standart format
        loginData = response.data
      } else if (response.accessToken) {
        // Direkt response format
        loginData = response
      } else if (response.data && response.data.accessToken) {
        // Nested data format
        loginData = response.data
      }
      
      if (loginData && loginData.accessToken) {
        await authStore.setAuth(loginData)
        await router.push('/dashboard')
        return loginData
      } else {
        throw new Error('Invalid login response format')
      }
    } catch (error) {
      console.error('Login error:', error)
      throw error
    } finally {
      authStore.setLoading(false)
    }
  }

  const register = async (userData: RegisterRequest) => {
    try {
      authStore.setLoading(true)
      const response = await api.post<User>(API_ENDPOINTS.AUTH.REGISTER, userData)
      
      if (response.success) {
        await router.push('/')
        return response.data
      }
    } catch (error) {
      console.error('Registration error:', error)
      throw error
    } finally {
      authStore.setLoading(false)
    }
  }

  const logout = async () => {
    try {
      const refreshToken = authStore.refreshToken
      if (refreshToken) {
        await api.post(API_ENDPOINTS.AUTH.LOGOUT, { refreshToken })
      }
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      authStore.clearAuth()
      await router.push('/')
    }
  }

  const refreshToken = async () => {
    try {
      const accessToken = authStore.accessToken
      const refreshToken = authStore.refreshToken
      
      if (!refreshToken || !accessToken) {
        throw new Error('No tokens available')
      }

      const response = await api.post<LoginResponse>(API_ENDPOINTS.AUTH.REFRESH_TOKEN, {
        accessToken,
        refreshToken
      })

      if (response.success && response.data) {
        await authStore.setAuth(response.data)
        return response.data
      }
    } catch (error) {
      console.error('Token refresh error:', error)
      authStore.clearAuth()
      await router.push('/?expired=true')
      throw error
    }
  }

  const getCurrentUser = async () => {
    try {
      const response = await api.get<User>(API_ENDPOINTS.AUTH.ME)
      if (response.success && response.data) {
        authStore.setUser(response.data)
        return response.data
      }
    } catch (error) {
      console.error('Get current user error:', error)
      throw error
    }
  }

  const hasPermission = (permission: string): boolean => {
    return authStore.permissions.includes(permission)
  }

  const hasRole = (role: string): boolean => {
    return authStore.roles.includes(role)
  }

  const hasAnyPermission = (permissions: string[]): boolean => {
    return permissions.some(permission => hasPermission(permission))
  }

  const hasAllPermissions = (permissions: string[]): boolean => {
    return permissions.every(permission => hasPermission(permission))
  }

  return {
    login,
    register,
    logout,
    refreshToken,
    getCurrentUser,
    hasPermission,
    hasRole,
    hasAnyPermission,
    hasAllPermissions
  }
} 