<template>
  <div class="dashboard-container">
    <!-- Header Section -->
    <div class="dashboard-header">
      <div>
        <h1 class="dashboard-title">{{ authStore.user?.fullName }}</h1>
        <h1 class="dashboard-subtitle">Hoş Geldiniz</h1>
      </div>
      <div class="header-actions">
        <v-chip color="primary" variant="elevated" prepend-icon="mdi-calendar-check">
          Bugün: {{ todayDate }}
        </v-chip>
      </div>
    </div>

    <!-- Charts & Tables Row -->
    <v-row class="content-row">
      <!-- Revenue Chart -->
      <v-col cols="12" lg="8" v-if="isAdminOrSuperAdmin">
        <v-card class="chart-card" elevation="4">
          <v-card-title class="card-header">
            <v-icon class="mr-2" color="primary">mdi-chart-line</v-icon>
            Haftalık Gelir Analizi
          </v-card-title>
          <v-card-text>
            <canvas ref="revenueChart"></canvas>
          </v-card-text>
        </v-card>
      </v-col>

      <!-- Today's Appointments -->
      <v-col cols="12" :lg="isAdminOrSuperAdmin ? 4 : 12">
        <v-card class="appointments-card" elevation="4">
          <v-card-title class="card-header">
            <v-icon class="mr-2" color="success">mdi-calendar-today</v-icon>
            Günün Randevuları
          </v-card-title>
          <v-card-text class="appointments-list">
            <!-- Loading State -->
            <div v-if="loadingAppointments" class="text-center py-4">
              <v-progress-circular indeterminate color="primary"></v-progress-circular>
              <p class="mt-2 text-gray-600">Randevular yükleniyor...</p>
            </div>
            
            <!-- Empty State -->
            <div v-else-if="!todayAppointments.length" class="text-center py-8">
              <v-icon size="48" color="grey-lighten-1" class="mb-3">mdi-tooth-outline</v-icon>
              <p class="text-gray-600 text-base">Bugün için planlanmış randevu bulunmuyor</p>
            </div>
            
            <!-- Appointments List -->
            <div v-else>
              <div 
                v-for="appointment in todayAppointments" 
                :key="appointment.id"
                class="appointment-item clickable"
                @click="goToAppointmentDetail(appointment.id)"
              >
                <div class="appointment-time">
                  <v-icon size="20" color="primary">mdi-clock-outline</v-icon>
                  <span>{{ formatTime(appointment.startTime) }}</span>
                </div>
                <div class="appointment-details">
                  <p class="appointment-patient">{{ appointment.patientName }}</p>
                  <p class="appointment-doctor">{{ appointment.doctorName }}</p>
                  <p v-if="appointment.notes" class="appointment-notes">{{ appointment.notes }}</p>
                </div>
                <div class="appointment-actions">
                  <v-chip 
                    :color="getStatusColor(appointment.status)" 
                    size="small"
                    variant="flat"
                  >
                    {{ translateStatus(appointment.status) }}
                  </v-chip>
                  <v-icon size="20" color="primary" class="ml-2">mdi-chevron-right</v-icon>
                </div>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Randevu Detay Modal -->
    <v-dialog 
      v-model="showAppointmentModal" 
      max-width="600px"
      :persistent="false"
      eager
    >
      <v-card class="appointment-modal-card">
        <v-card-title class="d-flex align-center">
          <v-icon class="mr-2" color="primary">mdi-calendar-check</v-icon>
          Randevu Detayları
          <v-spacer></v-spacer>
          <v-btn icon @click="closeAppointmentModal">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>
        
        <v-divider></v-divider>
        
        <v-card-text v-if="selectedAppointment" class="pa-6">
          <v-row>
            <v-col cols="12" md="6">
              <div class="detail-section">
                <h3 class="detail-title">
                  <v-icon class="mr-2" color="success">mdi-account</v-icon>
                  Hasta Bilgileri
                </h3>
                <p class="detail-text">{{ selectedAppointment.patientName }}</p>
              </div>
            </v-col>
            
            <v-col cols="12" md="6">
              <div class="detail-section">
                <h3 class="detail-title">
                  <v-icon class="mr-2" color="info">mdi-doctor</v-icon>
                  Doktor Bilgileri
                </h3>
                <p class="detail-text">{{ selectedAppointment.doctorName }}</p>
              </div>
            </v-col>
            
            <v-col cols="12" md="6">
              <div class="detail-section">
                <h3 class="detail-title">
                  <v-icon class="mr-2" color="primary">mdi-clock</v-icon>
                  Randevu Saati
                </h3>
                <p class="detail-text">{{ formatTime(selectedAppointment.startTime) }} - {{ formatTime(selectedAppointment.endTime) }}</p>
              </div>
            </v-col>
            
            <v-col cols="12" md="6">
              <div class="detail-section">
                <h3 class="detail-title">
                  <v-icon class="mr-2" color="warning">mdi-information</v-icon>
                  Durum
                </h3>
                <v-chip 
                  :color="getStatusColor(selectedAppointment.status)" 
                  variant="flat"
                  class="detail-status"
                >
                  {{ translateStatus(selectedAppointment.status) }}
                </v-chip>
              </div>
            </v-col>
            
            <v-col cols="12" md="6">
              <div class="detail-section">
                <h3 class="detail-title">
                  <v-icon class="mr-2" color="purple">mdi-tag</v-icon>
                  Randevu Tipi
                </h3>
                <p class="detail-text">{{ translateAppointmentType(selectedAppointment.type) }}</p>
              </div>
            </v-col>
            
            <v-col cols="12" md="6">
              <div class="detail-section">
                <h3 class="detail-title">
                  <v-icon class="mr-2" color="orange">mdi-calendar</v-icon>
                  Tarih
                </h3>
                <p class="detail-text">{{ formatDate(selectedAppointment.appointmentDate) }}</p>
              </div>
            </v-col>
            
            <v-col cols="12" v-if="selectedAppointment.notes">
              <div class="detail-section">
                <h3 class="detail-title">
                  <v-icon class="mr-2" color="grey">mdi-note-text</v-icon>
                  Notlar
                </h3>
                <p class="detail-notes">{{ selectedAppointment.notes }}</p>
              </div>
            </v-col>
          </v-row>
        </v-card-text>
        
        <v-divider></v-divider>
        
        <v-card-actions class="pa-4">
          <v-spacer></v-spacer>
          <v-btn color="primary" variant="flat" @click="closeAppointmentModal">
            Kapat
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, computed, watch } from 'vue'
import { Chart, registerables } from 'chart.js'
import { useApi } from '~/composables/useApi'
import { useAuth } from '~/composables/useAuth'
import { useAuthStore } from '~/stores/auth'

