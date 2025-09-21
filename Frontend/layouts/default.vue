<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Sidebar -->
    <aside class="fixed inset-y-0 left-0 w-64 bg-white shadow-lg transform transition-transform duration-300 ease-in-out" :class="{ '-translate-x-full': !isSidebarOpen }">
      <div class="flex items-center justify-center h-7 border-b">
        
      </div>
      
      <nav class="mt-5 px-2 space-y-6">
        <!-- Dinamik Navigation Items -->
        <template v-for="item in visibleMenus" :key="item.title">
          <!-- Ana menü öğesi (alt menü yoksa) -->
          <NuxtLink 
            v-if="!item.children && item.to" 
            :to="item.to" 
            class="group flex items-center px-2 py-2 text-base font-medium rounded-md" 
            :class="[$route.path === item.to ? 'bg-indigo-100 text-indigo-600' : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900']"
          >
            <Icon :name="item.icon" class="mr-3 h-6 w-6" />
            {{ item.title }}
          </NuxtLink>

          <!-- Alt menü grubu varsa -->
          <div v-else-if="item.children">
            <h3 class="px-2 text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
              {{ item.title }}
            </h3>
            <template v-for="child in item.children" :key="child.title">
              <NuxtLink 
                :to="child.to" 
                class="group flex items-center px-2 py-2 text-base font-medium rounded-md mt-1" 
                :class="[$route.path.startsWith(child.to) ? 'bg-indigo-100 text-indigo-600' : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900']"
              >
                <Icon :name="child.icon" class="mr-3 h-6 w-6" />
                {{ child.title }}
              </NuxtLink>
            </template>
          </div>
        </template>
      </nav>
    </aside>

    <!-- Main content -->
    <div class="flex flex-col min-h-screen transition-all duration-300" :class="{ 'pl-64': isSidebarOpen, 'pl-0': !isSidebarOpen }">
      <!-- Top navbar -->
      <header 
        class="fixed top-0 right-0 left-0 shadow-sm z-10" 
        :class="{ 'left-64': isSidebarOpen, 'left-0': !isSidebarOpen }"
        :style="{ background: appData?.theme?.gradients?.navbar || 'linear-gradient(135deg, #ffffff 0%, #2563eb 100%)' }"
      >
        <div class="flex items-center justify-between h-16 px-4">
          <div class="flex items-center">
            <div class="flex items-center">
              <img 
                :src="appData?.app?.logo?.src" 
                :class="`w-${appData?.app?.logo?.width || '8'} h-${appData?.app?.logo?.height || '8'} text-indigo-600`" 
                :alt="appData?.app?.logo?.alt || 'Logo'" 
              />
              <span class="ml-2 text-xl font-semibold text-gray-800">{{ appData?.app?.brand?.text || 'BaseAuth' }}</span>
            </div>
            <v-btn icon @click="toggleSidebar" class="ml-4">
              <v-icon>mdi-menu</v-icon>
            </v-btn>
          </div>
          
          <div class="flex items-center space-x-4">
            <LanguageSelector class="mr-4" />
            
            <!-- User Profile Dropdown -->
            <div class="relative">
              <button
                @click="showUserMenu = !showUserMenu"
                class="flex items-center space-x-3 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                <div class="w-8 h-8 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-full flex items-center justify-center">
                  <Icon name="mdi:account" class="w-4 h-4 text-white" />
                </div>
                <span class="truncate">{{ userInfo.name || 'Kullanıcı Adı' }}</span>
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
                </svg>
              </button>

              <!-- Dropdown Menu -->
              <div
                v-if="showUserMenu"
                class="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg border border-gray-200 z-50"
              >
                <div class="py-1">
                  <button @click="handleProfileClick" class="flex items-center w-full px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                    <Icon name="mdi:account-circle" class="mr-3 h-5 w-5" />
                    Profil
                  </button>
                  <button @click="handleSettingsClick" class="flex items-center w-full px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                    <Icon name="mdi:cog" class="mr-3 h-5 w-5" />
                    Ayarlar
                  </button>
                  <div class="border-t border-gray-200 my-1"></div>
                  <button @click="logout" class="flex items-center w-full px-4 py-2 text-sm text-red-600 hover:bg-red-50">
                    <Icon name="mdi:logout" class="mr-3 h-5 w-5" />
                    Çıkış Yap
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </header>

      <!-- Page content -->
      <main class="flex-1 p-6 mt-16">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup>
import { navigationItems, filterNavigationByPermissions } from '~/composables/useNavigation'
import { useAuth } from '~/composables/useAuth'
import { useAppData } from '~/composables/useAppData'
import { useAuthStore } from '~/stores/auth'

const isSidebarOpen = ref(true)
const showUserMenu = ref(false)
const authStore = useAuthStore()
const authUtils = useAuth() // <-- asıl yetki fonksiyonları burada
const router = useRouter()

// App data
const { loadAppData, appData } = useAppData()

// Kullanıcı bilgileri - auth store'dan al
const userInfo = computed(() => ({
  name: authStore.userFullName || 'Kullanıcı',
  email: authStore.user?.email || ''
}))

// Yetkiye göre menüleri filtrele
const visibleMenus = computed(() => {
  return filterNavigationByPermissions(
    navigationItems,
    authUtils.hasPermission,
    authUtils.hasRole
  )
})

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const logout = async () => {
  try {
    await authUtils.logout()
    showUserMenu.value = false
  } catch (error) {
    console.error('Çıkış yapılırken bir hata oluştu:', error)
  }
}

// Kullanıcı menüsü işlemleri
const handleProfileClick = () => {
  showUserMenu.value = false
  router.push('/profile')
}

const handleSettingsClick = () => {
  showUserMenu.value = false
  router.push('/settings')
}

// Dışarı tıklandığında menüyü kapat
onMounted(async () => {
  // Load app data
  await loadAppData()
  
  document.addEventListener('click', (e) => {
    if (!e.target.closest('.relative')) {
      showUserMenu.value = false
    }
  })
})
</script>

<style scoped>
/* Navbar styles */

/* Navbar içindeki text renklerini ayarla */
.gradient-navbar .text-gray-800 {
  color: #1e40af;
  font-weight: 600;
}

.gradient-navbar .text-gray-700 {
  color: #1e40af;
}

/* Logo ve başlık için gradient text */
.gradient-navbar .text-xl {
  background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  font-weight: 700;
}
</style>