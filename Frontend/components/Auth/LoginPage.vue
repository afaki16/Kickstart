
<template>
  <div class="login-page">
    <!-- LEFT: Image Carousel -->
    <div class="left-panel">
      <div
        v-for="(image, index) in backgroundImages"
        :key="index"
        :class="['carousel-slide', { active: currentImageIndex === index }]"
        :style="{ backgroundImage: `url(${image})` }"
      />
      <div class="left-overlay" />
      <div class="left-content">
        <div class="brand-badge">
          <v-icon size="16" color="white">mdi-lightning-bolt</v-icon>
          <span>{{ loginConfig?.texts?.brandBadge || 'Secure Platform' }}</span>
        </div>
        <h2 class="left-title">{{ loginConfig?.texts?.heroTitle || 'Manage everything in one place.' }}</h2>
        <p class="left-subtitle">{{ loginConfig?.texts?.heroSubtitle || 'Fast, reliable, and built for the way you work.' }}</p>
        <div class="carousel-dots">
          <div
            v-for="(_, index) in backgroundImages"
            :key="index"
            :class="['dot', { active: currentImageIndex === index }]"
            @click="goToSlide(index)"
          />
        </div>
      </div>
    </div>
 
    <!-- RIGHT: Login Form -->
    <div class="right-panel">
      <div class="right-inner">
        <div class="form-header">
          <div class="form-header-logo">
            <img
              :src="logoSrc"
              :alt="logoAlt"
              class="form-header-logo-img"
            />
          </div>
          <h1>{{ loginConfig?.texts?.welcome || 'Welcome back' }}</h1>
          <p>{{ loginConfig?.texts?.subtitle || 'Sign in to your account to continue' }}</p>
        </div>
 
        <!-- Session Expired Alert -->
        <v-alert
          v-if="route.query.expired"
          type="warning"
          variant="tonal"
          class="mb-6 expired-alert"
          closable
          density="compact"
        >
          Your session has expired. Please sign in again.
        </v-alert>

        <!-- Login API errors (from /api/auth/login) -->
        <v-alert
          v-if="loginApiErrors.length > 0"
          type="error"
          variant="tonal"
          class="mb-6 login-api-error-alert"
          closable
          density="compact"
          @click:close="loginApiErrors = []"
        >
          <template v-if="loginApiErrors.length === 1">
            {{ loginApiErrors[0] }}
          </template>
          <ul v-else class="login-api-error-list">
            <li v-for="(msg, i) in loginApiErrors" :key="i">{{ msg }}</li>
          </ul>
        </v-alert>
 
        <v-form ref="loginForm" v-model="isFormValid" @submit.prevent="handleLogin">
          <div class="input-group">
            <label>{{ loginConfig?.texts?.emailLabel || 'Email Address' }}</label>
            <v-text-field
              v-model="form.email"
              type="email"
              :placeholder="loginConfig?.texts?.emailPlaceholder || 'you@example.com'"
              prepend-inner-icon="mdi-email-outline"
              :rules="[validationRules.required, validationRules.email]"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              autofocus
              class="custom-input"
            />
          </div>
 
          <div class="input-group">
            <label>{{ loginConfig?.texts?.passwordLabel || 'Password' }}</label>
            <v-text-field
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              :placeholder="loginConfig?.texts?.passwordPlaceholder || 'Enter your password'"
              prepend-inner-icon="mdi-lock-outline"
              :append-inner-icon="showPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'"
              :rules="[validationRules.required]"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              class="custom-input"
              @click:append-inner="showPassword = !showPassword"
            />
          </div>
 
          <div
            class="options-row"
            :class="{ 'options-row--no-forgot': !showForgotPassword }"
          >
            <v-checkbox
              v-model="form.rememberMe"
              :label="loginConfig?.texts?.rememberMe || 'Remember me'"
              density="compact"
              hide-details
              class="remember-checkbox"
            />
            <a
              v-if="showForgotPassword"
              class="forgot-link"
              @click.prevent="$router.push('/auth/forgot-password')"
            >
              {{ loginConfig?.texts?.forgotPassword || 'Forgot password?' }}
            </a>
          </div>
 
          <v-btn
            type="submit"
            size="large"
            block
            :loading="authStore.isLoading"
            :disabled="!isFormValid"
            class="login-btn"
          >
            {{ loginConfig?.texts?.signIn || 'Sign In' }}
          </v-btn>
        </v-form>
 
        <!-- Register Section (controlled by showRegister prop) -->
        <template v-if="showRegister">
          <div class="divider">
            <span>{{ loginConfig?.texts?.divider || 'or' }}</span>
          </div>
 
          <div class="register-section">
            <p>{{ loginConfig?.texts?.noAccount || "Don't have an account?" }}</p>
            <v-btn
              variant="outlined"
              class="register-btn"
              @click="$router.push('/auth/register')"
            >
              <v-icon start size="18">mdi-account-plus-outline</v-icon>
              {{ loginConfig?.texts?.createAccount || 'Create Account' }}
            </v-btn>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
 
