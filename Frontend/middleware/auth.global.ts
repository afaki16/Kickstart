export default defineNuxtRouteMiddleware(async (to) => {
  // Skip middleware on server-side rendering
  if (process.server) return

  const authStore = useAuthStore()
  
  // Initialize auth state from cookies/localStorage (await - kritik: yeni kullanıcı girişinde menü için)
  if (!authStore.isAuthenticated) {
    await authStore.initializeAuth()
  }

  // Verify-email her zaman erişilebilir: kullanıcı email linkine
  // tıklayarak gelir; başka bir hesap altında authenticated olsa bile
  // token doğrulamasının yine de çalışması gerekir.
  if (to.path === '/auth/verify-email') return

  // Public routes that don't require authentication
  const publicRoutes = ['/auth/register', '/auth/check-email', '/']

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