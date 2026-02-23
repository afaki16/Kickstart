<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Sidebar -->
    <aside 
      class="fixed inset-y-0 left-0 bg-white shadow-lg transition-all duration-300 ease-in-out flex flex-col z-40"
      :class="isSidebarOpen ? 'w-64' : 'w-16'"
    >
      <div class="flex items-center justify-center h-16 border-b flex-shrink-0">
        <img 
          :src="appData?.app?.logo?.src" 
          class="object-contain"
          :style="{ height: '40px', width: 'auto' }"
          :alt="appData?.app?.logo?.alt || 'Logo'" 
        />
      </div>
      
      <nav class="flex-1 mt-5 px-2 space-y-6 overflow-y-auto overflow-x-hidden">
        <!-- Dinamik Navigation Items -->
        <template v-for="item in visibleMenus" :key="item.title">
          <!-- Ana menü öğesi (alt menü yoksa) -->
          <NuxtLink 
            v-if="!item.children && item.to" 
            :to="item.to" 
            class="group flex items-center py-2 text-base font-medium rounded-md whitespace-nowrap"
            :class="[
              $route.path === item.to ? 'bg-indigo-100 text-indigo-600' : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
              isSidebarOpen ? 'px-2' : 'justify-center px-0'
            ]"
            :title="!isSidebarOpen ? item.title : undefined"
          >
            <Icon :name="item.icon" class="h-6 w-6 flex-shrink-0" :class="{ 'mr-3': isSidebarOpen }" />
            <span v-if="isSidebarOpen">{{ item.title }}</span>
          </NuxtLink>

          <!-- Alt menü grubu varsa -->
          <div v-else-if="item.children">
            <h3 v-if="isSidebarOpen" class="px-2 text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
              {{ item.title }}
            </h3>
            <template v-for="child in item.children" :key="child.title">
              <NuxtLink 
                :to="child.to" 
                class="group flex items-center py-2 text-base font-medium rounded-md mt-1 whitespace-nowrap"
                :class="[
                  $route.path.startsWith(child.to) ? 'bg-indigo-100 text-indigo-600' : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
                  isSidebarOpen ? 'px-2' : 'justify-center px-0'
                ]"
                :title="!isSidebarOpen ? child.title : undefined"
              >
                <Icon :name="child.icon" class="h-6 w-6 flex-shrink-0" :class="{ 'mr-3': isSidebarOpen }" />
                <span v-if="isSidebarOpen">{{ child.title }}</span>
              </NuxtLink>
            </template>
          </div>
        </template>
      </nav>

      <!-- User Profile - Sidebar Bottom -->
      <div class="border-t flex-shrink-0 relative">
        <button
          @click="showUserMenu = !showUserMenu"
          class="w-full flex items-center p-3 hover:bg-gray-50 transition-all duration-200"
          :class="isSidebarOpen ? 'space-x-3' : 'justify-center'"
        >
          <div class="sidebar-avatar flex-shrink-0">
            <Icon name="mdi:account" class="w-5 h-5 text-white" />
          </div>
          <div v-if="isSidebarOpen" class="flex-1 min-w-0 text-left">
            <div class="text-sm font-semibold text-gray-800 truncate">{{ userInfo.name || 'Kullanıcı Adı' }}</div>
            <div class="text-xs text-gray-500 truncate">{{ userInfo.email || 'admin@theetify.com' }}</div>
          </div>
          <svg v-if="isSidebarOpen" class="w-4 h-4 text-gray-400 transition-transform duration-200" :class="{ 'rotate-180': showUserMenu }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7"></path>
          </svg>
        </button>

        <!-- Dropdown Menu (yukarı açılır) -->
        <div
          v-if="showUserMenu"
          class="sidebar-user-dropdown absolute bottom-full mb-2 bg-white rounded-2xl shadow-xl border border-gray-100 z-50"
          :class="isSidebarOpen ? 'left-2 right-2' : 'left-0 w-56'"
        >
          <div class="dropdown-header">
            <div class="user-avatar-large">
              <Icon name="mdi:account" class="w-6 h-6 text-white" />
            </div>
            <div class="user-info">
              <div class="user-name">{{ userInfo.name || 'Kullanıcı Adı' }}</div>
              <div class="user-email">{{ userInfo.email || 'admin@theetify.com' }}</div>
              <div class="user-badge">
                <Icon name="mdi:shield-crown" class="w-3 h-3" />
                <span>Admin</span>
              </div>
            </div>
          </div>
          
          <div class="dropdown-divider"></div>
          
          <div class="dropdown-menu">
            <button @click="handleProfileClick" class="dropdown-item">
              <div class="item-icon profile-icon">
                <Icon name="mdi:account-circle" class="w-4 h-4" />
              </div>
              <div class="item-content">
                <span class="item-title">Profil Ayarları</span>
                <span class="item-desc">Hesap bilgilerini düzenle</span>
              </div>
            </button>
            
            <button @click="handleSettingsClick" class="dropdown-item">
              <div class="item-icon settings-icon">
                <Icon name="mdi:cog" class="w-4 h-4" />
              </div>
              <div class="item-content">
                <span class="item-title">Sistem Ayarları</span>
                <span class="item-desc">Uygulama tercihlerini yönet</span>
              </div>
            </button>
            
            <div class="dropdown-divider"></div>
            
            <button @click="logout" class="dropdown-item logout-item">
              <div class="item-icon logout-icon">
                <Icon name="mdi:logout" class="w-4 h-4" />
              </div>
              <div class="item-content">
                <span class="item-title">Çıkış Yap</span>
                <span class="item-desc">Güvenli çıkış yap</span>
              </div>
            </button>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main content -->
    <div class="flex flex-col min-h-screen transition-all duration-300" :class="isSidebarOpen ? 'pl-64' : 'pl-16'">
      <!-- Top navbar -->
      <header 
        class="fixed top-0 right-0 shadow-sm z-50 transition-all duration-300" 
        :class="isSidebarOpen ? 'left-64' : 'left-16'"
        :style="{ background: appData?.theme?.gradients?.navbar || 'linear-gradient(135deg, #ffffff 0%, #2563eb 100%)' }"
      >
        <div class="flex items-center justify-between h-16 px-4">
          <div class="flex items-center">
            <v-btn 
              icon 
              @click="toggleSidebar" 
              class="bg-transparent hover:bg-white/10" 
              variant="text"
              color="primary"
            >
              <v-icon size="x-large" class="font-weight-black">mdi-menu</v-icon>
            </v-btn>
          </div>
          
          <div class="flex items-center space-x-4">
            <!-- <LanguageSelector class="mr-4" /> -->
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
const authUtils = useAuth()
const router = useRouter()

