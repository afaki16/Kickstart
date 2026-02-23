export default defineNuxtConfig({
  devtools: { enabled: true },
  compatibilityDate: '2025-07-26',
 
  // TypeScript configuration - geçici olarak kapatın
  typescript: {
    strict: false,  // ✅ Production için önerilen - true
    typeCheck: false  // ✅ Build sırasında kontrol -true
  },
  
  // CSS Framework
  css: [
    'vuetify/lib/styles/main.sass',
    '@mdi/font/css/materialdesignicons.min.css',
    '~/assets/scss/main.scss',
    '~/assets/css/main.css',
    '~/assets/css/global-admin.css',
    'flag-icons/css/flag-icons.min.css'
  ],
  
  build: {
    transpile: ['vuetify']
  },
  
  modules: [
    '@pinia/nuxt',
    '@vueuse/nuxt',
    '@nuxtjs/google-fonts',
    '@nuxtjs/tailwindcss',
    '@nuxt/icon'
  ],
  
  googleFonts: {
    families: {
      Inter: [300, 400, 500, 600, 700],
      'Plus+Jakarta+Sans': [600, 700, 800],
      'Material+Icons': true
    }
  },
  
  runtimeConfig: {
    public: {
      apiBase: process.env.API_BASE_URL || 'https://localhost:44333',
      appName: '{{PROJECT_NAME}}',
      appVersion: '1.0.0'
    }
  },
  
  app: {
    head: {
      title: '{{PROJECT_NAME}}',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap' }
      ]
    }
  },
  
  ssr: false,
  
  vite: {
    define: {
      'process.env.DEBUG': false
    },
    css: {
      preprocessorOptions: {
        scss: {
          additionalData: '@use "~/assets/scss/variables.scss" as *;'
        }
      }
    },
    ssr: {
      noExternal: ['vuetify']
    },
    // HMR overlay'i kapatın
    server: {
      hmr: {
        overlay: false
      }
    }
  },
  
  imports: {
    dirs: ['composables/**', 'stores/**', 'utils/**']
  },
  
  components: {
    dirs: [
      '~/components',
      '~/components/UI',
      '~/components/Auth',
      '~/components/Users',
      '~/components/Roles',
      '~/components/Dashboard'
    ]
  }
})