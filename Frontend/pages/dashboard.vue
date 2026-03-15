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
      <v-col cols="12" lg="12" v-if="isAdminOrSuperAdmin">
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
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { Chart, registerables } from 'chart.js'
import { useAuth } from '~/composables/useAuth'
import { useAuthStore } from '~/stores/auth'

Chart.register(...registerables)

definePageMeta({
  middleware: ['auth', 'permission'],
})

const authStore = useAuthStore()
const { hasRole } = useAuth()

const isAdminOrSuperAdmin = computed(() => {
  return hasRole('Admin') || hasRole('SuperAdmin')
})

const revenueChart = ref<HTMLCanvasElement>()

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

const initRevenueChart = () => {
  if (!revenueChart.value) return

  new Chart(revenueChart.value, {
    type: 'line',
    data: {
      labels: ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'],
      datasets: [{
        label: 'Gelir (₺)',
        data: [12500, 19800, 15200, 21300, 18900, 25400, 22100],
        borderColor: getComputedStyle(document.documentElement).getPropertyValue('--theme-primary').trim() || '#2563eb',
        backgroundColor: `rgba(${getComputedStyle(document.documentElement).getPropertyValue('--theme-primary-rgb').trim() || '37, 99, 235'}, 0.1)`,
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

onMounted(() => {
  if (isAdminOrSuperAdmin.value) {
    setTimeout(() => {
      initRevenueChart()
    }, 100)
  }
})

useHead({
  title: 'Dashboard - Kickstart'
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
  background: var(--theme-gradient);
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

.chart-card {
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

::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.05);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb {
  background: rgba(var(--theme-primary-rgb, 37, 99, 235), 0.3);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(var(--theme-primary-rgb, 37, 99, 235), 0.5);
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

</style>
