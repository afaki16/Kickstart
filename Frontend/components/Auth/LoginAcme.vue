<template>
  <div class="login-acme">
    <!-- Sol: Branding -->
    <div class="login-acme-branding" :style="{ background: 'var(--theme-gradient-sidebar)' }">
      <div class="branding-overlay" />
      <div class="branding-content">
        <h1 class="branding-title">{{ loginConfig?.texts?.welcome || "Acme'ye Hoş Geldiniz" }}</h1>
        <p class="branding-subtitle">{{ loginConfig?.texts?.subtitle || 'Hesabınıza giriş yapın' }}</p>
        <div class="branding-logo">
          <v-icon size="80" color="white">mdi-shield-check</v-icon>
        </div>
      </div>
    </div>

    <!-- Sağ: Form -->
    <div class="login-acme-form">
      <div class="form-card">
        <h2 class="form-title">{{ loginConfig?.texts?.signIn || 'Giriş Yap' }}</h2>

        <v-alert
          v-if="route.query.expired"
          type="warning"
          variant="tonal"
          class="mb-4"
          closable
        >
          Oturumunuz sona erdi. Lütfen tekrar giriş yapın.
        </v-alert>

        <v-form ref="loginForm" v-model="isFormValid" @submit.prevent="handleLogin">
          <v-text-field
            v-model="form.email"
            :label="loginConfig?.texts?.emailLabel || 'E-posta Adresi'"
            type="email"
            prepend-inner-icon="mdi-email"
            :rules="[validationRules.required, validationRules.email]"
            variant="outlined"
            class="mb-3"
            density="comfortable"
          />

          <v-text-field
            v-model="form.password"
            :type="showPassword ? 'text' : 'password'"
            :label="loginConfig?.texts?.passwordLabel || 'Şifre'"
            prepend-inner-icon="mdi-lock"
            :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
            :rules="[validationRules.required]"
            variant="outlined"
            class="mb-3"
            density="comfortable"
            @click:append-inner="showPassword = !showPassword"
          />

          <div class="form-options">
            <v-checkbox
              v-model="form.rememberMe"
              :label="loginConfig?.texts?.rememberMe || 'Beni hatırla'"
              density="compact"
              hide-details
            />
            <v-btn
              variant="text"
              size="small"
              @click="$router.push('/auth/forgot-password')"
            >
              {{ loginConfig?.texts?.forgotPassword || 'Şifremi unuttum?' }}
            </v-btn>
          </div>

          <v-btn
            type="submit"
            color="primary"
            size="large"
            block
            :loading="authStore.isLoading"
            :disabled="!isFormValid"
            class="mt-4"
          >
            {{ loginConfig?.texts?.signIn || 'Giriş Yap' }}
          </v-btn>
        </v-form>

        <div class="form-footer">
          <span>{{ loginConfig?.texts?.noAccount || 'Hesabınız yok mu?' }}</span>
          <v-btn variant="text" color="primary" @click="$router.push('/auth/register')">
            {{ loginConfig?.texts?.createAccount || 'Hesap Oluştur' }}
          </v-btn>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { LoginRequest } from '~/types'

const route = useRoute()
const authStore = useAuthStore()
const { validationRules } = useValidators()
const { getLoginConfig } = useAppData()

const loginForm = ref()
const isFormValid = ref(false)
const showPassword = ref(false)

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
})
</script>

<style scoped>
.login-acme {
  display: flex;
  min-height: 100vh;
}

.login-acme-branding {
  flex: 1;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.branding-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.2);
}

.branding-content {
  position: relative;
  z-index: 1;
  text-align: center;
  color: white;
}

.branding-title {
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.branding-subtitle {
  font-size: 1.1rem;
  opacity: 0.9;
  margin-bottom: 2rem;
}

.branding-logo {
  opacity: 0.9;
}

.login-acme-form {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: #f8fafc;
}

.form-card {
  width: 100%;
  max-width: 400px;
  padding: 2.5rem;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1);
}

.form-title {
  font-size: 1.5rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
  color: #1e293b;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.form-footer {
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid #e2e8f0;
  text-align: center;
  font-size: 0.875rem;
  color: #64748b;
}

.form-footer .v-btn {
  margin-left: 0.25rem;
}

@media (max-width: 768px) {
  .login-acme {
    flex-direction: column;
  }

  .login-acme-branding {
    min-height: 200px;
    padding: 1.5rem;
  }

  .branding-title {
    font-size: 1.75rem;
  }

  .branding-logo {
    display: none;
  }
}
</style>
