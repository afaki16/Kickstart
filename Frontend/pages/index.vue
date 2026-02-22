<template>
  <!-- Splash Screen -->
  <Transition name="splash-fade">
    <div v-if="showSplash" class="splash-screen">
      <div class="splash-bg-glow"></div>
      <div class="splash-rings">
        <div class="splash-ring ring-1"></div>
        <div class="splash-ring ring-2"></div>
        <div class="splash-ring ring-3"></div>
      </div>
      <div class="splash-content" :class="{ 'splash-animate': splashReady }">
        <div class="splash-logo-wrapper">
          <img
            :src="appData?.app?.logo?.src || '/images/logo.svg'"
            :alt="appData?.app?.logo?.alt || 'Logo'"
            class="splash-logo-img"
          />
        </div>
        <p class="splash-tagline">{{ appData?.app?.brand?.tagline || 'Secure Authentication System' }}</p>
        <div class="splash-loader">
          <div class="splash-loader-bar"></div>
        </div>
      </div>
      <div class="splash-particles-container">
        <div v-for="i in 30" :key="'sp'+i" class="splash-particle" :style="getSplashParticleStyle(i)"></div>
      </div>
    </div>
  </Transition>

  <div class="login-container" :style="{ background: themeGradients?.login || 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }">
    <!-- 3D Background with rotating images -->
    <div class="background-container">
      <div 
        v-for="(image, index) in backgroundImages" 
        :key="index"
        :class="['background-image', { active: currentImageIndex === index }]"
        :style="{ backgroundImage: `url(${image})` }"
      />
      <div 
        class="background-overlay"
        :style="{ background: loginConfig?.overlay?.color || 'rgba(0, 0, 0, 0.4)' }"
      ></div>
    </div>

    <!-- 3D Login Card -->
    <div class="login-card-container" :class="{ 'card-enter-active': !showSplash }">
      <div 
        class="login-card"
        :style="{
          background: loginConfig?.card?.background || 'rgba(255, 255, 255, 0.1)',
          backdropFilter: loginConfig?.card?.backdropFilter || 'blur(20px)',
          borderRadius: loginConfig?.card?.borderRadius || '20px',
          border: loginConfig?.card?.border || '1px solid rgba(255, 255, 255, 0.2)',
          boxShadow: loginConfig?.card?.shadow || '0 25px 45px rgba(0, 0, 0, 0.2)'
        }"
      >
        <div class="card-content">
          <div class="logo-section">
            <div class="logo-icon">
              <v-icon size="64" color="white">mdi-shield-account</v-icon>
            </div>
            <h1 class="welcome-text">{{ loginConfig?.texts?.welcome }}</h1>
            <p class="subtitle-text">{{ loginConfig?.texts?.subtitle }}</p>
          </div>

          <!-- Expired session alert -->
          <v-alert
            v-if="route.query.expired"
            type="warning"
            variant="tonal"
            class="mb-4"
            closable
          >
            Your session has expired. Please sign in again.
          </v-alert>

          <v-form ref="loginForm" v-model="isFormValid" @submit.prevent="handleLogin">
                        <div class="input-group">
              <v-text-field
                v-model="form.email"
                :label="loginConfig?.texts?.emailLabel || 'Email Address'"
                type="email"
                prepend-inner-icon="mdi-email"
                :rules="[validationRules.required, validationRules.email]"
                variant="outlined"
                class="mb-4"
                autofocus
                bg-color="rgba(255, 255, 255, 0.1)"
                color="white"
                hide-details="auto"
              />
            </div>

            <div class="input-group">
              <v-text-field
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                :label="loginConfig?.texts?.passwordLabel || 'Password'"
                prepend-inner-icon="mdi-lock"
                :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                :rules="[validationRules.required]"
                variant="outlined"
                class="mb-4"
                bg-color="rgba(255, 255, 255, 0.1)"
                color="white"
                hide-details="auto"
                @click:append-inner="showPassword = !showPassword"
              />
            </div>

            <div class="options-row">
              <v-checkbox
                v-model="form.rememberMe"
                :label="loginConfig?.texts?.rememberMe || 'Remember me'"
                density="compact"
                hide-details
                color="white"
              />
              
              <v-btn
                variant="text"
                color="white"
                size="small"
                @click="$router.push('/auth/forgot-password')"
              >
                {{ loginConfig?.texts?.forgotPassword || 'Forgot password?' }}
              </v-btn>
            </div>

            <v-btn
              type="submit"
              color="white"
              size="large"
              block
              :loading="authStore.isLoading"
              :disabled="!isFormValid"
              class="login-btn mb-4"
              elevation="8"
            >
              <span class="btn-text">{{ loginConfig?.texts?.signIn || 'Sign In' }}</span>
            </v-btn>
          </v-form>

          <div class="divider-section">
            <div class="divider-line"></div>
            <span class="divider-text">{{ loginConfig?.texts?.divider || 'or' }}</span>
            <div class="divider-line"></div>
          </div>

          <div class="register-section">
            <p class="register-text">
              {{ loginConfig?.texts?.noAccount || 'Don\'t have an account?' }}
            </p>
            <v-btn
              variant="outlined"
              color="white"
              @click="$router.push('/auth/register')"
              class="register-btn"
            >
              {{ loginConfig?.texts?.createAccount || 'Create Account' }}
            </v-btn>
          </div>
        </div>
      </div>
    </div>

    <!-- Floating particles for 3D effect -->
    <div class="particles">
      <div v-for="i in 20" :key="i" class="particle" :style="getParticleStyle(i)"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { LoginRequest } from '~/types'

