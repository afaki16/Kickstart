<template>
  <div class="form-container">
    <PageHeader
      :title="role ? 'Rol Düzenle' : 'Yeni Rol Oluştur'"
      subtitle="Sistem rollerini ve izinlerini yönetin"
      icon="mdi-shield-account"
    />
      
    <v-form ref="form" v-model="isValid" @submit.prevent="handleSubmit">
      <div class="form-content">
        <!-- Rol Bilgileri Card -->
        <v-card class="form-card" elevation="0">
          <v-card-title class="form-card-title">
            <v-icon class="form-card-title-icon">mdi-information</v-icon>
            Rol Bilgileri
          </v-card-title>
          
          <v-card-text class="form-card-content">
            <div class="input-group">
              <v-text-field
                v-model="formData.name"
                label="Rol Adı"
                placeholder="Örn: Yönetici, Editör, Kullanıcı"
                :rules="[rules.required, rules.minLength(2), rules.maxLength(50)]"
                variant="outlined"
                :disabled="loading || (role && role.isSystemRole)"
                prepend-inner-icon="mdi-account-key"
                color="primary"
                class="modern-input"
                hide-details="auto"
              />
              
              <v-textarea
                v-model="formData.description"
                label="Açıklama"
                placeholder="Bu rolün sorumluluklarını ve amacını açıklayın..."
                :rules="[rules.maxLength(500)]"
                variant="outlined"
                :disabled="loading"
                rows="3"
                auto-grow
                prepend-inner-icon="mdi-text-box"
                color="primary"
                class="modern-input"
                hide-details="auto"
              />
            </div>
          </v-card-text>
        </v-card>

        <!-- İzinler Card -->
        <v-card class="form-card" elevation="0">
          <v-card-title class="form-card-title">
            <v-icon class="form-card-title-icon">mdi-key-variant</v-icon>
            İzinler
            <v-chip 
              v-if="formData.permissionIds.length" 
              size="small" 
              color="primary" 
              variant="tonal"
              class="ml-auto"
            >
              {{ formData.permissionIds.length }} seçili
            </v-chip>
          </v-card-title>

          <v-card-text class="permissions-content">
            <div v-if="groupedPermissions && Object.keys(groupedPermissions).length" class="permissions-tree">
              <!-- Search Bar -->
              <div class="search-container">
                <v-text-field
                  v-model="searchQuery"
                  placeholder="İzinlerde ara..."
                  prepend-inner-icon="mdi-magnify"
                  variant="outlined"
                  density="compact"
                  hide-details
                  clearable
                  class="search-input"
                />
              </div>

              <!-- Quick Actions -->
              <div class="quick-actions">
                <v-btn
                  variant="outlined"
                  size="small"
                  @click="selectAllPermissions"
                  :disabled="loading"
                  prepend-icon="mdi-check-all"
                >
                  Tümünü Seç
                </v-btn>
                <v-btn
                  variant="outlined"
                  size="small"
                  @click="clearAllPermissions"
                  :disabled="loading"
                  prepend-icon="mdi-close-box"
                >
                  Temizle
                </v-btn>
              </div>

              <!-- Simple Collapsible List -->
              <div class="permissions-simple-tree">
                <div
                  v-for="(perms, resource) in filteredPermissions"
                  :key="resource"
                  class="resource-group"
                >
                  <!-- Resource Header - With Group Checkbox -->
                  <div 
                    class="resource-row"
                    @click="toggleResource(resource)"
                  >
                    <v-icon 
                      class="chevron-icon"
                      :class="{ 'expanded': expandedResources.includes(resource) }"
                    >
                      mdi-chevron-right
                    </v-icon>
                    <v-checkbox
                      :model-value="isResourceFullySelected(resource)"
                      :indeterminate="isResourcePartiallySelected(resource)"
                      @update:model-value="toggleResourcePermissions(resource, $event)"
                      @click.stop
                      color="primary"
                      hide-details
                      density="compact"
                      class="resource-group-checkbox"
                    />
                    <span class="resource-title">{{ resource }} ({{ perms.length }})</span>
                  </div>

                  <!-- Permissions List - Simple Checkboxes -->
                  <div 
                    v-show="expandedResources.includes(resource)"
                    class="permissions-simple-container"
                  >
                    <div
                      v-for="permission in perms"
                      :key="permission.id"
                      class="permission-simple-item"
                    >
                      <v-checkbox
                        v-model="formData.permissionIds"
                        :value="String(permission.id)"
                        :disabled="loading"
                        color="primary"
                        hide-details
                        density="compact"
                        class="permission-simple-checkbox"
                        :label="permission.name"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Empty State -->
            <div v-else class="empty-state">
              <div class="empty-icon">
                <v-icon size="64" color="grey-400">mdi-key-off</v-icon>
              </div>
              <h3 class="empty-title">İzin Bulunamadı</h3>
              <p class="empty-text">Atanabilir izin bulunmuyor</p>
            </div>
          </v-card-text>
        </v-card>
      </div>

      <!-- Actions -->
      <div class="form-actions">
        <v-btn
          variant="outlined"
          size="large"
          @click="$emit('cancel')"
          :disabled="loading"
          class="btn-cancel"
        >
          İptal
        </v-btn>
        
        <v-btn
          color="primary"
          size="large"
          :loading="loading"
          type="submit"
          class="btn-submit"
          prepend-icon="mdi-check"
        >
          {{ role ? 'Güncelle' : 'Oluştur' }}
        </v-btn>
      </div>
    </v-form>
  </div>
