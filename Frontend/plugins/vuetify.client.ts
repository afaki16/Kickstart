import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { mdi } from 'vuetify/iconsets/mdi'
import { useAppData } from '../composables/useAppData'

export default defineNuxtPlugin(async () => {
  const nuxtApp = useNuxtApp()
  const { loadAppData, appData } = useAppData()

  await loadAppData()

  const colors = appData.value?.theme?.colors

  const defaultPrimary = { dark: '#4338ca', main: '#2563eb', light: '#3b82f6' }
  let p = defaultPrimary

  if (typeof colors?.primary === 'string' && colors.primary.includes('gradient')) {
    const hexMatches = colors.primary.match(/#[a-fA-F0-9]{6}/g)
    if (hexMatches && hexMatches.length >= 2) {
      p = {
        dark: hexMatches[0],
        main: hexMatches[1],
        light: hexMatches[2] ?? hexMatches[1]
      }
    }
  }

  const themeColors = {
    light: {
      colors: {
        primary: p.main,
        secondary: p.dark,
        accent: colors?.accent || '#82B1FF',
        error: colors?.error || '#FF5252',
        info: colors?.info || '#2196F3',
        success: colors?.success || '#4CAF50',
        warning: colors?.warning || '#FB8C00',
        background: '#FAFAFA',
        surface: '#FFFFFF'
      }
    },
    dark: {
      colors: {
        primary: p.light,
        secondary: p.dark,
        accent: colors?.accent || '#82B1FF',
        error: colors?.error || '#FF5252',
        info: colors?.info || '#2196F3',
        success: colors?.success || '#4CAF50',
        warning: colors?.warning || '#FB8C00',
        background: '#121212',
        surface: '#1E1E1E'
      }
    }
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