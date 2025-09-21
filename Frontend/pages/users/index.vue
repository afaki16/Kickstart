<template>
    <!-- Breadcrumb -->
    <div class="mb-6">
      <BreadCrumb :items="[
        { text: 'Ana Sayfa', to: '/' },
        { text: 'Kullanıcılar' }
      ]" />
    </div>

  <BaseDataTable
      :items="items"
      :columns="tableColumns"
      title="Kullanıcılar"
      toolbar-icon="mdi-account"
      search-placeholder="Kullanıcı ara..."
      add-button-text="Yeni Kullanıcı Ekle"
      :loading="isLoading"
      loading-text="Kullanıcılar yükleniyor..."
      empty-title="Kullanıcı bulunamadı"
      :show-add-button="true"
      :show-advanced-filters="true"
      :show-actions="true"
      :show-view-button="true"
      :show-edit-button="true"
      :show-delete-button="true"
      :show-pagination="true"
      :items-per-page="10"
      @add="openCreateDialog"
      @view="openViewDialog"
      @edit="openEditDialog"
      @delete="openDeleteDialog"
      @search="handleSearch"
    >
    <!-- For FullName -->
 <template #cell-fullName="{ item, value }">
  <v-chip
    color="primary"
    variant="tonal"
    size="default"
    prepend-icon="mdi-account"
  >
    {{ value }}
    <span v-if="item.username" class="ml-1 opacity-75 text-xs">
      (@{{ item.username }})
    </span>
  </v-chip>
</template>

<!-- For Status -->
  <template #cell-status="{ value }">
  <v-chip
    :color="value == 1 ? 'success' : 'error'"
    size="small"
    variant="tonal"
  >
    {{ value == 1 ? 'Aktif' : 'Pasif' }}
  </v-chip>
</template>
  </BaseDataTable>

<!-- Create/Edit User Dialog -->
<v-dialog v-model="dialogs.create" max-width="800" scrollable>
  <v-card>
    <v-card-text>
    <UserForm
      :user="selectedItem"
      :roles="roles"
      :loading="isLoading"
      @submit="handleSubmit"
      @cancel="closeCreateDialog"
    />
    </v-card-text>
  </v-card>
</v-dialog>

<!-- Confirm Delete Dialog -->
<ConfirmDialog
  v-model="dialogs.delete"
  title="Kullanıcıyı Sil"
  :message="`'${itemToDelete?.fullName || itemToDelete?.email}' kullanıcısını silmek istediğinizden emin misiniz?`"
  type="error"
  confirm-text="Sil"
  :loading="isDeleting"
  @confirm="confirmDelete"
  @cancel="closeDeleteDialog"
/>

</template>

<script setup>
import { ref, onMounted } from 'vue'
import { dateTimeFormatLong } from '~/utils/datesFormat.ts'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'
import UserForm from '~/components/Users/UserForm.vue'
import ConfirmDialog from '~/components/UI/ConfirmDialog.vue'

//#region Page Metadata
definePageMeta({
  title: 'Kullanıcılar',
  requiresAuth: true,
  permissions: ['users.read']
})

useHead({
  title: 'Kullanıcılar',
})
//#endregion

//#region DataTable Columns
const tableColumns = [
  { 
    label: 'Ad Soyad', 
    key: 'fullName',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '300px'
  },
  { 
    label: 'E-posta', 
    key: 'email',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '300px'
  },
  { 
    label: 'Durum', 
    key: 'status',
    sortable: true,
    filterable: true,
    filterType: 'select',
    width: '300px'
  },
  { 
    label: 'Son Giriş Tarihi', 
    key: 'lastLoginDate',
    sortable: true,
    filterable: false,
    width: '300px',
    formatter: dateTimeFormatLong
  },
]
//#endregion

//#region Composables
const { getUsers, createUser, updateUser, deleteUser } = useUsers()
const { getRoles } = useRoles()

// CRUD Operations with Dialog Manager
const {
  items,
  isLoading,
  isDeleting,
  dialogs,
  selectedItem,
  itemToDelete,
  isEditMode,
  openCreateDialog,
  openViewDialog,
  openEditDialog,
  openDeleteDialog,
  closeCreateDialog,
  closeDeleteDialog,
  handleSubmit,
  confirmDelete,
  handleSearch,
  loadItemsData
} = useCrudOperations({
  loadItems: getUsers,
  createItem: createUser,
  updateItem: updateUser,
  deleteItem: deleteUser,
  itemName: 'kullanıcı'
})

//#endregion

//#region Additional Data
const roles = ref([])

const loadRoles = async () => {
  try {
    const response = await getRoles()
    roles.value = response || []
  } catch (error) {
    console.error('Error loading roles:', error)
    roles.value = []
  }
}
//#endregion

//#region Lifecycle
onMounted(async () => {
  await Promise.all([loadItemsData(), loadRoles()])
})
//#endregion
</script>