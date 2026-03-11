/**
 * Uygulama başlangıcında tenant'ı çözer.
 * Subdomain veya ?tenant= param ile belirlenir.
 */
export default defineNuxtPlugin(() => {
  const { initTenant } = useTenant()
  initTenant()
})
