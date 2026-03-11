/**
 * Mevcut tenant context'ini çözer.
 * Subdomain (acme.uygulama.com → acme), URL param (?tenant=acme) vb.
 *
 * useTenants = API/CRUD (tenant yönetimi)
 * useTenant = "Şu an hangi tenant'tayız?" (context çözümleme)
 */
import { ref, computed, readonly } from 'vue'

export const useTenant = () => {
  const tenantId = ref<string | null>(null)
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
   * Tenant identifier'ı çözer (data.json path: /data/{domain}.json).
   * Öncelik: 1) Subdomain 2) User (JWT/API) 3) Cookie 4) URL param (sadece localhost dev)
   */
  const resolveTenantId = (): string | null => {
    if (typeof window === 'undefined') return null

    const subdomain = getSubdomain()
    if (subdomain) return subdomain

    try {
      const authStore = useAuthStore()
      const user = authStore.user as { tenantDomain?: string; tenantId?: number } | null
      if (user?.tenantDomain) return user.tenantDomain.trim().toLowerCase()
      if (user?.tenantId != null) return String(user.tenantId)
    } catch {
      // Auth store henüz hazır olmayabilir
    }

    const stored = localStorage.getItem('tenant_id') ?? useCookie('tenant_id').value
    if (stored) return String(stored).trim().toLowerCase()

    const params = new URLSearchParams(window.location.search)
    const paramTenant = params.get('tenant')
    if (paramTenant) return paramTenant.trim().toLowerCase()

    return null
  }

  const initTenant = () => {
    const id = resolveTenantId()
    tenantId.value = id
    isResolved.value = true
    if (id && typeof window !== 'undefined') {
      const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname.startsWith('127.')
      const fromParam = new URLSearchParams(window.location.search).get('tenant')
      if (isLocalhost && fromParam) {
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
    return id && id !== 'default' ? `/data/${id}.json` : '/data.json'
  })

  return {
    tenantId: readonly(tenantId),
    isResolved: readonly(isResolved),
    getSubdomain,
    resolveTenantId,
    initTenant,
    setTenantId,
    getDataPath,
  }
}
