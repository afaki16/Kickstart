/**
 * Tenant'a göre layout seçer.
 * data.json'daki tenant.layout değerine göre setPageLayout çağrılır.
 * layout: false olan sayfalar (login, register) etkilenmez.
 */
export default defineNuxtRouteMiddleware(async (to) => {
  if (to.meta.layout === false) return

  const { loadAppData } = useAppData()
  const appData = await loadAppData()

  const layout = (appData as { tenant?: { layout?: string } })?.tenant?.layout
  if (layout) {
    setPageLayout(layout)
  }
})