// Meta
definePageMeta({
  layout: false,
  middleware: 'guest'
})

// Composables
const route = useRoute()
const authStore = useAuthStore()
const auth = useAuth()
const { validationRules } = useValidators()

// Splash screen
const showSplash = ref(true)
const splashReady = ref(false)

// Reactive data
const loginForm = ref()
const isFormValid = ref(false)
const showPassword = ref(false)
const currentImageIndex = ref(0)

// App data
const { loadAppData, appData, getBackgroundImages, getLoginConfig, getThemeGradients } = useAppData()

// Background images from data.json
const backgroundImages = computed(() => getBackgroundImages.value || [])
const loginConfig = computed(() => getLoginConfig.value)
const themeGradients = computed(() => getThemeGradients.value)

const form = reactive<LoginRequest>({
  email: '',
  password: '',
  rememberMe: false,
  deviceId: '',
  deviceName: ''
})

// Methods
const handleLogin = async () => {
  if (!isFormValid.value) return

  try {
    await auth.login(form)
  } catch (error) {
    console.error('Login failed:', error)
    // Error handling is now done in useApi.ts, so we don't need to show additional messages here
  }
}

const getParticleStyle = (index: number) => {
  const delay = index * 0.1
  const size = Math.random() * 4 + 2
  const x = Math.random() * 100
  const y = Math.random() * 100
  const duration = Math.random() * 3 + 2
  
  return {
    left: `${x}%`,
    top: `${y}%`,
    width: `${size}px`,
    height: `${size}px`,
    animationDelay: `${delay}s`,
    animationDuration: `${duration}s`
  }
}

const getSplashParticleStyle = (index: number) => {
  const angle = (index / 30) * Math.PI * 2
  const radius = 120 + Math.random() * 200
  const size = Math.random() * 6 + 2
  const delay = Math.random() * 1.5
  const duration = 1.5 + Math.random() * 1.5
  const tx = Math.cos(angle) * radius
  const ty = Math.sin(angle) * radius
  return {
    '--tx': `${tx}px`,
    '--ty': `${ty}px`,
    width: `${size}px`,
    height: `${size}px`,
    animationDelay: `${delay}s`,
    animationDuration: `${duration}s`
  } as Record<string, string>
}

// Background image rotation
let imageInterval: NodeJS.Timeout

