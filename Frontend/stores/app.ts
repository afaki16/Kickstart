import { defineStore } from 'pinia'

interface AppState {
  theme: 'light' | 'dark'
  sidebarCollapsed: boolean
  loading: boolean
  breadcrumbs: { title: string; to?: string }[]
  pageTitle: string
}

export const useAppStore = defineStore('app', {
  state: (): AppState => ({
    theme: 'light',
    sidebarCollapsed: false,
    loading: false,
    breadcrumbs: [],
    pageTitle: ''
  }),

  getters: {
    isDark: (state) => state.theme === 'dark'
  },

  actions: {
    toggleTheme() {
      this.theme = this.theme === 'light' ? 'dark' : 'light'
      if (process.client) {
        localStorage.setItem('theme', this.theme)
      }
    },

    setTheme(theme: 'light' | 'dark') {
      this.theme = theme
      if (process.client) {
        localStorage.setItem('theme', theme)
      }
    },

    toggleSidebar() {
      this.sidebarCollapsed = !this.sidebarCollapsed
      if (process.client) {
        localStorage.setItem('sidebarCollapsed', this.sidebarCollapsed.toString())
      }
    },

    setSidebarCollapsed(collapsed: boolean) {
      this.sidebarCollapsed = collapsed
      if (process.client) {
        localStorage.setItem('sidebarCollapsed', collapsed.toString())
      }
    },

    setLoading(loading: boolean) {
      this.loading = loading
    },

    setBreadcrumbs(breadcrumbs: { title: string; to?: string }[]) {
      this.breadcrumbs = breadcrumbs
    },

    setPageTitle(title: string) {
      this.pageTitle = title
      if (process.client) {
        document.title = title ? `${title} - {{PROJECT_NAME}}` : '{{PROJECT_NAME}}'
      }
    },

    initializeApp() {
      if (!process.client) return

      // Initialize theme
      const savedTheme = localStorage.getItem('theme')
      if (savedTheme === 'dark' || savedTheme === 'light') {
        this.theme = savedTheme
      }

      // Initialize sidebar state
      const savedSidebar = localStorage.getItem('sidebarCollapsed')
      if (savedSidebar) {
        this.sidebarCollapsed = savedSidebar === 'true'
      }
    }
  }
}) 