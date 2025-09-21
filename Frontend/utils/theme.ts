export const useTheme = () => {
  const appStore = useAppStore()
  
  const toggleTheme = () => {
    appStore.toggleTheme()
    
    // Apply theme to Vuetify
    if (process.client) {
      const vuetify = useNuxtApp().$vuetify
      if (vuetify?.theme) {
        vuetify.theme.global.name.value = appStore.theme
      }
    }
  }
  
  const setTheme = (theme: 'light' | 'dark') => {
    appStore.setTheme(theme)
    
    // Apply theme to Vuetify
    if (process.client) {
      const vuetify = useNuxtApp().$vuetify
      if (vuetify?.theme) {
        vuetify.theme.global.name.value = theme
      }
    }
  }
  
  const initTheme = () => {
    if (process.client) {
      const vuetify = useNuxtApp().$vuetify
      if (vuetify?.theme) {
        vuetify.theme.global.name.value = appStore.theme
      }
    }
  }
  
  return {
    toggleTheme,
    setTheme,
    initTheme
  }
} 