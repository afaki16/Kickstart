<template>
  <div class="min-h-screen bg-slate-50">
    <aside class="flex flex-col h-screen w-64 fixed left-0 top-0 bg-slate-50 border-r border-slate-200 p-4 space-y-2 z-50">
      <!-- Logo Alanı -->
      <div class="flex items-center space-x-3 px-2 py-4 mb-4">
        <div
          class="w-10 h-10 rounded-lg flex items-center justify-center text-white overflow-hidden flex-shrink-0"
          :style="{ background: 'var(--theme-primary)' }"
        >
          <img
            v-if="appData?.app?.logo?.src"
            :src="appData.app.logo.src"
            :alt="appData?.app?.logo?.alt || 'Logo'"
            class="max-w-[70%] max-h-[70%] object-contain"
          />
          <span v-else class="text-xl font-bold italic">
            {{ (appData?.app?.brand?.text || 'K').charAt(0).toUpperCase() }}
          </span>
        </div>
        <div class="min-w-0">
          <h1 class="text-xl font-bold text-slate-900 leading-tight truncate">
            {{ appData?.app?.brand?.text || 'Kickstart' }}
          </h1>
          <p class="text-xs text-slate-500 font-medium tracking-wide truncate">
            {{ $t('sidebar.brand.tagline') }}
          </p>
        </div>
      </div>

      <!-- Navigasyon Linkleri -->
      <nav :key="authStore.user?.id || 'guest'" class="flex-1 space-y-1 overflow-y-auto">
        <NuxtLink
          v-for="item in flatMenus"
          :key="item.to"
          :to="item.to"
          class="flex items-center space-x-3 px-4 py-3 rounded-lg transition-all duration-200 font-semibold text-sm group"
          :class="[
            isActive(item.to)
              ? 'bg-white shadow-sm'
              : 'text-slate-600 hover:bg-slate-100 hover:translate-x-1'
          ]"
          :style="isActive(item.to) ? { color: 'var(--theme-primary)' } : {}"
        >
          <Icon
            :name="item.icon"
            class="w-5 h-5 flex-shrink-0"
            :style="isActive(item.to) ? { color: 'var(--theme-primary)' } : {}"
          />
          <span class="truncate">{{ $t(item.title) }}</span>
        </NuxtLink>
      </nav>

      <!-- Alt Kısım / Footer -->
      <div class="pt-4 border-t border-slate-200 space-y-1">
        <div class="px-2 pb-2">
          <LanguageSwitcher class="lang-switcher-pure" />
        </div>
        <button
          type="button"
          class="w-full flex items-center space-x-3 px-4 py-3 text-slate-600 hover:bg-slate-100 rounded-lg transition-all text-sm font-semibold group"
        >
          <Icon name="mdi:help-circle-outline" class="w-5 h-5 flex-shrink-0" />
          <span>{{ $t('sidebar.actions.support') }}</span>
        </button>
        <button
          type="button"
          @click="logout"
          class="w-full flex items-center space-x-3 px-4 py-3 text-slate-600 hover:bg-red-50 hover:text-red-600 rounded-lg transition-all text-sm font-semibold group"
        >
          <Icon name="mdi:logout" class="w-5 h-5 flex-shrink-0 group-hover:text-red-600" />
          <span>{{ $t('sidebar.actions.logout') }}</span>
        </button>
      </div>
    </aside>

    <!-- Main content -->
    <main class="ml-64 p-6 min-h-screen">
      <slot />
    </main>
  </div>
</template>

<script setup>
import { storeToRefs } from 'pinia'
import { navigationItems, filterNavigationByPermissions } from '~/composables/useNavigation'
import { useAuth } from '~/composables/useAuth'
import { useAppData } from '~/composables/useAppData'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()
const { permissions, roles } = storeToRefs(authStore)
const authUtils = useAuth()
const route = useRoute()

const { loadAppData, appData } = useAppData()

const visibleMenus = computed(() => {
  return filterNavigationByPermissions(
    navigationItems,
    (p) => permissions.value.includes(p),
    (r) => roles.value.includes(r)
  )
})

// Tasarım düz liste olduğu için grupları düzleştir
const flatMenus = computed(() => {
  const result = []
  for (const item of visibleMenus.value) {
    if (item.children?.length) {
      for (const child of item.children) {
        if (child.to) result.push(child)
      }
    } else if (item.to) {
      result.push(item)
    }
  }
  return result
})

const isActive = (to) => {
  if (!to) return false
  return route.path === to || route.path.startsWith(to + '/')
}

const logout = async () => {
  try {
    await authUtils.logout()
  } catch (error) {
    console.error('Çıkış yapılırken bir hata oluştu:', error)
  }
}

onMounted(async () => {
  await loadAppData()
})
</script>

<style scoped>
.lang-switcher-pure :deep(.lang-switcher-btn) {
  width: 100%;
  justify-content: flex-start;
  background: transparent;
  border-color: rgb(226 232 240);
  color: rgb(71 85 105);
}

.lang-switcher-pure :deep(.lang-switcher-btn:hover) {
  background: rgb(241 245 249);
}
</style>
