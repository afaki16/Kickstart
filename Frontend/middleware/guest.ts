export default defineNuxtRouteMiddleware(async () => {
  const authStore = useAuthStore()
  
  // Initialize auth state (await - cookie/localStorage senkronizasyonu için)
  if (!authStore.isAuthenticated) {
    await authStore.initializeAuth()
  }

  // If user is authenticated, redirect to dashboard
  if (authStore.isAuthenticated) {
    return navigateTo('/dashboard')
  }
}) 