</template>

<script setup lang="ts">
import type { Role, Permission, CreateRoleRequest, UpdateRoleRequest } from '~/types'
import PageHeader from '~/components/UI/PageHeader.vue'
import { onMounted, watch, watchEffect, computed, reactive, ref } from 'vue';
import { useValidators } from '~/composables/useValidators';

// Props
const props = defineProps<{
  role?: Role | null
  permissions: Permission[]
  loading?: boolean
}>()

// Emits
const emit = defineEmits<{
  submit: [data: CreateRoleRequest | UpdateRoleRequest]
  cancel: []
  'update:selected-permissions': [permissionIds: string[]]
}>()

// Composables
const { validationRules: rules } = useValidators()

// Reactive data
const form = ref()
const isValid = ref(false)
const expandedResources = ref<string[]>([])
const searchQuery = ref('')

const formData = reactive<CreateRoleRequest>({
  name: '',
  description: '',
  permissionIds: []
})

// Computed
const groupedPermissions = computed(() => {
  return props.permissions.reduce((groups, permission) => {
    const resource = permission.resource
    if (!groups[resource]) {
      groups[resource] = []
    }
    groups[resource].push(permission)
    return groups
  }, {} as Record<string, Permission[]>)
})

const filteredPermissions = computed(() => {
  if (!searchQuery.value) return groupedPermissions.value
  
  const filtered: Record<string, Permission[]> = {}
  Object.entries(groupedPermissions.value).forEach(([resource, permissions]) => {
    const matchingPermissions = permissions.filter(permission =>
      permission.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      resource.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
    if (matchingPermissions.length > 0) {
      filtered[resource] = matchingPermissions
    }
  })
  return filtered
})

// Methods
const isResourceFullySelected = (resource: string): boolean => {
  const resourcePermissions = groupedPermissions.value[resource] || []
  return resourcePermissions.length > 0 && 
    resourcePermissions.every(p => formData.permissionIds.includes(String(p.id)))
}

const isResourcePartiallySelected = (resource: string): boolean => {
  const resourcePermissions = groupedPermissions.value[resource] || []
  const selectedCount = resourcePermissions.filter(p => 
    formData.permissionIds.includes(String(p.id))
  ).length
  
  return selectedCount > 0 && selectedCount < resourcePermissions.length
}

const toggleResource = (resource: string) => {
  const index = expandedResources.value.indexOf(resource)
  if (index > -1) {
    expandedResources.value.splice(index, 1)
  } else {
    expandedResources.value.push(resource)
  }
}

const toggleResourcePermissions = (resource: string, selected: boolean) => {
  const resourcePermissions = groupedPermissions.value[resource] || []
  
  if (selected) {
    resourcePermissions.forEach(permission => {
      const permissionId = String(permission.id)
      if (!formData.permissionIds.includes(permissionId)) {
        formData.permissionIds.push(permissionId)
      }
    })
  } else {
    formData.permissionIds = formData.permissionIds.filter(id =>
      !resourcePermissions.some(p => String(p.id) === id)
    )
  }
}

const selectAllPermissions = () => {
  formData.permissionIds = props.permissions.map(p => String(p.id))
}

const clearAllPermissions = () => {
  formData.permissionIds = []
}

const handleSubmit = async () => {
  const validation = await form.value.validate()
  if (!validation?.valid) return
  
  const submitData = {
    name: formData.name,
    description: formData.description,
    permissionIds: formData.permissionIds
  }
  
  if (props.role) {
    // Update role
    emit('submit', { ...submitData, id: props.role.id } as UpdateRoleRequest)
  } else {
    // Create role
    emit('submit', submitData as CreateRoleRequest)
  }
}

const submit = () => {
  handleSubmit()
}

// Watch for permission changes
watch(() => formData.permissionIds, (newIds) => {
  emit('update:selected-permissions', newIds)
}, { deep: true })

// Initialize form with role data if editing
watchEffect(() => {
  if (props.role) {
    Object.assign(formData, {
      name: props.role.name,
      description: props.role.description || '',
      // Permission ID'leri string olarak tut (checkbox v-model için)
      permissionIds: props.role.permissions?.map(p => String(p.id)) || []
    })
  } else {
    // Reset form when not editing
    Object.assign(formData, {
      name: '',
      description: '',
      permissionIds: []
    })
  }
})

// Auto-expand first resource
onMounted(() => {
  if (Object.keys(groupedPermissions.value).length > 0) {
    expandedResources.value = [Object.keys(groupedPermissions.value)[0]]
  }
})

// Expose methods
defineExpose({
  submit
})
</script>

<style scoped>

/* Permissions */
.permissions-content {
  padding: 0 !important;
}

.permissions-tree {
  padding: 24px;
}

.search-container {
  margin-bottom: 20px;
}

.search-input :deep(.v-field) {
  border-radius: 12px;
  background: #f8fafc;
}

.quick-actions {
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
}

.permissions-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

/* Simple Tree Style */
.permissions-simple-tree {
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  overflow: hidden;
}

.resource-group {
  border-bottom: 1px solid #f1f5f9;
}

.resource-group:last-child {
  border-bottom: none;
}

.resource-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  cursor: pointer;
  transition: background-color 0.2s ease;
  user-select: none;
}

.resource-row:hover {
  background: #f8fafc;
}

.chevron-icon {
  color: #64748b;
  transition: transform 0.2s ease;
  font-size: 16px;
}

.chevron-icon.expanded {
  transform: rotate(90deg);
}

.folder-icon {
  color: #64748b;
}

.resource-group-checkbox {
  margin: 0 !important;
}

.resource-group-checkbox :deep(.v-selection-control) {
  min-height: auto !important;
}

.resource-title {
  font-weight: 500;
  color: #1e293b;
  font-size: 0.875rem;
}

.permissions-simple-container {
  padding: 8px 16px 16px 40px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
}

.permission-simple-item {
  padding: 2px 0;
}

.permission-simple-checkbox :deep(.v-label) {
  font-size: 0.875rem !important;
  color: #374151 !important;
}

/* Responsive */
@media (max-width: 768px) {
  
  .permissions-grid {
    grid-template-columns: 1fr;
  }
  
}

@media (max-width: 640px) {
  
  .permission-simple-item {
    padding: 1px 0;
  }
}
</style>