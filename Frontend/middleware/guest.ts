export default defineNuxtRouteMiddleware(() => {
  const authStore = useAuthStore()
  
  // Initialize auth state
  if (!authStore.isAuthenticated) {
    authStore.initializeAuth()
  }

  // If user is authenticated, redirect to dashboard
  if (authStore.isAuthenticated) {
    return navigateTo('/dashboard')
  }
}) 