<script setup lang="ts">
import type { LoginRequest } from '~/types'
import { parseApiErrorMessages } from '~/utils/apiError'
 
const props = withDefaults(defineProps<{
  showRegister?: boolean
  showForgotPassword?: boolean
}>(), {
  showRegister: false, // false: register gizli, true yapınca açılır
  showForgotPassword: false // false: forgot password linki gizli, true: gösterilir
})
 
const route = useRoute()
const authStore = useAuthStore()
const { validationRules } = useValidators()
const { getBackgroundImages, getLoginConfig, getLogo } = useAppData()

const logoSrc = computed(() => getLogo.value?.src || '/favicon.ico')
const logoAlt = computed(() => getLogo.value?.alt || 'Logo')
 
const loginForm = ref()
const isFormValid = ref(false)
const showPassword = ref(false)
const loginApiErrors = ref<string[]>([])
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
 
// ── Carousel Logic ──
let imageInterval: ReturnType<typeof setInterval>
let isPaused = false
 
const goToSlide = (index: number) => {
  currentImageIndex.value = index
}
 
const nextSlide = () => {
  if (!isPaused && backgroundImages.value.length > 0) {
    currentImageIndex.value = (currentImageIndex.value + 1) % backgroundImages.value.length
  }
}
 
// ── Login Handler ──
const handleLogin = async () => {
  if (!isFormValid.value) return
  loginApiErrors.value = []
  try {
    await useAuth().login(form)
  } catch (error) {
    console.error('Login failed:', error)
    loginApiErrors.value = parseApiErrorMessages(error)
  }
}
 
// ── Lifecycle ──
onMounted(() => {
  // Pre-fill email from query
  const registeredEmail = route.query.email as string
  if (registeredEmail) form.email = registeredEmail
 
  // Device info
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
 
  // Start carousel
  imageInterval = setInterval(nextSlide, loginConfig.value?.rotationInterval || 3000)
})
 
onUnmounted(() => {
  if (imageInterval) clearInterval(imageInterval)
})
</script>
 
<style scoped>
/* ── Variables ── */
.login-page {
  --lp-bg: #0a0a0f;
  --lp-card-border: rgba(255, 255, 255, 0.06);
  --lp-input-bg: rgba(255, 255, 255, 0.04);
  --lp-input-border: rgba(255, 255, 255, 0.1);
  --lp-accent: #818cf8;
  --lp-accent-glow: rgba(129, 140, 248, 0.25);
  --lp-gradient: linear-gradient(135deg, #6366f1, #8b5cf6, #a855f7);
  --lp-text: #f1f1f4;
  --lp-text-secondary: rgba(255, 255, 255, 0.5);
  --lp-text-muted: rgba(255, 255, 255, 0.3);
 
  display: flex;
  min-height: 100vh;
  background: var(--lp-bg);
  color: var(--lp-text);
  font-family: inherit;
}
 
/* ── LEFT PANEL ── */
.left-panel {
  flex: 1.1;
  position: relative;
  overflow: hidden;
  display: flex;
  align-items: flex-end;
}
 
.carousel-slide {
  position: absolute;
  inset: 0;
  z-index: 0;
  background-size: cover;
  background-position: center;
  opacity: 0;
  /* Sadece crossfade — zoom/scale yok (önceki “geri küçülme” hissi kalkar) */
  transition: opacity 1s cubic-bezier(0.4, 0, 0.2, 1);
  transform: translateZ(0);
  backface-visibility: hidden;
}

.carousel-slide.active {
  z-index: 1;
  opacity: 1;
}
 
.left-overlay {
  position: absolute;
  inset: 0;
  background:
    linear-gradient(to right, transparent 60%, var(--lp-bg) 100%),
    linear-gradient(to top, rgba(10, 10, 15, 0.85) 0%, transparent 50%),
    linear-gradient(to bottom, rgba(10, 10, 15, 0.3) 0%, transparent 30%);
  z-index: 2;
}
 
.left-content {
  position: relative;
  z-index: 3;
  padding: 48px;
  width: 100%;
}
 
.brand-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 100px;
  padding: 8px 18px;
  font-size: 0.8rem;
  font-weight: 500;
  letter-spacing: 0.5px;
  margin-bottom: 24px;
}
 
.left-title {
  font-size: 2.4rem;
  font-weight: 700;
  line-height: 1.15;
  color: white;
  margin-bottom: 14px;
  max-width: 440px;
}
 
.left-subtitle {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.55);
  line-height: 1.6;
  max-width: 400px;
}
 
.carousel-dots {
  display: flex;
  gap: 8px;
  margin-top: 28px;
}
 
.dot {
  width: 32px;
  height: 3px;
  border-radius: 3px;
  background: rgba(255, 255, 255, 0.2);
  transition: all 0.5s ease;
  cursor: pointer;
}
 
.dot.active {
  width: 48px;
  background: var(--lp-accent);
  box-shadow: 0 0 12px var(--lp-accent-glow);
}
 
