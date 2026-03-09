import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { mdi } from 'vuetify/iconsets/mdi'

function extractColorsFromGradient(gradient: string): { dark: string; main: string; light: string } {
  const hexMatches = gradient.match(/#[a-fA-F0-9]{6}/g)
  if (!hexMatches || hexMatches.length < 2) {
    return { dark: '#4338ca', main: '#2563eb', light: '#3b82f6' }
  }
  if (hexMatches.length === 2) {
    return { dark: hexMatches[0], main: hexMatches[1], light: hexMatches[1] }
  }
  return { dark: hexMatches[0], main: hexMatches[1], light: hexMatches[2] }
}

export default defineNuxtPlugin(async (nuxtApp) => {
  let themeColors = {
    light: {
      colors: {
        primary: '#2563eb',
        secondary: '#4338ca',
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
        primary: '#3b82f6',
        secondary: '#4338ca',
        accent: '#82B1FF',
        error: '#FF5252',
        info: '#2196F3',
        success: '#4CAF50',
        warning: '#FB8C00',
        background: '#121212',
        surface: '#1E1E1E'
      }
    }
  }

  try {
    const response = await fetch('/data.json')
    if (response.ok) {
      const appData = await response.json()
      const colors = appData.theme?.colors
      if (colors) {
        const p = typeof colors.primary === 'string' && colors.primary.includes('gradient')
          ? extractColorsFromGradient(colors.primary)
          : { dark: '#4338ca', main: '#2563eb', light: '#3b82f6' }

        themeColors = {
          light: {
            colors: {
              primary: p.main,
              secondary: p.dark,
              accent: colors.accent || '#82B1FF',
              error: colors.error || '#FF5252',
              info: colors.info || '#2196F3',
              success: colors.success || '#4CAF50',
              warning: colors.warning || '#FB8C00',
              background: '#FAFAFA',
              surface: '#FFFFFF'
            }
          },
          dark: {
            colors: {
              primary: p.light,
              secondary: p.dark,
              accent: colors.accent || '#82B1FF',
              error: colors.error || '#FF5252',
              info: colors.info || '#2196F3',
              success: colors.success || '#4CAF50',
              warning: colors.warning || '#FB8C00',
              background: '#121212',
              surface: '#1E1E1E'
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