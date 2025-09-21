<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title>
            API Debug - Login Test
          </v-card-title>
          
          <v-card-text>
            <v-form @submit.prevent="testLogin">
              <v-text-field
                v-model="testData.email"
                label="Email"
                type="email"
                required
                class="mb-3"
              />
              
              <v-text-field
                v-model="testData.password"
                label="Password"
                type="password"
                required
                class="mb-3"
              />
              
              <v-btn
                type="submit"
                color="primary"
                :loading="isLoading"
              >
                Test Login
              </v-btn>
            </v-form>
            
            <v-divider class="my-4"></v-divider>
            
            <h3>Request Details:</h3>
            <v-textarea
              v-model="requestDetails"
              label="Request"
              readonly
              rows="3"
              variant="outlined"
            ></v-textarea>
            
            <h3>Response Details:</h3>
            <v-textarea
              v-model="responseDetails"
              label="Response"
              readonly
              rows="10"
              variant="outlined"
            ></v-textarea>
            
            <h3>Error Details:</h3>
            <v-textarea
              v-model="errorDetails"
              label="Error"
              readonly
              rows="5"
              variant="outlined"
            ></v-textarea>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
const config = useRuntimeConfig()

const isLoading = ref(false)
const testData = reactive({
  email: 'admin@baseauth.com',
  password: 'Admin123!',
  rememberMe: true,
  deviceId: '',
  deviceName: ''
})

const requestDetails = ref('')
const responseDetails = ref('')
const errorDetails = ref('')

const testLogin = async () => {
  isLoading.value = true
  requestDetails.value = ''
  responseDetails.value = ''
  errorDetails.value = ''
  
  try {
    // Device bilgilerini otomatik doldur
    if (process.client) {
      // Device ID oluştur
      let deviceId = localStorage.getItem('deviceId')
      if (!deviceId) {
        deviceId = 'device_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
        localStorage.setItem('deviceId', deviceId)
      }
      testData.deviceId = deviceId

      // Device name tespit et
      const userAgent = navigator.userAgent
      if (userAgent.includes('Windows')) {
        testData.deviceName = 'Windows Device'
      } else if (userAgent.includes('Mac')) {
        testData.deviceName = 'Mac Device'
      } else if (userAgent.includes('Linux')) {
        testData.deviceName = 'Linux Device'
      } else if (userAgent.includes('Android')) {
        testData.deviceName = 'Android Device'
      } else if (userAgent.includes('iOS')) {
        testData.deviceName = 'iOS Device'
      } else {
        testData.deviceName = 'Unknown Device'
      }
    }
    
    const url = `${config.public.apiBase}/api/auth/login`
    requestDetails.value = `URL: ${url}\nMethod: POST\nData: ${JSON.stringify(testData, null, 2)}`
    
    console.log('Testing login with:', { url, data: testData })
    
    const response = await $fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: testData
    })
    
    responseDetails.value = JSON.stringify(response, null, 2)
    console.log('Login response:', response)
    
  } catch (error: any) {
    console.error('Login test error:', error)
    errorDetails.value = JSON.stringify({
      message: error.message,
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      headers: error.response?.headers
    }, null, 2)
  } finally {
    isLoading.value = false
  }
}

// Set page title
useHead({
  title: 'API Debug - {{PROJECT_NAME}}'
})
</script> 