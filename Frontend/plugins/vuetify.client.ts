import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { mdi } from 'vuetify/iconsets/mdi'

export default defineNuxtPlugin(async (nuxtApp) => {
  // Load app data for theme colors
  let themeColors = {
    light: {
      colors: {
        primary: '#1976D2',
        secondary: '#424242',
        accent: '#82B1FF',
        error: '#FF5252',
        info: '#2196F3',
        success: '#4CAF50',
        warning: '#FB8C00',
        background: '#FAFAFA',
        surface: '#FFFFFF'
      }
    },
    dark: {
      colors: {
        primary: '#2196F3',
        secondary: '#616161',
        accent: '#FF4081',
        error: '#FF5252',
        info: '#2196F3',
        success: '#4CAF50',
        warning: '#FB8C00',
        background: '#121212',
        surface: '#1E1E1E'
      }
    }
  }

  // Try to load theme colors from data.json
  try {
    const response = await fetch('/data.json')
    if (response.ok) {
      const appData = await response.json()
      const colors = appData.theme?.colors
      if (colors) {
        themeColors = {
          light: {
            colors: {
              primary: colors.primary?.main || '#1976D2',
              secondary: colors.secondary?.main || '#424242',
              accent: colors.accent?.main || '#82B1FF',
              error: colors.error || '#FF5252',
              info: colors.info || '#2196F3',
              success: colors.success || '#4CAF50',
              warning: colors.warning || '#FB8C00',
              background: colors.background?.light || '#FAFAFA',
              surface: colors.surface?.light || '#FFFFFF'
            }
          },
          dark: {
            colors: {
              primary: colors.primary?.dark || '#2196F3',
              secondary: colors.secondary?.dark || '#616161',
              accent: colors.accent?.dark || '#FF4081',
              error: colors.error || '#FF5252',
              info: colors.info || '#2196F3',
              success: colors.success || '#4CAF50',
              warning: colors.warning || '#FB8C00',
              background: colors.background?.dark || '#121212',
              surface: colors.surface?.dark || '#1E1E1E'
            }
          }
        }
      }
    }
  } catch (error) {
    console.warn('Could not load theme colors from data.json, using defaults:', error)
  }

  const vuetify = createVuetify({
    components,
    directives,
    theme: {
      defaultTheme: 'light',
      themes: themeColors
    },
    icons: {
      defaultSet: 'mdi',
      sets: {
        mdi
      }
    },
    defaults: {
      VBtn: {
        style: 'text-transform: none;',
        variant: 'flat'
      },
      VCard: {
        elevation: 2
      },
      VTextField: {
        variant: 'outlined',
        density: 'comfortable'
      },
      VSelect: {
        variant: 'outlined',
        density: 'comfortable'
      },
      VTextarea: {
        variant: 'outlined',
        density: 'comfortable'
      }
    }
  })

  nuxtApp.vueApp.use(vuetify)
}) 