Chart.register(...registerables)

definePageMeta({
  middleware: ['auth', 'permission'],
})

const authStore = useAuthStore()
const { hasRole } = useAuth()
const { get } = useApi()

const isAdminOrSuperAdmin = computed(() => {
  return hasRole('Admin') || hasRole('SuperAdmin')
})

const revenueChart = ref<HTMLCanvasElement>()
const loadingAppointments = ref(false)
const showAppointmentModal = ref(false)
const selectedAppointment = ref(null)

const stats = ref({
  todayAppointments: 24,
  todayRevenue: 45750,
  totalPatients: 1248,
  activeDoctors: 8
})

const todayDate = computed(() => {
  return new Date().toLocaleDateString('tr-TR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  })
})

const todayAppointments = ref([])

const fetchTodayAppointments = async () => {
  try {
    loadingAppointments.value = true
    const response = await get('/api/Appointment/today')
    if (response && response.value) {
      todayAppointments.value = response.value
    }
  } catch (error) {
    console.error('Bugünkü randevular yüklenirken hata oluştu:', error)
    todayAppointments.value = []
  } finally {
    loadingAppointments.value = false
  }
}

const formatTime = (timeString: string) => {
  if (!timeString) return ''
  return timeString.substring(0, 5)
}

const translateStatus = (status: string) => {
  const statusMap: { [key: string]: string } = {
    'Scheduled': 'Planlandı',
    'Confirmed': 'Onaylandı',
    'InProgress': 'Devam Ediyor',
    'Completed': 'Tamamlandı',
    'Cancelled': 'İptal Edildi',
    'NoShow': 'Gelmedi'
  }
  return statusMap[status] || status
}

const translateAppointmentType = (type: string) => {
  const typeMap: { [key: string]: string } = {
    'FirstVisit': 'İlk Ziyaret',
    'Checkup': 'Kontrol',
    'Treatment': 'Tedavi',
    'Consultation': 'Konsültasyon',
    'Emergency': 'Acil',
    'FollowUp': 'Takip'
  }
  return typeMap[type] || type
}

const formatDate = (dateString: string) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('tr-TR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  })
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Completed':
    case 'Tamamlandı': 
      return 'success'
    case 'InProgress':
    case 'Devam Ediyor': 
      return 'warning'
    case 'Scheduled':
    case 'Confirmed':
    case 'Planlandı':
    case 'Onaylandı':
    case 'Bekliyor': 
      return 'info'
    case 'Cancelled':
    case 'NoShow':
    case 'İptal':
    case 'Gelmedi': 
      return 'error'
    default: 
      return 'grey'
  }
}

const goToAppointmentDetail = (appointmentId: number) => {
  const appointment = todayAppointments.value.find(app => app.id === appointmentId)
  if (appointment) {
    selectedAppointment.value = appointment
    showAppointmentModal.value = true
  }
}

const closeAppointmentModal = () => {
  showAppointmentModal.value = false
  selectedAppointment.value = null
}

const handleKeyPress = (event: KeyboardEvent) => {
  if (event.key === 'Escape' && showAppointmentModal.value) {
    closeAppointmentModal()
  }
}

