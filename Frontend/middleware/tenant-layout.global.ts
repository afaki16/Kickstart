/**
 * Layout seçimi (tek yerden yönetim):
 * 1. data.json'daki tenant.layout varsa → onu kullan (tenant öncelikli)
 * 2. Yoksa → .env'deki NUXT_PUBLIC_DEFAULT_LAYOUT (default | gradient)
 *
 * layout: false olan sayfalar (login, register) etkilenmez.
 */
export default defineNuxtRouteMiddleware(async (to) => {
  if (to.meta.layout === false) return

  const config = useRuntimeConfig()
  let layout = config.public.defaultLayout || 'default'

  const { loadAppData } = useAppData()
  const appData = await loadAppData()
  const tenantLayout = (appData as { tenant?: { layout?: string } })?.tenant?.layout
  if (tenantLayout) {
    layout = tenantLayout
  }

  setPageLayout(layout)
})