/* ── RIGHT PANEL ── */
.right-panel {
  flex: 0.9;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  position: relative;
  z-index: 5;
}
 
.right-panel::before {
  content: '';
  position: absolute;
  top: -200px;
  right: -200px;
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, rgba(99, 102, 241, 0.06), transparent 70%);
  pointer-events: none;
}
 
.right-inner {
  width: 100%;
  max-width: 400px;
  animation: fadeUp 0.7s 0.2s cubic-bezier(0.22, 1, 0.36, 1) both;
}
 
@keyframes fadeUp {
  from { opacity: 0; transform: translateY(24px); }
  to { opacity: 1; transform: translateY(0); }
}
 
/* Form Header */
.form-header {
  margin-bottom: 36px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.form-header-logo {
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.form-header-logo-img {
  max-height: 56px;
  width: auto;
  max-width: min(220px, 100%);
  height: auto;
  object-fit: contain;
  display: block;
}

.form-header h1 {
  font-size: 1.7rem;
  font-weight: 700;
  margin-bottom: 6px;
  width: 100%;
}

.form-header p {
  color: var(--lp-text-secondary);
  font-size: 0.92rem;
  width: 100%;
}
 
/* Input Groups */
.input-group {
  margin-bottom: 20px;
}
 
.input-group label {
  display: block;
  font-size: 0.82rem;
  font-weight: 500;
  color: var(--lp-text-secondary);
  margin-bottom: 8px;
  letter-spacing: 0.3px;
}
 
.custom-input :deep(.v-field) {
  background: var(--lp-input-bg) !important;
  border-color: var(--lp-input-border) !important;
  border-radius: 12px !important;
  color: var(--lp-text) !important;
}
 
.custom-input :deep(.v-field--focused) {
  border-color: var(--lp-accent) !important;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important;
}
 
.custom-input :deep(.v-field__input),
.custom-input :deep(.v-label),
.custom-input :deep(.v-icon) {
  color: rgba(255, 255, 255, 0.6) !important;
}
 
.custom-input :deep(.v-field__input) {
  color: var(--lp-text) !important;
}
 
/* Expired Alert */
.expired-alert :deep(.v-alert) {
  border-radius: 12px !important;
}

.login-api-error-alert :deep(.v-alert) {
  border-radius: 12px !important;
}

.login-api-error-list {
  margin: 0;
  padding-left: 1.1rem;
  list-style: disc;
}

.login-api-error-list li {
  margin-top: 0.25rem;
}

.login-api-error-list li:first-child {
  margin-top: 0;
}
 
/* Options Row */
.options-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 28px;
}

.options-row--no-forgot {
  justify-content: flex-start;
}
 
.remember-checkbox :deep(.v-label) {
  font-size: 0.85rem;
  color: var(--lp-text-secondary) !important;
}
 
.remember-checkbox :deep(.v-selection-control__input) {
  color: var(--lp-accent) !important;
}
 
.forgot-link {
  font-size: 0.85rem;
  color: var(--lp-accent);
  text-decoration: none;
  font-weight: 500;
  cursor: pointer;
  transition: color 0.2s;
}
 
.forgot-link:hover {
  color: #a5b4fc;
}
 
/* Login Button */
.login-btn {
  border-radius: 12px !important;
  height: 50px !important;
  font-weight: 600 !important;
  text-transform: none !important;
  background: var(--lp-gradient) !important;
  color: white !important;
  letter-spacing: 0.3px;
  transition: all 0.3s ease !important;
}
 
.login-btn:hover:not(:disabled) {
  transform: translateY(-2px) !important;
  box-shadow: 0 12px 40px rgba(99, 102, 241, 0.35) !important;
}
 
/* Divider */
.divider {
  display: flex;
  align-items: center;
  gap: 16px;
  margin: 28px 0;
}
 
.divider span {
  font-size: 0.8rem;
  color: var(--lp-text-muted);
  white-space: nowrap;
}
 
.divider::before,
.divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--lp-card-border);
}
 
/* Register Section */
.register-section {
  text-align: center;
}
 
.register-section p {
  font-size: 0.88rem;
  color: var(--lp-text-secondary);
  margin-bottom: 14px;
}
 
.register-btn {
  border: 1.5px solid rgba(255, 255, 255, 0.12) !important;
  border-radius: 12px !important;
  color: var(--lp-text) !important;
  font-weight: 500 !important;
  text-transform: none !important;
  transition: all 0.3s ease !important;
}
 
.register-btn:hover {
  border-color: var(--lp-accent) !important;
  background: rgba(99, 102, 241, 0.06) !important;
  transform: translateY(-1px) !important;
}
 
/* ── Responsive ── */
@media (max-width: 1024px) {
  .left-panel { display: none; }
  .right-panel { flex: 1; }
}
 
@media (max-width: 480px) {
  .right-panel { padding: 24px 20px; }
  .form-header h1 { font-size: 1.4rem; }
}
</style>
 

