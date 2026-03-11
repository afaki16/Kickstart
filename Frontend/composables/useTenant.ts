/**
 * Mevcut tenant context'ini çözer.
 * Subdomain (acme.uygulama.com → acme), URL param (?tenant=acme) vb.
 *
 * useTenants = API/CRUD (tenant yönetimi)
 * useTenant = "Şu an hangi tenant'tayız?" (context çözümleme)
 */
import { ref, computed, readonly } from 'vue'

export interface TenantConfig {
  layout?: string
  loginPage?: 'default' | string
  registerPage?: 'default' | string
}

export const useTenant = () => {
  const tenantId = ref<string | null>(null)
  const tenantConfig = ref<TenantConfig | null>(null)
  const isResolved = ref(false)

  /**
   * Subdomain'den tenant identifier çıkarır.
   * acme.uygulama.com → "acme"
   */
  const getSubdomain = (): string | null => {
    if (typeof window === 'undefined') return null

    const host = window.location.hostname
    const parts = host.split('.')

    if (host === 'localhost' || host.startsWith('127.')) return null
    if (parts[0] === 'www' && parts.length === 2) return null
    if (parts.length >= 2 && parts[0] !== 'www') {
      return parts[0].toLowerCase()
    }

    return null
  }

  /**
   * Tenant ID'yi öncelik sırasına göre çözer:
   * 1. ?tenant=acme
   * 2. Subdomain
   * 3. User.tenantId (login sonrası)
   * 4. Cookie / localStorage
   */
  const resolveTenantId = (): string | null => {
    if (typeof window === 'undefined') return null

    const params = new URLSearchParams(window.location.search)
    const paramTenant = params.get('tenant')
    if (paramTenant) return paramTenant.trim().toLowerCase()

    const subdomain = getSubdomain()
    if (subdomain) return subdomain

    try {
      const authStore = useAuthStore()
      const userTenantId = (authStore.user as { tenantId?: string })?.tenantId
      if (userTenantId) return String(userTenantId).toLowerCase()
    } catch {
      // Auth store henüz hazır olmayabilir
    }

    const stored = localStorage.getItem('tenant_id') ?? useCookie('tenant_id').value
    if (stored) return stored.trim().toLowerCase()

    return null
  }

  const initTenant = () => {
    const id = resolveTenantId()
    tenantId.value = id
    isResolved.value = true
    if (id && typeof window !== 'undefined') {
      const fromParam = new URLSearchParams(window.location.search).get('tenant')
      if (fromParam) {
        localStorage.setItem('tenant_id', id)
        useCookie('tenant_id').value = id
      }
    }
  }

  const setTenantId = (id: string | null) => {
    tenantId.value = id
    if (typeof window !== 'undefined' && id) {
      localStorage.setItem('tenant_id', id)
      useCookie('tenant_id').value = id
    } else if (typeof window !== 'undefined') {
      localStorage.removeItem('tenant_id')
      useCookie('tenant_id').value = null
    }
  }

  const getDataPath = computed(() => {
    const id = tenantId.value ?? resolveTenantId()
    return id ? `/data/${id}.json` : '/data.json'
  })

  const getLayoutName = computed(() => tenantConfig.value?.layout ?? 'default')
  const getLoginPage = computed(() => tenantConfig.value?.loginPage ?? 'default')

  return {
    tenantId: readonly(tenantId),
    tenantConfig: readonly(tenantConfig),
    isResolved: readonly(isResolved),
    getSubdomain,
    resolveTenantId,
    initTenant,
    setTenantId,
    getDataPath,
    getLayoutName,
    getLoginPage,
  }
}
