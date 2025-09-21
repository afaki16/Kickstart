<template>
  <div class="register-container" :style="{ background: themeGradients?.register || 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }">
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
        :style="{ background: registerConfig?.overlay?.color || 'rgba(0, 0, 0, 0.4)' }"
      ></div>
    </div>

    <!-- 3D Register Card -->
    <div class="register-card-container">
      <div 
        class="register-card"
        :style="{
          background: registerConfig?.card?.background || 'rgba(255, 255, 255, 0.1)',
          backdropFilter: registerConfig?.card?.backdropFilter || 'blur(20px)',
          borderRadius: registerConfig?.card?.borderRadius || '20px',
          border: registerConfig?.card?.border || '1px solid rgba(255, 255, 255, 0.2)',
          boxShadow: registerConfig?.card?.shadow || '0 25px 45px rgba(0, 0, 0, 0.2)'
        }"
      >
        <div class="card-content">
          <div class="logo-section">
            <div class="logo-icon">
              <v-icon size="64" color="white">mdi-account-plus</v-icon>
            </div>
            <h1 class="welcome-text">{{ registerConfig?.texts?.welcome || 'Create Account' }}</h1>
            <p class="subtitle-text">{{ registerConfig?.texts?.subtitle || 'Join us today and get started' }}</p>
          </div>

          <!-- Register Form Component -->
          <AuthRegisterForm
            ref="registerFormRef"
            :loading="authStore.isLoading"
            @submit="handleRegister"
          >
            <template #terms>
              <!-- Terms and conditions -->
              <div class="terms-section mb-4">
                <v-checkbox
                  v-model="acceptTerms"
                  :rules="[v => !!v || 'You must accept the terms and conditions']"
                  density="compact"
                  color="white"
                  hide-details
                >
                  <template #label>
                    <span class="terms-text">
                      {{ registerConfig?.texts?.agreeTo || 'I agree to the' }}
                      <a href="/terms" target="_blank" class="terms-link">{{ registerConfig?.texts?.terms || 'Terms of Service' }}</a>
                      {{ registerConfig?.texts?.and || 'and' }}
                      <a href="/privacy" target="_blank" class="terms-link">{{ registerConfig?.texts?.privacy || 'Privacy Policy' }}</a>
                    </span>
                  </template>
                </v-checkbox>
              </div>
            </template>

            <template #submit-text>
              <span class="btn-text">{{ registerConfig?.texts?.createAccount || 'Create Account' }}</span>
            </template>
          </AuthRegisterForm>

          <div class="divider-section">
            <div class="divider-line"></div>
            <span class="divider-text">{{ registerConfig?.texts?.divider || 'or' }}</span>
            <div class="divider-line"></div>
          </div>

          <div class="login-section">
            <p class="login-text">
              {{ registerConfig?.texts?.alreadyHaveAccount || 'Already have an account?' }}
            </p>
            <v-btn
              variant="outlined"
              color="white"
              @click="$router.push('/')"
              class="login-btn"
            >
              {{ registerConfig?.texts?.signIn || 'Sign In' }}
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
import type { RegisterRequest } from '~/types'
import AuthRegisterForm from '~/components/Auth/RegisterForm.vue'

// Meta
definePageMeta({
  layout: false,
  middleware: 'guest'
})

// Composables
const authStore = useAuthStore()
const auth = useAuth()

// Reactive data
const registerFormRef = ref()
const acceptTerms = ref(false)
const currentImageIndex = ref(0)

// App data
const { loadAppData, getBackgroundImages, getRegisterConfig, getThemeGradients } = useAppData()

// Background images from data.json
const backgroundImages = computed(() => getBackgroundImages.value || [])
const registerConfig = computed(() => getRegisterConfig.value)
const themeGradients = computed(() => getThemeGradients.value)

// Methods
const handleRegister = async (formData: RegisterRequest) => {
  if (!acceptTerms.value) return

  try {
    await auth.register(formData)
  } catch (error) {
    console.error('Registration failed:', error)
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

// Background image rotation
let imageInterval: NodeJS.Timeout

// Lifecycle
onMounted(async () => {
  // Load app data
  await loadAppData()

  // Start background image rotation
  imageInterval = setInterval(() => {
    currentImageIndex.value = (currentImageIndex.value + 1) % backgroundImages.value.length
  }, registerConfig.value?.rotationInterval || 3000)
})

onUnmounted(() => {
  if (imageInterval) {
    clearInterval(imageInterval)
  }
})

// SEO
useHead({
  title: 'Create Account - {{PROJECT_NAME}}',
  meta: [
    { name: 'description', content: 'Create your {{PROJECT_NAME}} account' }
  ]
})
</script>

<style scoped>
.register-container {
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

.register-card-container {
  position: relative;
  z-index: 10;
  perspective: 1000px;
}

.register-card {
  padding: 40px;
  width: 450px;
  max-width: 90vw;
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


.terms-section {
  text-align: left;
}

.terms-section :deep(.v-checkbox) {
  color: white;
}

.terms-text {
  color: rgba(255, 255, 255, 0.8);
  font-size: 0.875rem;
  line-height: 1.4;
}

.terms-link {
  color: white;
  text-decoration: underline;
  font-weight: 600;
}

.terms-link:hover {
  color: rgba(255, 255, 255, 0.9);
}

.register-btn {
  border-radius: 12px !important;
  height: 50px !important;
  font-weight: 600 !important;
  text-transform: none !important;
  transition: all 0.3s ease !important;
}

.register-btn:hover {
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

.login-section {
  text-align: center;
}

.login-text {
  color: rgba(255, 255, 255, 0.8);
  margin-bottom: 15px;
}

.login-btn {
  border: 2px solid rgba(255, 255, 255, 0.3) !important;
  border-radius: 12px !important;
  color: white !important;
  font-weight: 600 !important;
  text-transform: none !important;
  transition: all 0.3s ease !important;
}

.login-btn:hover {
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
  .register-card {
    width: 90%;
    max-width: 400px;
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
</style> 