<template>
  <div class="min-h-screen bg-[#f4f7fa]">
    <!-- Sidebar Overlay (Mobile Only) -->
    <div 
      v-if="isMobile && isSidebarOpen" 
      @click="toggleSidebar"
      class="fixed inset-0 bg-gray-900 bg-opacity-40 backdrop-blur-sm z-40 transition-opacity duration-300"
    ></div>

    <!-- Sidebar -->
    <aside 
      class="fixed inset-y-0 left-0 bg-[#f8f9fa] border-r border-gray-200 transform transition-all duration-300 ease-in-out flex flex-col z-50" 
      :class="[
        isSidebarOpen ? 'w-[260px] translate-x-0' : (isMobile ? '-translate-x-full w-[260px]' : 'w-[72px] translate-x-0')
      ]"
    >
      <!-- Logo Section -->
      <div 
        class="h-20 flex items-center px-4 transition-all overflow-hidden whitespace-nowrap" 
        :class="isSidebarOpen ? 'justify-between' : 'justify-center cursor-pointer hover:bg-gray-100/50'"
        @click="!isSidebarOpen && !isMobile && toggleSidebar()"
      >
        <v-tooltip :disabled="isSidebarOpen" location="right">
          <template v-slot:activator="{ props }">
            <div class="flex items-center min-w-max" v-bind="props">
              <img 
                :src="appData?.app?.logo?.src || '/favicon.ico'" 
                class="w-8 h-8 object-contain shrink-0" 
                :alt="appData?.app?.logo?.alt || 'Logo'" 
              />
              <span 
                class="ml-3 text-[19px] font-bold text-gray-900 tracking-tight transition-opacity duration-300"
                :class="{ 'opacity-0 hidden': !isSidebarOpen }"
              >
                {{ appData?.app?.brand?.text || 'Kickstart' }}
              </span>
            </div>
          </template>
          <span>{{ appData?.app?.brand?.text || 'Kickstart' }}</span>
        </v-tooltip>
        
        <button v-if="isSidebarOpen && !isMobile" @click.stop="toggleSidebar" class="p-1.5 text-gray-400 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors focus:outline-none">
          <Icon name="mdi:dock-left" class="h-5 w-5" />
        </button>
      </div>
      
      <!-- Quick Actions / Search -->
      <div class="px-3 pt-6 pb-4 transition-all overflow-hidden whitespace-nowrap" :class="!isSidebarOpen ? 'px-2' : ''">
        <h3 
          class="text-[10px] font-bold text-gray-400 uppercase tracking-widest mb-2 px-2 transition-opacity"
          v-if="isSidebarOpen"
        >
          Quick Actions
        </h3>
        <div class="relative" v-if="isSidebarOpen">
          <Icon name="mdi:magnify" class="absolute left-3 top-2.5 text-gray-400 h-[18px] w-[18px]" />
          <input type="text" placeholder="Search" class="w-full bg-white border border-gray-200 text-[13px] rounded-lg pl-9 pr-9 py-2 text-gray-700 placeholder-gray-400 focus:outline-none focus:ring-1 focus:ring-gray-300 focus:border-gray-300 transition-all shadow-sm">
          <div class="absolute right-2 top-1.5 flex items-center justify-center w-6 h-6 bg-gray-100 border border-gray-200 rounded text-[11px] text-gray-500 font-medium">
            ⌘
          </div>
        </div>
        <!-- Collapsed Search Icon Button -->
        <button v-else @click="toggleSidebar" class="w-10 h-10 mx-auto flex items-center justify-center bg-white border border-gray-200 rounded-lg text-gray-500 hover:text-gray-900 hover:shadow-sm transition-all focus:outline-none focus:ring-1 focus:ring-gray-300">
          <Icon name="mdi:magnify" class="h-5 w-5" />
        </button>
      </div>
      
      <!-- Navigation Menu (key: kullanıcı değişiminde menüyü yeniden oluştur) -->
      <nav :key="authStore.user?.id || 'guest'" class="flex-1 overflow-y-auto px-3 py-0 space-y-[2px] scrollbar-hide relative overflow-x-hidden">
        <!-- Dinamik Navigation Items -->
        <template v-for="item in visibleMenus" :key="item.title">
          
          <!-- Standalone Menu Item -->
          <v-tooltip :disabled="isSidebarOpen" location="right">
            <template v-slot:activator="{ props }">
              <NuxtLink 
                v-if="!item.children && item.to" 
                :to="item.to" 
                v-bind="props"
                class="group relative flex items-center py-2 text-[14px] font-medium transition-all" 
                :class="[
                  $route.path === item.to ? 'bg-white shadow-[0_1px_3px_rgba(0,0,0,0.06)] border border-gray-200/60 text-gray-900' : 'text-gray-500 hover:text-gray-900 hover:bg-white/40 border border-transparent hover:translate-x-1',
                  isSidebarOpen ? 'px-3 rounded-lg' : 'justify-center rounded-lg mx-1'
                ]"
              >
                <!-- Active Indicator Line -->
                <div v-if="$route.path === item.to" class="absolute left-0 top-1/2 -translate-y-1/2 w-[3px] h-[60%] bg-blue-500 rounded-r-[2px]"></div>
                
                <Icon :name="item.icon" class="h-[20px] w-[20px] transition-colors shrink-0" :class="[$route.path === item.to ? 'text-gray-800' : 'text-gray-400 group-hover:text-gray-600', isSidebarOpen ? 'mr-3' : 'm-0']" />
                <span class="whitespace-nowrap transition-opacity duration-300" :class="{ 'opacity-0 hidden': !isSidebarOpen }">{{ item.title }}</span>
              </NuxtLink>
            </template>
            <span>{{ item.title }}</span>
          </v-tooltip>

          <!-- Group Menu (children) -->
          <div v-if="item.children" class="mt-4 mb-2">
            <h3 
              class="text-[10px] font-bold text-gray-400 uppercase tracking-widest mb-1 mt-4 transition-opacity"
              :class="isSidebarOpen ? 'px-4' : 'text-center opacity-0 h-0 overflow-hidden m-0'"
            >
              {{ item.title }}
            </h3>
            
            <div class="w-6 h-[1px] bg-gray-200 mx-auto my-3" v-if="!isSidebarOpen"></div>

            <div class="space-y-[2px]">
              <template v-for="child in item.children" :key="child.title">
                <v-tooltip :disabled="isSidebarOpen" location="right">
                  <template v-slot:activator="{ props }">
                    <NuxtLink 
                      :to="child.to" 
                      v-bind="props"
                      class="group relative flex items-center py-2 text-[14px] font-medium transition-all" 
                      :class="[
                        $route.path.startsWith(child.to) ? 'bg-white shadow-[0_1px_3px_rgba(0,0,0,0.06)] border border-gray-200/60 text-gray-900' : 'text-gray-500 hover:text-gray-900 hover:bg-white/40 border border-transparent hover:translate-x-1',
                        isSidebarOpen ? 'px-3 rounded-lg' : 'justify-center rounded-lg mx-1'
                      ]"
                    >
                      <!-- Active Indicator Line -->
                      <div v-if="$route.path.startsWith(child.to)" class="absolute left-0 top-1/2 -translate-y-1/2 w-[3px] h-[60%] bg-blue-500 rounded-r-[2px]"></div>
                      
                      <Icon :name="child.icon" class="h-[20px] w-[20px] transition-colors shrink-0" :class="[$route.path.startsWith(child.to) ? 'text-gray-800' : 'text-gray-400 group-hover:text-gray-600', isSidebarOpen ? 'mr-3' : 'm-0']" />
                      <span class="whitespace-nowrap transition-opacity duration-300" :class="{ 'opacity-0 hidden': !isSidebarOpen }">{{ child.title }}</span>
                    </NuxtLink>
                  </template>
                  <span>{{ child.title }}</span>
                </v-tooltip>
              </template>
            </div>
          </div>
        </template>
      </nav>
      
      <!-- Sidebar Footer (User Profile) -->
      <div class="mt-auto border-t border-gray-200/60 p-3 transition-all duration-300">
        <div class="relative w-full">
          <!-- Dropdown Menu -->
          <div
            v-if="showUserMenu && isSidebarOpen"
            class="absolute bottom-full left-0 mb-2 w-full bg-white rounded-xl shadow-[0_4px_12px_rgba(0,0,0,0.05)] border border-gray-100 z-50 overflow-hidden"
          >
            <div class="py-1">
              <button @click="handleProfileClick" class="flex items-center w-full px-4 py-2 text-[13px] font-medium text-gray-700 hover:bg-gray-50 transition-colors">
                <Icon name="mdi:account-circle" class="mr-3 h-4 w-4 text-gray-400" />
                Profil
              </button>
              <button @click="handleSettingsClick" class="flex items-center w-full px-4 py-2 text-[13px] font-medium text-gray-700 hover:bg-gray-50 transition-colors">
                <Icon name="mdi:cog" class="mr-3 h-4 w-4 text-gray-400" />
                Ayarlar
              </button>
            </div>
          </div>

          <div 
            class="flex items-center w-full rounded-xl hover:bg-gray-100/50 p-2 transition-colors cursor-pointer"
            :class="!isSidebarOpen ? 'justify-center p-0 mt-1 hover:bg-transparent' : 'justify-between'"
            @click="isSidebarOpen ? (showUserMenu = !showUserMenu) : toggleSidebar()"
          >
            <v-tooltip :disabled="isSidebarOpen" location="right">
              <template v-slot:activator="{ props }">
                <div class="flex items-center overflow-hidden" v-bind="props">
                  <div class="w-9 h-9 shrink-0 bg-gray-100 border border-gray-200 overflow-hidden rounded-full flex items-center justify-center">
                    <img v-if="userInfo.avatar" :src="userInfo.avatar" class="w-full h-full object-cover">
                    <Icon v-else name="mdi:account" class="w-5 h-5 text-gray-400" />
                  </div>
                  <div class="ml-3 flex flex-col min-w-0 transition-opacity duration-300" :class="{ 'opacity-0 hidden': !isSidebarOpen }">
                    <span class="text-[13px] font-bold text-slate-800 truncate">{{ userInfo.name || 'Kullanıcı' }}</span>
                    <span class="text-[11px] text-gray-500 truncate">{{ userInfo.email || '' }}</span>
                  </div>
                </div>
              </template>
              <span>{{ userInfo.name || 'Kullanıcı' }}</span>
            </v-tooltip>

            <button v-if="isSidebarOpen" @click.stop="logout" class="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors shrink-0">
              <Icon name="mdi:logout-variant" class="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main content -->
    <div class="flex flex-col min-h-screen transition-all duration-300" :class="[isSidebarOpen && !isMobile ? 'pl-[260px]' : (isMobile ? 'pl-0' : 'pl-[72px]')]">
      <!-- Mobile Top navbar -->
      <header 
        v-if="isMobile"
        class="sticky top-0 right-0 left-0 bg-white h-16 z-10 transition-all duration-300 border-b border-gray-200" 
      >
        <div class="flex items-center justify-between h-full px-6">
          <div class="flex items-center">
            <!-- Hamburger Menu (Mobile Only) -->
            <button @click="toggleSidebar" class="p-2 -ml-2 text-gray-600 hover:text-gray-900 hover:bg-gray-200/50 rounded-lg transition-colors focus:outline-none">
              <Icon name="mdi:menu" class="h-6 w-6" />
            </button>
          </div>
        </div>
      </header>

      <!-- Page content -->
      <main class="flex-1 p-4 lg:p-6 lg:pt-6">
        <slot />
      </main>
    </div>

    <AppToast />
  </div>
