/**
 * Layout seçimi: .env'deki NUXT_PUBLIC_DEFAULT_LAYOUT (default | gradient)
 * layout: false olan sayfalar (login, register) etkilenmez.
 */
export default defineNuxtRouteMiddleware((to) => {
  if (to.meta.layout === false) return

  const config = useRuntimeConfig()
  const layout = config.public.defaultLayout || 'default'
  setPageLayout(layout)
})
