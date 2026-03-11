<template>
  <div class="login-container" :style="{ background: 'var(--theme-gradient-sidebar)' }">
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
      />
    </div>

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
                hide-details
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
            <div class="divider-line" />
            <span class="divider-text">{{ loginConfig?.texts?.divider || 'or' }}</span>
            <div class="divider-line" />
          </div>

          <div class="register-section">
            <p class="register-text">{{ loginConfig?.texts?.noAccount || "Don't have an account?" }}</p>
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

    <div class="particles">
      <div v-for="i in 20" :key="i" class="particle" :style="getParticleStyle(i)" />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { LoginRequest } from '~/types'

const route = useRoute()
const authStore = useAuthStore()
const { validationRules } = useValidators()
const { getBackgroundImages, getLoginConfig } = useAppData()

defineProps<{ showSplash?: boolean }>()

const loginForm = ref()
const isFormValid = ref(false)
const showPassword = ref(false)
const currentImageIndex = ref(0)

const backgroundImages = computed(() => getBackgroundImages.value || [])
const loginConfig = computed(() => getLoginConfig.value)

const form = reactive<LoginRequest>({
  email: '',
  password: '',
  rememberMe: false,
  deviceId: '',
  deviceName: ''
})

const handleLogin = async () => {
  if (!isFormValid.value) return
  try {
    await useAuth().login(form)
  } catch (error) {
    console.error('Login failed:', error)
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

let imageInterval: NodeJS.Timeout

onMounted(() => {
  const registeredEmail = route.query.email as string
  if (registeredEmail) form.email = registeredEmail

  if (import.meta.client) {
    let deviceId = localStorage.getItem('deviceId')
    if (!deviceId) {
      deviceId = 'device_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
      localStorage.setItem('deviceId', deviceId)
    }
    form.deviceId = deviceId

    const ua = navigator.userAgent
    form.deviceName = ua.includes('Windows') ? 'Windows Device'
      : ua.includes('Mac') ? 'Mac Device'
      : ua.includes('Linux') ? 'Linux Device'
      : ua.includes('Android') ? 'Android Device'
      : ua.includes('iOS') ? 'iOS Device'
      : 'Unknown Device'
  }

  imageInterval = setInterval(() => {
    currentImageIndex.value = (currentImageIndex.value + 1) % (backgroundImages.value?.length || 1)
  }, loginConfig.value?.rotationInterval || 3000)
})

onUnmounted(() => {
  if (imageInterval) clearInterval(imageInterval)
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
  inset: 0;
  z-index: 1;
}

.background-image {
  position: absolute;
  inset: 0;
  background-size: cover;
  background-position: center;
  opacity: 0;
  transition: opacity 1s ease-in-out;
}

.background-image.active {
  opacity: 1;
}

.background-overlay {
  position: absolute;
  inset: 0;
  z-index: 2;
}

.login-card-container {
  position: relative;
  z-index: 10;
  opacity: 0;
  transform: translateY(40px) scale(0.95);
}

.login-card-container.card-enter-active {
  animation: card-slide-in 0.8s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
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
}

.input-group :deep(.v-field) {
  background: rgba(255, 255, 255, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.2) !important;
  border-radius: 12px !important;
}

.input-group :deep(.v-field__input),
.input-group :deep(.v-field__label),
.input-group :deep(.v-icon) {
  color: rgba(255, 255, 255, 0.8) !important;
}

.options-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.login-btn {
  border-radius: 12px !important;
  height: 50px !important;
  font-weight: 600 !important;
  text-transform: none !important;
}

.login-btn:hover {
  transform: translateY(-2px) !important;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3) !important;
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
}

.register-btn:hover {
  background: rgba(255, 255, 255, 0.1) !important;
  border-color: white !important;
  transform: translateY(-2px) !important;
}

.particles {
  position: absolute;
  inset: 0;
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
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

@keyframes float-particle {
  0%, 100% { transform: translateY(0) rotate(0deg); opacity: 0.6; }
  50% { transform: translateY(-20px) rotate(180deg); opacity: 1; }
}

@keyframes card-slide-in {
  0% { opacity: 0; transform: translateY(40px) scale(0.95); }
  100% { opacity: 1; transform: translateY(0) scale(1); }
}

@media (max-width: 768px) {
  .login-card {
    width: 90%;
    max-width: 350px;
    padding: 30px 20px;
  }
  .welcome-text { font-size: 2rem; }
}
</style>
