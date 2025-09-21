export default defineNuxtRouteMiddleware(async (to) => {
  // Skip middleware on server-side rendering
  if (process.server) return
  
  const authStore = useAuthStore()
 
  // Initialize auth state from cookies/localStorage
  await authStore.initializeAuth()
  
  console.log('Auth Middleware Debug:', {
    isAuthenticated: authStore.isAuthenticated,
    hasAccessToken: !!authStore.accessToken,
    hasRefreshToken: !!authStore.refreshToken,
    isTokenExpired: authStore.isTokenExpired,
    route: to.path
  })
  
  // Check if user is authenticated
  if (!authStore.isAuthenticated || !authStore.accessToken) {
    console.log('Redirecting to login - not authenticated')
    return navigateTo('/')
  }
  
  // Check if token is expired
  if (authStore.isTokenExpired) {
    console.log('Redirecting to login - token expired')
    authStore.clearAuth()
    return navigateTo('/?expired=true')
  }
  
  console.log('Auth middleware passed - user authenticated')
})