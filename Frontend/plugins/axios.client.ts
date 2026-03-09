import axios from 'axios'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()
  const router = useRouter()
  const authStore = useAuthStore()

  // Create axios instance
  const api = axios.create({
    baseURL: config.public.apiBase,
    timeout: 30000,
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  })

  // Refresh token işlemi devam ederken bekleyen istekler için
  let isRefreshing = false
  const failedQueue: Array<{
    resolve: (value?: unknown) => void
    reject: (reason?: unknown) => void
    config: import('axios').InternalAxiosRequestConfig
  }> = []

  const processQueue = (err: Error | null, token: string | null = null) => {
    failedQueue.forEach(({ resolve, reject, config }) => {
      if (err) {
        reject(err)
      } else if (config && token) {
        config.headers.Authorization = `Bearer ${token}`
        ;(config as any)._retry = true
        resolve(api(config))
      }
    })
    failedQueue.length = 0
  }

  // Request interceptor
  api.interceptors.request.use(
    (reqConfig) => {
      const token = useCookie('access_token')
      if (token.value) {
        reqConfig.headers.Authorization = `Bearer ${token.value}`
      }
      return reqConfig
    },
    (error) => Promise.reject(error)
  )

  // Response interceptor - 401'de token refresh
  api.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config

      if (error.response?.status !== 401) {
        return Promise.reject(error)
      }

      // Refresh endpoint'ine yapılan istek 401 dönerse tekrar deneme (sonsuz döngü önleme)
      if (originalRequest?.url?.includes('/auth/refresh-token')) {
        authStore.clearAuth()
        await router.push('/?expired=true')
        return Promise.reject(error)
      }

      // Zaten retry edilmiş istek
      if (originalRequest?._retry) {
        return Promise.reject(error)
      }

      const refreshTokenCookie = useCookie('refresh_token')
      const accessToken = authStore.accessToken || useCookie('access_token').value

      if (!refreshTokenCookie.value || !accessToken) {
        authStore.clearAuth()
        await router.push('/')
        return Promise.reject(error)
      }

      if (isRefreshing) {
        // Başka bir istek zaten refresh yapıyorsa kuyruğa ekle
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject, config: originalRequest })
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const response = await axios.post(
          `${config.public.apiBase}${API_ENDPOINTS.AUTH.REFRESH_TOKEN}`,
          { accessToken, refreshToken: refreshTokenCookie.value },
          { headers: { 'Content-Type': 'application/json' } }
        )

        const data = response.data?.data || response.data?.value || response.data
        if (data?.accessToken && data?.refreshToken) {
          await authStore.setAuth(data)
          processQueue(null, data.accessToken)
          originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
          return api(originalRequest)
        }

        throw new Error('Invalid refresh response')
      } catch (refreshError) {
        processQueue(refreshError as Error, null)
        authStore.clearAuth()
        await router.push('/?expired=true')
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }
  )

  return {
    provide: {
      api
    }
  }
}) 