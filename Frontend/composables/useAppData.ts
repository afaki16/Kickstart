import { ref, computed, readonly, watch } from 'vue'

function hexToRgb(hex: string): string {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
  if (!result) return '0, 0, 0'
  return `${parseInt(result[1], 16)}, ${parseInt(result[2], 16)}, ${parseInt(result[3], 16)}`
}

function parseGradientColors(gradient: string): { dark: string; main: string; light: string } {
  const hexMatches = gradient.match(/#[a-fA-F0-9]{6}/g)
  if (!hexMatches || hexMatches.length < 2) {
    return { dark: '#4338ca', main: '#2563eb', light: '#3b82f6' }
  }
  if (hexMatches.length === 2) {
    return { dark: hexMatches[0], main: hexMatches[1], light: hexMatches[1] }
  }
  return { dark: hexMatches[0], main: hexMatches[1], light: hexMatches[2] }
}

interface AppData {
  tenant?: {
    layout?: string
    loginPage?: string
    registerPage?: string
  }
  app: {
    name: string
    version: string
    description: string
    logo: {
      src: string
      alt: string
      width: string
      height: string
    }
    brand: {
      text: string
      tagline: string
    }
  }
  theme: {
    colors: {
      primary: string
      accent?: string
      success: string
      warning: string
      error: string
      info: string
    }
  }
  login: {
    backgroundImages: string[]
    rotationInterval: number
    overlay: {
      opacity: number
      color: string
    }
    card: {
      background: string
      backdropFilter: string
      borderRadius: string
      border: string
      shadow: string
    }
    texts: {
      welcome: string
      subtitle: string
      emailLabel: string
      passwordLabel: string
      rememberMe: string
      forgotPassword: string
      signIn: string
      divider: string
      noAccount: string
      createAccount: string
    }
  }
  register: {
    backgroundImages: string[]
    rotationInterval: number
    overlay: {
      opacity: number
      color: string
    }
    card: {
      background: string
      backdropFilter: string
      borderRadius: string
      border: string
      shadow: string
    }
    texts: {
      welcome: string
      subtitle: string
      firstNameLabel: string
      lastNameLabel: string
      emailLabel: string
      phoneLabel: string
      passwordLabel: string
      confirmPasswordLabel: string
      passwordStrength: string
      agreeTo: string
      terms: string
      and: string
      privacy: string
      createAccount: string
      divider: string
      alreadyHaveAccount: string
      signIn: string
    }
  }
  navigation: {
    sidebar: {
      width: string
      background: string
      shadow: string
      borderColor: string
    }
    navbar: {
      height: string
      background: string
      shadow: string
    }
  }
  ui: {
    borderRadius: {
      small: string
      medium: string
      large: string
    }
    shadows: {
      small: string
      medium: string
      large: string
    }
    transitions: {
      fast: string
      medium: string
      slow: string
    }
  }
}

const appData = ref<AppData | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

export const useAppData = () => {

  /**
   * App data yükler (public/data.json).
   */
  const loadAppData = async (forceReload = false) => {
    if (appData.value && !forceReload) {
      return appData.value
    }

    isLoading.value = true
    error.value = null

    try {
      const response = await fetch('/data.json')
      if (!response.ok) {
        throw new Error('Failed to load app data')
      }

      const data = (await response.json()) as AppData
      appData.value = data
      return appData.value
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error'
      console.error('Error loading app data:', err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  const injectThemeCSSVariables = () => {
    if (typeof document === 'undefined' || !appData.value) return

    const colors = appData.value.theme?.colors
    if (!colors) return

    const root = document.documentElement
    const p = parseGradientColors(colors.primary)

    root.style.setProperty('--theme-primary', p.main)
    root.style.setProperty('--theme-primary-light', p.light)
    root.style.setProperty('--theme-primary-dark', p.dark)
    root.style.setProperty('--theme-primary-rgb', hexToRgb(p.main))
    root.style.setProperty('--theme-primary-dark-rgb', hexToRgb(p.dark))

    root.style.setProperty('--theme-secondary', p.dark)
    root.style.setProperty('--theme-secondary-light', p.main)
    root.style.setProperty('--theme-secondary-dark', p.dark)
    root.style.setProperty('--theme-secondary-rgb', hexToRgb(p.dark))

    root.style.setProperty('--theme-gradient', `linear-gradient(135deg, ${p.dark} 0%, ${p.main} 100%)`)
    root.style.setProperty('--theme-gradient-hover', `linear-gradient(135deg, ${p.dark} 0%, ${p.light} 100%)`)
    root.style.setProperty('--theme-gradient-sidebar', colors.primary)
    root.style.setProperty('--theme-gradient-text', `linear-gradient(135deg, #1f2937 0%, ${p.dark} 100%)`)

    if (colors.accent) {
      root.style.setProperty('--theme-accent', colors.accent)
    }
    root.style.setProperty('--theme-success', colors.success)
    root.style.setProperty('--theme-warning', colors.warning)
    root.style.setProperty('--theme-error', colors.error)
    root.style.setProperty('--theme-info', colors.info)
  }

  const getAppInfo = computed(() => appData.value?.app || null)
  const getTheme = computed(() => appData.value?.theme || null)
  const getLoginConfig = computed(() => appData.value?.login || null)
  const getRegisterConfig = computed(() => appData.value?.register || null)
  const getNavigationConfig = computed(() => appData.value?.navigation || null)
  const getUIConfig = computed(() => appData.value?.ui || null)

  const getLogo = computed(() => appData.value?.app.logo || null)
  const getBrandText = computed(() => appData.value?.app.brand.text || 'Kickstart')
  const getBackgroundImages = computed(() => appData.value?.login.backgroundImages || appData.value?.register.backgroundImages || [])
  const getThemeColors = computed(() => appData.value?.theme.colors || null)

  watch(appData, () => {
    injectThemeCSSVariables()
  }, { immediate: true })

  return {
    appData: readonly(appData),
    isLoading: readonly(isLoading),
    error: readonly(error),
    loadAppData,
    getAppInfo,
    getTheme,
    getLoginConfig,
    getRegisterConfig,
    getNavigationConfig,
    getUIConfig,
    getLogo,
    getBrandText,
    getBackgroundImages,
    getThemeColors
  }
}