const { loadAppData, appData } = useAppData()

const userInfo = computed(() => ({
  name: authStore.userFullName || 'Kullanıcı',
  email: authStore.user?.email || ''
}))

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

const handleProfileClick = () => {
  showUserMenu.value = false
  router.push('/profile')
}

const handleSettingsClick = () => {
  showUserMenu.value = false
  router.push('/settings')
}

onMounted(async () => {
  await loadAppData()
  
  document.addEventListener('click', (e) => {
    if (!e.target.closest('.border-t')) {
      showUserMenu.value = false
    }
  })
})
</script>

<style scoped>
/* Scrollbar */
nav::-webkit-scrollbar {
  width: 6px;
}

nav::-webkit-scrollbar-track {
  background: #f1f1f1;
}

nav::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

nav::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

/* Sidebar Avatar */
.sidebar-avatar {
  width: 36px;
  height: 36px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Sidebar User Dropdown */
.sidebar-user-dropdown {
  backdrop-filter: blur(20px);
  background: rgba(255, 255, 255, 0.95);
  animation: dropdownFadeUp 0.2s ease-out;
}

@keyframes dropdownFadeUp {
  from {
    opacity: 0;
    transform: translateY(10px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

/* Dropdown Header */
.dropdown-header {
  padding: 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 16px 16px 0 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar-large {
  width: 48px;
  height: 48px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.user-name {
  font-weight: 700;
  font-size: 1rem;
  color: white;
  line-height: 1.2;
}

.user-email {
  font-size: 0.75rem;
  color: rgba(255, 255, 255, 0.8);
  margin-top: 2px;
}

.user-badge {
  display: flex;
  align-items: center;
  gap: 4px;
  background: rgba(255, 255, 255, 0.2);
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.625rem;
  font-weight: 600;
  margin-top: 6px;
  width: fit-content;
}

/* Dropdown Divider */
.dropdown-divider {
  height: 1px;
  background: linear-gradient(90deg, transparent 0%, #e5e7eb 50%, transparent 100%);
  margin: 0;
}

/* Dropdown Menu */
.dropdown-menu {
  padding: 8px;
}

/* Dropdown Item */
.dropdown-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  text-align: left;
  background: transparent;
  border: none;
  border-radius: 12px;
  transition: all 0.2s ease;
  cursor: pointer;
}

.dropdown-item:hover {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
  transform: translateX(2px);
}

.item-icon {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.profile-icon {
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: white;
}

.settings-icon {
  background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
  color: white;
}

.logout-icon {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
}

.item-content {
  flex: 1;
  min-width: 0;
}

.item-title {
  display: block;
  font-weight: 600;
  font-size: 0.875rem;
  color: #1f2937;
  line-height: 1.2;
}

.item-desc {
  display: block;
  font-size: 0.75rem;
  color: #6b7280;
  margin-top: 2px;
  line-height: 1.2;
}

.logout-item:hover {
  background: linear-gradient(135deg, rgba(239, 68, 68, 0.05) 0%, rgba(220, 38, 38, 0.05) 100%);
}

.logout-item .item-title {
  color: #dc2626;
}
</style>