// Lifecycle
onMounted(async () => {
  // Load app data
  await loadAppData()

  // Trigger splash animation after a tiny delay for DOM paint
  nextTick(() => { splashReady.value = true })
  setTimeout(() => { showSplash.value = false }, 13000)

  // Pre-fill email from registration if available
  const registeredEmail = route.query.email as string
  if (registeredEmail) {
    form.email = registeredEmail
  }

  // Auto-generate device information
  if (process.client) {
    // Device ID oluştur
    let deviceId = localStorage.getItem('deviceId')
    if (!deviceId) {
      deviceId = 'device_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
      localStorage.setItem('deviceId', deviceId)
    }
    form.deviceId = deviceId

    // Device name tespit et
    const userAgent = navigator.userAgent
    if (userAgent.includes('Windows')) {
      form.deviceName = 'Windows Device'
    } else if (userAgent.includes('Mac')) {
      form.deviceName = 'Mac Device'
    } else if (userAgent.includes('Linux')) {
      form.deviceName = 'Linux Device'
    } else if (userAgent.includes('Android')) {
      form.deviceName = 'Android Device'
    } else if (userAgent.includes('iOS')) {
      form.deviceName = 'iOS Device'
    } else {
      form.deviceName = 'Unknown Device'
    }
  }

  // Start background image rotation
  imageInterval = setInterval(() => {
    currentImageIndex.value = (currentImageIndex.value + 1) % backgroundImages.value.length
  }, loginConfig.value?.rotationInterval || 3000)
})

onUnmounted(() => {
  if (imageInterval) {
    clearInterval(imageInterval)
  }
})

// SEO
useHead({
  title: '{{PROJECT_NAME}} - Authentication System',
  meta: [
    { name: 'description', content: 'Sign in to your {{PROJECT_NAME}} account' }
  ]
})
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  position: relative;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.background-container {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1;
}

.background-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  opacity: 0;
  transition: opacity 1s ease-in-out;
}

.background-image.active {
  opacity: 1;
}

.background-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  z-index: 2;
}

.login-card-container {
  position: relative;
  z-index: 10;
  perspective: 1000px;
  opacity: 0;
  transform: translateY(40px) scale(0.95);
}

.login-card {
  padding: 40px;
  width: 400px;
}

.card-content {
  text-align: center;
}

.logo-section {
  margin-bottom: 30px;
}

.logo-icon {
  margin-bottom: 20px;
  animation: float 3s ease-in-out infinite;
}

.welcome-text {
  color: white;
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 10px;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.subtitle-text {
  color: rgba(255, 255, 255, 0.8);
  font-size: 1.1rem;
  margin-bottom: 0;
}

.input-group {
  margin-bottom: 20px;
}

.input-group :deep(.v-field) {
  background: rgba(255, 255, 255, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.2) !important;
  border-radius: 12px !important;
}

.input-group :deep(.v-field__input) {
  color: white !important;
}

.input-group :deep(.v-field__label) {
  color: rgba(255, 255, 255, 0.8) !important;
}

.input-group :deep(.v-icon) {
  color: rgba(255, 255, 255, 0.8) !important;
}

.options-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.options-row :deep(.v-checkbox) {
  color: white;
}

.login-btn {
  border-radius: 12px !important;
  height: 50px !important;
  font-weight: 600 !important;
  text-transform: none !important;
  transition: all 0.3s ease !important;
}

.login-btn:hover {
  transform: translateY(-2px) !important;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3) !important;
}

.btn-text {
  font-size: 1.1rem;
}

.divider-section {
  display: flex;
  align-items: center;
  margin: 30px 0;
}

.divider-line {
  flex: 1;
  height: 1px;
  background: rgba(255, 255, 255, 0.3);
}

.divider-text {
  color: rgba(255, 255, 255, 0.6);
  margin: 0 15px;
  font-size: 0.9rem;
}

.register-section {
  text-align: center;
}

.register-text {
  color: rgba(255, 255, 255, 0.8);
  margin-bottom: 15px;
}

.register-btn {
  border: 2px solid rgba(255, 255, 255, 0.3) !important;
  border-radius: 12px !important;
  color: white !important;
  font-weight: 600 !important;
  text-transform: none !important;
  transition: all 0.3s ease !important;
}

.register-btn:hover {
  background: rgba(255, 255, 255, 0.1) !important;
  border-color: white !important;
  transform: translateY(-2px) !important;
}

.particles {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 5;
  pointer-events: none;
}

.particle {
  position: absolute;
  background: rgba(255, 255, 255, 0.6);
  border-radius: 50%;
  animation: float-particle 3s ease-in-out infinite;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px);
  }
  50% {
    transform: translateY(-10px);
  }
}

@keyframes float-particle {
  0%, 100% {
    transform: translateY(0px) rotate(0deg);
    opacity: 0.6;
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
    opacity: 1;
  }
}