const handleModalClick = (event: MouseEvent) => {
  const target = event.target as HTMLElement
  if (target.classList.contains('v-overlay__scrim') || 
      target.classList.contains('v-overlay') ||
      target.classList.contains('v-overlay__content')) {
    closeAppointmentModal()
  }
}

watch(showAppointmentModal, (newValue) => {
  if (!newValue) {
    selectedAppointment.value = null
  }
})

const initRevenueChart = () => {
  if (!revenueChart.value) return

  new Chart(revenueChart.value, {
    type: 'line',
    data: {
      labels: ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'],
      datasets: [{
        label: 'Gelir (₺)',
        data: [12500, 19800, 15200, 21300, 18900, 25400, 22100],
        borderColor: 'rgb(102, 126, 234)',
        backgroundColor: 'rgba(102, 126, 234, 0.1)',
        tension: 0.4,
        fill: true
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            callback: function(value) {
              return '₺' + value.toLocaleString()
            }
          }
        }
      }
    }
  })
}

onMounted(async () => {
  await fetchTodayAppointments()
  
  if (isAdminOrSuperAdmin.value) {
    setTimeout(() => {
      initRevenueChart()
    }, 100)
  }
  
  document.addEventListener('keydown', handleKeyPress)
  document.addEventListener('click', handleModalClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('keydown', handleKeyPress)
  document.removeEventListener('click', handleModalClick)
})

useHead({
  title: 'Dashboard - {{PROJECT_NAME}}'
})
</script>

<style scoped>
.dashboard-container {
  padding: 24px;
  background: #f5f7fa;
  min-height: 100vh;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  background: white;
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.dashboard-title {
  font-size: 2rem;
  font-weight: 700;
  color: #2c3e50;
  margin: 0;
}

.dashboard-subtitle {
  font-size: 1.1rem;
  color: #7f8c8d;
  margin-top: 8px;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.stats-row {
  margin-bottom: 24px;
}

.stat-card {
  border-radius: 16px !important;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
  overflow: hidden;
}

.stat-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15) !important;
}

.stat-card-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.stat-card-success {
  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
}

.stat-card-warning {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.stat-card-info {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
}

.stat-card :deep(.v-card-text) {
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 24px !important;
}

.stat-icon-wrapper {
  width: 80px;
  height: 80px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-content {
  flex: 1;
  color: white;
}

.stat-value {
  font-size: 2.5rem;
  font-weight: 700;
  margin: 0;
  line-height: 1;
}

.stat-label {
  font-size: 0.95rem;
  opacity: 0.9;
  margin: 8px 0;
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.85rem;
  opacity: 0.8;
}

.content-row {
  margin-bottom: 24px;
}

.chart-card, .appointments-card {
  border-radius: 16px !important;
  height: 100%;
}

.card-header {
  background: #f8f9fa;
  font-weight: 600 !important;
  padding: 20px 24px !important;
  border-bottom: 1px solid #e9ecef;
}

.chart-card :deep(.v-card-text) {
  height: 400px;
  padding: 24px !important;
}

.appointments-list {
  max-height: 500px;
  overflow-y: auto;
  padding: 16px !important;
}

.appointment-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border-radius: 12px;
  margin-bottom: 12px;
  background: #f8f9fa;
  transition: all 0.3s ease;
}

.appointment-item.clickable {
  cursor: pointer;
}

.appointment-item:hover {
  background: #e9ecef;
  transform: translateX(4px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.15);
}

.appointment-actions {
  display: flex;
  align-items: center;
}

.appointment-time {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 80px;
  font-weight: 600;
  color: #667eea;
}

.appointment-details {
  flex: 1;
}

.appointment-patient {
  font-weight: 600;
  margin: 0;
  color: #2c3e50;
}

.appointment-doctor {
  font-size: 0.85rem;
  color: #7f8c8d;
  margin: 4px 0 0 0;
}

.appointment-notes {
  font-size: 0.8rem;
  color: #95a5a6;
  margin: 2px 0 0 0;
  font-style: italic;
}

::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.05);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb {
  background: rgba(102, 126, 234, 0.3);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(102, 126, 234, 0.5);
}

@media (max-width: 960px) {
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .dashboard-title {
    font-size: 2rem;
  }
}

.detail-section {
  margin-bottom: 20px;
}

.detail-title {
  font-size: 1rem;
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 8px;
  display: flex;
  align-items: center;
}

.detail-text {
  font-size: 1rem;
  color: #34495e;
  margin: 0;
  font-weight: 500;
}

.detail-notes {
  font-size: 0.95rem;
  color: #7f8c8d;
  margin: 0;
  background: #f8f9fa;
  padding: 12px;
  border-radius: 8px;
  border-left: 4px solid #667eea;
}

.detail-status {
  font-weight: 600 !important;
}

:deep(.v-dialog .v-card) {
  border-radius: 16px !important;
}

:deep(.v-dialog .v-card-title) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white !important;
  font-weight: 600 !important;
}
</style>
