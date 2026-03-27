<template>
  <div class="admin-settings">
    <h2 class="section-title">
      <v-icon color="primary" class="mr-2">mdi-shield-account-outline</v-icon>
      Tenant Ayarları
    </h2>
    <p class="section-desc mb-6">
      Kendi tenant'ınıza ait ayarları buradan yönetebilirsiniz.
    </p>

    <!-- Stats Row -->
    <v-row class="mb-6">
      <v-col cols="12" md="4">
        <v-card class="stat-card" variant="tonal" color="primary">
          <v-card-text class="d-flex align-center">
            <v-icon size="48" class="mr-4">mdi-account-group</v-icon>
            <div>
              <div class="text-h4 font-weight-bold">
                <v-progress-circular v-if="loadingCount" indeterminate size="24" />
                <template v-else>{{ activeUserCount }}</template>
              </div>
              <div class="text-body-2 text-medium-emphasis">Aktif Kullanıcı</div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Aktif oturumlar tablosu (tenant içi) -->
    <v-card variant="outlined" class="mb-4">
      <v-card-title class="d-flex align-center flex-wrap ga-2">
        <v-icon class="mr-2">mdi-account-multiple-check-outline</v-icon>
        Şu an çevrimiçi kullanıcılar
        <v-spacer />
        <v-btn
          size="small"
          variant="text"
          prepend-icon="mdi-refresh"
          :loading="loadingActiveUsers"
          @click="loadActiveUsers"
        >
          Yenile
        </v-btn>
      </v-card-title>
      <v-card-text>
        <p class="text-body-2 text-medium-emphasis mb-4">
          En az bir geçerli oturumu (refresh token) olan kullanıcılar. Yalnızca kendi tenant'ınıza ait kayıtlar listelenir.
        </p>
        <v-progress-linear v-if="loadingActiveUsers" indeterminate class="mb-2" />
        <v-table v-else-if="activeUsers.length" density="comfortable" class="active-users-table">
          <thead>
            <tr>
              <th class="text-left">Ad Soyad</th>
              <th class="text-left">E-posta</th>
              <th class="text-left">Oturum sayısı</th>
              <th class="text-left">Son aktivite</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in activeUsers" :key="row.userId">
              <td>{{ row.fullName }}</td>
              <td>{{ row.email }}</td>
              <td>{{ row.activeSessionCount }}</td>
              <td>{{ formatActivity(row.lastActivityAt) }}</td>
            </tr>
          </tbody>
        </v-table>
        <p v-else class="text-body-2 text-medium-emphasis">Şu anda çevrimiçi kullanıcı yok.</p>
      </v-card-text>
    </v-card>

    <!-- Revoke User Sessions -->
    <v-card variant="outlined" class="mb-4">
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2">mdi-account-off</v-icon>
        Uzaktan Kullanıcı Çıkışı
      </v-card-title>
      <v-card-text>
        <p class="text-body-2 text-medium-emphasis mb-4">
          Bir kullanıcıyı seçerek tüm cihazlarından çıkış yaptırabilirsiniz.
        </p>
        <v-row>
          <v-col cols="12" md="8">
            <v-autocomplete
              v-model="selectedUser"
              :items="users"
              item-title="fullName"
              item-value="id"
              label="Kullanıcı seçin"
              placeholder="Kullanıcı ara..."
              :loading="loadingUsers"
              clearable
              hide-details
              density="comfortable"
            >
              <template #item="{ props, item }">
                <v-list-item v-bind="props" :title="item.raw.fullName" :subtitle="item.raw.email" />
              </template>
            </v-autocomplete>
          </v-col>
          <v-col cols="12" md="4">
            <v-btn
              color="error"
              variant="tonal"
              :disabled="!selectedUser"
              :loading="revoking"
              prepend-icon="mdi-logout"
              @click="revokeSessions"
            >
              Çıkış Yaptır
            </v-btn>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { dateTimeFormat } from '~/utils/datesFormat'
import type { ActiveUserSnapshot } from '~/composables/useSettings'

const { getActiveUserCount, getActiveUsersSnapshot, getRevokableUsers, revokeUserSessions } = useSettings()

const activeUserCount = ref<number>(0)
const loadingCount = ref(false)
const users = ref<Array<{ id: number; fullName: string; email: string }>>([])
const loadingUsers = ref(false)
const selectedUser = ref<number | null>(null)
const revoking = ref(false)
const activeUsers = ref<ActiveUserSnapshot[]>([])
const loadingActiveUsers = ref(false)

const formatActivity = (iso: string | null) => {
  if (!iso) return '—'
  return dateTimeFormat(iso)
}

const loadActiveUsers = async () => {
  loadingActiveUsers.value = true
  try {
    activeUsers.value = await getActiveUsersSnapshot()
  } catch {
    activeUsers.value = []
  } finally {
    loadingActiveUsers.value = false
  }
}

const loadActiveCount = async () => {
  loadingCount.value = true
  try {
    activeUserCount.value = await getActiveUserCount()
  } catch {
    activeUserCount.value = 0
  } finally {
    loadingCount.value = false
  }
}

const loadUsers = async () => {
  loadingUsers.value = true
  try {
    users.value = await getRevokableUsers()
  } catch {
    users.value = []
  } finally {
    loadingUsers.value = false
  }
}

const revokeSessions = async () => {
  if (!selectedUser.value) return
  revoking.value = true
  try {
    await revokeUserSessions(selectedUser.value, 'Admin tarafından çıkış yaptırıldı')
    selectedUser.value = null
    await Promise.all([loadActiveCount(), loadUsers(), loadActiveUsers()])
  } finally {
    revoking.value = false
  }
}

onMounted(async () => {
  await Promise.all([loadActiveCount(), loadUsers(), loadActiveUsers()])
})
</script>

<style scoped>
.admin-settings {
  max-width: 900px;
}

.section-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 8px;
}

.section-desc {
  font-size: 0.95rem;
  color: #6c757d;
}

.stat-card {
  border-radius: 12px;
}

.active-users-table th {
  font-weight: 600;
  white-space: nowrap;
}
</style>
