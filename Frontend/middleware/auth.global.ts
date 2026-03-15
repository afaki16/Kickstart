export default defineNuxtRouteMiddleware(async (to) => {
  // Skip middleware on server-side rendering
  if (process.server) return

  const authStore = useAuthStore()
  
  // Initialize auth state from cookies/localStorage (await - kritik: yeni kullanıcı girişinde menü için)
  if (!authStore.isAuthenticated) {
    await authStore.initializeAuth()
  }

  // Public routes that don't require authentication
  const publicRoutes = ['/auth/register', '/']
  
  if (publicRoutes.includes(to.path)) {
    // If user is already authenticated and trying to access auth pages, redirect to dashboard
    if (authStore.isAuthenticated && to.path.startsWith('/auth/')) {
      return navigateTo('/dashboard')
    }
    return
  }

  // Protected routes - require authentication
  if (!authStore.isAuthenticated) {
    return navigateTo('/')
  }

  // Check if token is expired
  if (authStore.isTokenExpired) {
    authStore.clearAuth()
    return navigateTo('/?expired=true')
  }
}) 