/* Responsive design */
@media (max-width: 768px) {
  .login-card {
    width: 90%;
    max-width: 350px;
    padding: 30px 20px;
  }
  
  .welcome-text {
    font-size: 2rem;
  }
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
}

::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}

/* ===== Splash Screen ===== */
.splash-screen {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0f0c29 0%, #302b63 50%, #24243e 100%);
  overflow: hidden;
}

.splash-bg-glow {
  position: absolute;
  width: 500px;
  height: 500px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(102, 126, 234, 0.35) 0%, transparent 70%);
  animation: splash-glow-pulse 2s ease-in-out infinite alternate;
}

@keyframes splash-glow-pulse {
  0% { transform: scale(0.8); opacity: 0.4; }
  100% { transform: scale(1.3); opacity: 0.8; }
}

.splash-rings {
  position: absolute;
  display: flex;
  align-items: center;
  justify-content: center;
}

.splash-ring {
  position: absolute;
  border-radius: 50%;
  border: 2px solid rgba(102, 126, 234, 0.3);
  animation: splash-ring-expand 2.5s ease-out infinite;
}

.ring-1 { width: 120px; height: 120px; animation-delay: 0s; }
.ring-2 { width: 120px; height: 120px; animation-delay: 0.6s; }
.ring-3 { width: 120px; height: 120px; animation-delay: 1.2s; }

@keyframes splash-ring-expand {
  0% { transform: scale(1); opacity: 0.6; border-color: rgba(102, 126, 234, 0.5); }
  100% { transform: scale(4); opacity: 0; border-color: rgba(118, 75, 162, 0.1); }
}

.splash-content {
  position: relative;
  z-index: 10;
  display: flex;
  flex-direction: column;
  align-items: center;
  opacity: 0;
  transform: scale(0.6);
  transition: all 0.7s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.splash-content.splash-animate {
  opacity: 1;
  transform: scale(1);
}

.splash-logo-wrapper {
  position: relative;
  width: 280px;
  height: 280px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 40px;
}

.splash-logo-img {
  position: relative;
  z-index: 2;
  width: 250px;
  height: 250px;
  object-fit: contain;
  filter: drop-shadow(0 0 30px rgba(102, 126, 234, 0.6));
  animation: splash-logo-float 2s ease-in-out infinite alternate;
}

@keyframes splash-logo-float {
  0% { transform: translateY(4px) scale(1); }
  100% { transform: translateY(-4px) scale(1.05); }
}

.splash-tagline {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.55);
  letter-spacing: 2px;
  text-transform: uppercase;
  margin-bottom: 32px;
  animation: splash-tagline-in 1s ease-out 0.5s both;
}

@keyframes splash-tagline-in {
  0% { opacity: 0; transform: translateY(10px); letter-spacing: 8px; }
  100% { opacity: 1; transform: translateY(0); letter-spacing: 2px; }
}

.splash-loader {
  width: 160px;
  height: 3px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
  overflow: hidden;
}

.splash-loader-bar {
  height: 100%;
  border-radius: 3px;
  background: linear-gradient(90deg, #667eea, #764ba2);
  animation: splash-load 12s ease-in-out forwards;
}

@keyframes splash-load {
  0% { width: 0; }
  30% { width: 40%; }
  70% { width: 75%; }
  100% { width: 100%; }
}

.splash-particles-container {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.splash-particle {
  position: absolute;
  border-radius: 50%;
  background: rgba(102, 126, 234, 0.7);
  animation: splash-particle-burst var(--duration, 2s) ease-out infinite;
  animation-delay: var(--delay, 0s);
}

@keyframes splash-particle-burst {
  0% {
    transform: translate(0, 0) scale(1);
    opacity: 0.8;
  }
  100% {
    transform: translate(var(--tx), var(--ty)) scale(0);
    opacity: 0;
  }
}

.splash-fade-leave-active {
  transition: all 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

.splash-fade-leave-to {
  opacity: 0;
  transform: scale(1.08);
}

.login-card-container.card-enter-active {
  animation: card-slide-in 0.8s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
}

@keyframes card-slide-in {
  0% { opacity: 0; transform: translateY(40px) scale(0.95); }
  100% { opacity: 1; transform: translateY(0) scale(1); }
}
</style> 