</template>

<script setup>
import { storeToRefs } from 'pinia'
import { navigationItems, filterNavigationByPermissions } from '~/composables/useNavigation'
import { useAuth } from '~/composables/useAuth'
import { useAppData } from '~/composables/useAppData'
import { useAuthStore } from '~/stores/auth'
import AppToast from '~/components/UI/AppToast.vue'

import { useDisplay } from 'vuetify'

const display = useDisplay()
const isMobile = computed(() => display.smAndDown.value)

const isSidebarOpen = ref(true)
const showUserMenu = ref(false)
const authStore = useAuthStore()
const { permissions, roles } = storeToRefs(authStore)
const authUtils = useAuth()
const router = useRouter()

// App data
const { loadAppData, appData } = useAppData()

// Kullanıcı bilgileri - auth store'dan al
const userInfo = computed(() => ({
  name: authStore.userFullName || 'Kullanıcı',
  email: authStore.user?.email || ''
}))

// Yetkiye göre menüleri filtrele (storeToRefs ile explicit reactivity - kullanıcı değişiminde güncellenir)
const visibleMenus = computed(() => {
  return filterNavigationByPermissions(
    navigationItems,
    (p) => permissions.value.includes(p),
    (r) => roles.value.includes(r)
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
  // Restore sidebar state from localStorage or close on mobile
  if (isMobile.value) {
    isSidebarOpen.value = false
  } else {
    const storedState = localStorage.getItem('sidebarOpen')
    if (storedState !== null) {
      isSidebarOpen.value = storedState === 'true'
    }
  }

  // Load app data
  await loadAppData()
  
  document.addEventListener('click', (e) => {
    if (!e.target.closest('.relative')) {
      showUserMenu.value = false
    }
  })
})

watch(isSidebarOpen, (newVal) => {
  localStorage.setItem('sidebarOpen', newVal.toString())
})
</script>


<style scoped>
/* Scrollbar utility classes */
.scrollbar-hide::-webkit-scrollbar {
    display: none;
}
.scrollbar-hide {
    -ms-overflow-style: none;
    scrollbar-width: none;
}
</style>
