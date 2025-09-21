import { ref, computed, readonly } from 'vue'

interface AppData {
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
      primary: {
        main: string
        light: string
        dark: string
        gradient: string
      }
      secondary: {
        main: string
        light: string
        dark: string
      }
      accent: {
        main: string
        light: string
        dark: string
      }
      success: string
      warning: string
      error: string
      info: string
      background: {
        light: string
        dark: string
      }
      surface: {
        light: string
        dark: string
      }
      text: {
        primary: string
        secondary: string
        disabled: string
      }
    }
    gradients: {
      navbar: string
      login: string
      button: string
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

export const useAppData = () => {
  const appData = ref<AppData | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const loadAppData = async () => {
    if (appData.value) return appData.value

    isLoading.value = true
    error.value = null

    try {
      const response = await fetch('/data.json')
      if (!response.ok) {
        throw new Error('Failed to load app data')
      }
      appData.value = await response.json()
      return appData.value
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error'
      console.error('Error loading app data:', err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  const getAppInfo = computed(() => appData.value?.app || null)
  const getTheme = computed(() => appData.value?.theme || null)
  const getLoginConfig = computed(() => appData.value?.login || null)
  const getRegisterConfig = computed(() => appData.value?.register || null)
  const getNavigationConfig = computed(() => appData.value?.navigation || null)
  const getUIConfig = computed(() => appData.value?.ui || null)

  const getLogo = computed(() => appData.value?.app.logo || null)
  const getBrandText = computed(() => appData.value?.app.brand.text || '{{PROJECT_NAME}}')
  const getBackgroundImages = computed(() => appData.value?.login.backgroundImages || appData.value?.register.backgroundImages || [])
  const getThemeColors = computed(() => appData.value?.theme.colors || null)
  const getThemeGradients = computed(() => appData.value?.theme.gradients || null)

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
    getThemeColors,
    getThemeGradients
  }
}
