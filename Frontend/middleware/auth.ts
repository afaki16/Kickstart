export default defineNuxtRouteMiddleware(async (to) => {
  if (process.server) return
  
  const authStore = useAuthStore()
  const auth = useAuth()

  await authStore.initializeAuth()
  
  if (!authStore.isAuthenticated || !authStore.accessToken) {
    return navigateTo('/')
  }
  
  // Token süresi dolmuşsa önce refresh dene
  if (authStore.isTokenExpired && authStore.refreshToken) {
    try {
      await auth.refreshToken()
      // Refresh başarılı, devam et
    } catch {
      authStore.clearAuth()
      return navigateTo('/?expired=true')
    }
  } else if (authStore.isTokenExpired) {
    authStore.clearAuth()
    return navigateTo('/?expired=true')
  }
})