# Kickstart Frontend - Yeni Sayfa Oluşturma Rehberi

Bu rehber, Kickstart Frontend projesinde yeni bir CRUD sayfa oluşturmak için izlenmesi gereken adımları detaylı şekilde açıklar. Users, Roles ve Tenants sayfaları referans alınmıştır.

---

## Genel Bakış - Proje Yapısı

```
Frontend/
├── pages/              → Sayfalar (Nuxt file-based routing)
├── composables/        → İş mantığı (API çağrıları, CRUD, dialog yönetimi)
├── components/         → Yeniden kullanılabilir bileşenler
│   ├── UI/             → Genel UI bileşenleri (BaseDataTable, ConfirmDialog, ResizableDrawer...)
│   └── [Entity]/       → Entity'ye özel form bileşenleri
├── types/              → TypeScript tip tanımları
├── utils/              → Yardımcı fonksiyonlar ve API endpoint tanımları
├── middleware/          → Auth ve permission middleware'leri
├── layouts/            → Sayfa layout'ları (default)
└── plugins/            → Axios, Vuetify, VeeValidate gibi plugin'ler
```

---

## Adım Adım Yeni Sayfa Oluşturma

Örnek entity adı: **Service** (Hizmet)

---

### ADIM 1: Tip Tanımlamaları (`types/`)

`types/service.ts` dosyası oluşturun:

```typescript
import type { Gender } from './common'

export interface Service {
  id: number
  name: string
  description?: string
  price: number
  isActive: boolean
  createdDate: string
  updatedDate?: string
}

export interface CreateServiceRequest {
  name: string
  description?: string
  price: number
  isActive: boolean
}

export interface UpdateServiceRequest {
  id: number
  name: string
  description?: string
  price: number
  isActive: boolean
}
```

Ardından `types/index.ts` dosyasına export ekleyin:

```typescript
// Service Types
export type {
  Service,
  CreateServiceRequest,
  UpdateServiceRequest
} from './service'
```

---

### ADIM 2: API Endpoint Tanımlamaları (`utils/apiEndpoints.ts`)

`API_ENDPOINTS` objesine yeni endpoint grubunu ekleyin:

```typescript
// Service management endpoints
SERVICES: {
  LIST: '/api/Service',
  CREATE: '/api/Service',
  GET_BY_ID: (id: string) => `/api/Service/${id}`,
  UPDATE: '/api/Service',
  DELETE: (id: string) => `/api/Service/${id}`,
},
```

> **Not:** Backend API yapınıza göre UPDATE endpoint'i bazen `'/api/Service'` (body'de id ile) bazen `(id: string) => \`/api/Service/${id}\`` şeklinde olabilir. Backend Swagger'ınızı kontrol edin.

---

### ADIM 3: Composable Oluşturma (`composables/`)

`composables/useServices.ts` dosyası oluşturun:

```typescript
import type { Service, CreateServiceRequest, UpdateServiceRequest } from '~/types'
import { API_ENDPOINTS, getPaginatedEndpoint } from '~/utils/apiEndpoints'
import { useApi } from './useApi'

export const useServices = () => {
  const api = useApi()

  //#region GET - Listeleme
  const getServices = async (page = 1, pageSize = 1000, searchTerm = '') => {
    try {
      let endpoint = getPaginatedEndpoint(API_ENDPOINTS.SERVICES.LIST, page, pageSize, searchTerm)
      const response: any = await api.get<Service[] | any>(endpoint)
      const val = response?.value ?? response?.data ?? response
      if (Array.isArray(val)) return val
      if (Array.isArray(val?.value)) return val.value
      if (Array.isArray(val?.items)) return val.items
      return []
    } catch (error) {
      console.error('Get services error:', error)
      throw error
    }
  }
  //#endregion

  //#region GET BY ID - Tekil Getirme
  const getServiceById = async (id: string) => {
    try {
      const response = await api.get<Service>(API_ENDPOINTS.SERVICES.GET_BY_ID(id))
      return response.value
    } catch (error) {
      console.error('Get service error:', error)
      throw error
    }
  }
  //#endregion

  //#region POST - Oluşturma
  const createService = async (serviceData: CreateServiceRequest) => {
    try {
      const response = await api.post<Service>(API_ENDPOINTS.SERVICES.CREATE, serviceData)
      return response.value
    } catch (error) {
      console.error('Create service error:', error)
      throw error
    }
  }
  //#endregion

  //#region PUT - Güncelleme
  const updateService = async (id: string, serviceData: UpdateServiceRequest) => {
    try {
      const response = await api.put<Service>(API_ENDPOINTS.SERVICES.UPDATE, serviceData)
      if (response.success) {
        return response.value
      }
    } catch (error) {
      console.error('Update service error:', error)
      throw error
    }
  }
  //#endregion

  //#region DELETE - Silme
  const deleteService = async (id: string) => {
    try {
      await api.delete(API_ENDPOINTS.SERVICES.DELETE(id))
      return true
    } catch (error) {
      console.error('Delete service error:', error)
      throw error
    }
  }
  //#endregion

  return {
    getServices,
    getServiceById,
    createService,
    updateService,
    deleteService,
  }
}
```

> **Not:** Eğer FormData (resim yükleme vs.) gerekiyorsa, `useNuxtApp().$api.post()` ile `multipart/form-data` header'ı kullanın.

---

### ADIM 4: Form Bileşeni Oluşturma (`components/`)

`components/Services/ServiceForm.vue` dosyası oluşturun:

```vue
<template>
  <v-form ref="formRef" @submit.prevent="submitForm">
    <v-card flat>
      <v-card-text>
        <!-- Form alanları -->
        <v-text-field
          v-model="formData.name"
          label="Hizmet Adı"
          :rules="[rules.required]"
          variant="outlined"
          density="comfortable"
        />
        
        <v-textarea
          v-model="formData.description"
          label="Açıklama"
          variant="outlined"
          density="comfortable"
          rows="3"
        />

        <v-text-field
          v-model.number="formData.price"
          label="Fiyat"
          type="number"
          :rules="[rules.required]"
          variant="outlined"
          density="comfortable"
        />

        <v-switch
          v-model="formData.isActive"
          label="Aktif"
          color="primary"
        />
      </v-card-text>

      <v-card-actions class="px-4 pb-4">
        <v-spacer />
        <v-btn variant="outlined" @click="$emit('cancel')">İptal</v-btn>
        <v-btn color="primary" type="submit" :loading="loading">
          {{ service ? 'Güncelle' : 'Oluştur' }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { Service } from '~/types'

const props = defineProps<{
  service?: Service | null
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [data: any]
  cancel: []
}>()

const formRef = ref()

const formData = ref({
  name: '',
  description: '',
  price: 0,
  isActive: true,
})

const rules = {
  required: (v: any) => !!v || 'Bu alan zorunludur',
}

watch(() => props.service, (newVal) => {
  if (newVal) {
    formData.value = {
      name: newVal.name || '',
      description: newVal.description || '',
      price: newVal.price || 0,
      isActive: newVal.isActive ?? true,
    }
  }
}, { immediate: true })

const submitForm = async () => {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  emit('submit', {
    data: { ...formData.value }
  })
}
</script>
```

---

### ADIM 5: Liste Sayfası Oluşturma (`pages/`)

`pages/services/index.vue` dosyası oluşturun:

```vue
<template>
  <!-- Breadcrumb -->
  <div class="mb-6">
    <BreadCrumb :items="[
      { text: 'Ana Sayfa', to: '/' },
      { text: 'Hizmetler' }
    ]" />
  </div>

  <BaseDataTable
    :items="items"
    :columns="tableColumns"
    title="Hizmetler"
    toolbar-icon="mdi-room-service"
    search-placeholder="Hizmet ara..."
    add-button-text="Yeni Hizmet Ekle"
    :loading="isLoading"
    loading-text="Hizmetler yükleniyor..."
    empty-title="Hizmet bulunamadı"
    :show-add-button="true"
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
    @refresh="refreshData"
  >
    <!-- Özel hücre render'ları (isteğe bağlı) -->
    <template #cell-isActive="{ value }">
      <v-chip
        :color="value ? 'success' : 'error'"
        size="small"
        variant="tonal"
      >
        {{ value ? 'Aktif' : 'Pasif' }}
      </v-chip>
    </template>

    <template #cell-price="{ value }">
      <span class="font-weight-bold">{{ value }} ₺</span>
    </template>
  </BaseDataTable>

  <!-- Create/Edit Dialog -->
  <ResizableDrawer
    v-model="dialogs.create"
    :title="isEditMode ? 'Hizmet Düzenle' : 'Yeni Hizmet Ekle'"
    icon="mdi-room-service"
    :default-width="600"
    :min-width="400"
  >
    <ServiceForm
      :service="selectedItem"
      :loading="isLoading"
      @submit="handleSubmit"
      @cancel="closeCreateDialog"
    />
  </ResizableDrawer>

  <!-- Confirm Delete Dialog -->
  <ConfirmDialog
    v-model="dialogs.delete"
    title="Hizmeti Sil"
    :message="`'${itemToDelete?.name}' hizmetini silmek istediğinizden emin misiniz?`"
    type="error"
    confirm-text="Sil"
    :loading="isDeleting"
    @confirm="confirmDelete"
    @cancel="closeDeleteDialog"
  />
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'
import ServiceForm from '~/components/Services/ServiceForm.vue'
import ConfirmDialog from '~/components/UI/ConfirmDialog.vue'

//#region Page Metadata
definePageMeta({
  title: 'Hizmetler',
  requiresAuth: true,
  permissions: ['services.read']
})

useHead({
  title: 'Hizmetler',
})
//#endregion

//#region DataTable Columns
const tableColumns = [
  {
    label: 'Hizmet Adı',
    key: 'name',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '250px'
  },
  {
    label: 'Açıklama',
    key: 'description',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '300px'
  },
  {
    label: 'Fiyat',
    key: 'price',
    sortable: true,
    width: '120px'
  },
  {
    label: 'Durum',
    key: 'isActive',
    sortable: true,
    filterable: true,
    filterType: 'select',
    width: '100px'
  },
]
//#endregion

//#region Composables
const { getServices, createService, updateService, deleteService, getServiceById } = useServices()

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
  loadItemsData,
  refreshData
} = useCrudOperations({
  loadItems: getServices,
  createItem: createService,
  updateItem: updateService,
  deleteItem: deleteService,
  loadByIdItem: getServiceById,
  itemName: 'Hizmet'
})
//#endregion

//#region Lifecycle
onMounted(async () => {
  await Promise.all([loadItemsData()])
})
//#endregion
</script>
```

---

### ADIM 6: Navigasyona Ekleme (`composables/useNavigation.ts`)

`navigationItems` array'ine yeni menü öğesini ekleyin:

```typescript
{
  title: 'Hizmetler',
  icon: 'mdi-room-service',
  to: '/services',
  permission: 'Services.Read'
}
```

Uygun parent grubun `children` array'ine yerleştirin, örneğin:

```typescript
{
  title: 'Tanımlamalar',
  icon: 'mdi-cog-outline',
  children: [
    // ... mevcut öğeler
    {
      title: 'Hizmetler',
      icon: 'mdi-room-service',
      to: '/services',
      permission: 'Services.Read'
    },
  ]
},
```

---

### ADIM 7 (Opsiyonel): Detay Sayfası (`pages/[entity]/[id].vue`)

Eğer entity'nin detay sayfası gerekiyorsa (`patients/[id].vue` gibi), `pages/services/[id].vue` oluşturun:

```vue
<template>
  <div>
    <!-- Breadcrumb -->
    <nav class="breadcrumb-nav">
      <ol class="breadcrumb">
        <li class="breadcrumb-item">
          <NuxtLink to="/" class="breadcrumb-link">Ana Sayfa</NuxtLink>
        </li>
        <li class="breadcrumb-item">
          <span class="separator">/</span>
          <NuxtLink to="/services" class="breadcrumb-link">Hizmetler</NuxtLink>
        </li>
        <li class="breadcrumb-item active">
          <span class="separator">/</span>
          <span class="current-page">{{ service?.name || 'Yükleniyor...' }}</span>
        </li>
      </ol>
    </nav>

    <!-- Loading State -->
    <div v-if="isLoading" class="loading-container">
      <v-progress-circular indeterminate color="primary" size="64" />
      <p>Hizmet bilgileri yükleniyor...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-container">
      <h2>Hata Oluştu</h2>
      <p>{{ error }}</p>
      <button class="btn btn-primary" @click="loadService">Tekrar Dene</button>
    </div>

    <!-- Ana İçerik -->
    <div v-else-if="service" class="service-container">
      <!-- Sayfa içeriği buraya -->
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { Service } from '~/types'

definePageMeta({
  title: 'Hizmet Detayı',
  requiresAuth: true,
  permissions: ['services.read']
})

const route = useRoute()
const router = useRouter()
const { getServiceById } = useServices()

const service = ref<Service | null>(null)
const isLoading = ref(true)
const error = ref<string | null>(null)

const loadService = async () => {
  try {
    isLoading.value = true
    error.value = null

    const id = route.params.id as string
    if (!id) throw new Error('Hizmet ID bulunamadı')

    const data = await getServiceById(id)
    if (!data) throw new Error('Hizmet bulunamadı')

    service.value = data
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Bilinmeyen bir hata oluştu'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => loadService())

useHead({
  title: computed(() => service.value ? `${service.value.name} - Hizmet Detayı` : 'Hizmet Detayı')
})
</script>
```

---

## Kontrol Listesi (Checklist)

Yeni bir sayfa oluştururken aşağıdaki adımları sırasıyla uygulayın:

| # | Adım | Dosya | Zorunlu |
|---|------|-------|---------|
| 1 | Tip tanımla | `types/[entity].ts` | Evet |
| 2 | Tipleri export et | `types/index.ts` | Evet |
| 3 | API endpoint ekle | `utils/apiEndpoints.ts` | Evet |
| 4 | Composable oluştur | `composables/use[Entity]s.ts` | Evet |
| 5 | Form bileşeni oluştur | `components/[Entity]s/[Entity]Form.vue` | Evet |
| 6 | Liste sayfası oluştur | `pages/[entity]s/index.vue` | Evet |
| 7 | Navigasyona ekle | `composables/useNavigation.ts` | Evet |
| 8 | Detay sayfası oluştur | `pages/[entity]s/[id].vue` | Hayır |

---

## Script Region Yapısı

Sayfa dosyalarında `<script setup>` bölümünü aşağıdaki region sıralamasıyla düzenleyin:

```typescript
//#region Page Metadata
definePageMeta({ ... })
useHead({ ... })
//#endregion

//#region DataTable Columns
const tableColumns = [ ... ]
//#endregion

//#region Composables
const { ... } = use[Entity]s()
const { ... } = useCrudOperations({ ... })
//#endregion

//#region Lifecycle
onMounted(async () => { ... })
//#endregion
```

---

## Template Yapısı

Sayfa template'ini aşağıdaki sırada yapılandırın:

```
1. BreadCrumb
2. BaseDataTable
   └── Özel hücre template slot'ları (#cell-[key])
3. ResizableDrawer (Create/Edit)
   └── [Entity]Form
4. ConfirmDialog (Delete)
```

---

## Kullanılan Ortak Bileşenler

| Bileşen | Kullanım | Import Yolu |
|---------|----------|-------------|
| `BaseDataTable` | Liste/tablo görünümü | `~/components/UI/BaseDataTable.vue` |
| `BreadCrumb` | Breadcrumb navigasyon | `~/components/BreadCrumb.vue` |
| `ResizableDrawer` | Yan panel (form açma) | `~/components/UI/ResizableDrawer.vue` |
| `ConfirmDialog` | Silme onay dialogu | `~/components/UI/ConfirmDialog.vue` |

---

## Kullanılan Ortak Composable'lar

| Composable | Açıklama |
|-----------|----------|
| `useCrudOperations()` | CRUD işlemleri + dialog yönetimi (en önemli) |
| `useDialogManager()` | Dialog aç/kapa durumlarını yönetir (useCrudOperations içinde otomatik) |
| `useApi()` | HTTP istekleri (get, post, put, delete) |
| `useValidators()` | Form doğrulama kuralları |

---

## BaseDataTable Kolon Konfigürasyonu

```typescript
{
  label: string,        // Kolon başlığı
  key: string,          // Data nesnesindeki alan adı
  sortable?: boolean,   // Sıralanabilir mi
  filterable?: boolean, // Filtrelenebilir mi
  filterType?: 'text' | 'select' | 'date' | 'single-date',
  width?: string,       // Kolon genişliği ('150px')
  align?: string,       // Hizalama
  formatter?: Function, // Değer formatlama fonksiyonu
  cellClass?: string,   // Hücre CSS class'ı
}
```

Özel hücre gösterimi için slot kullanın: `#cell-[key]="{ value, item }"`

---

## useCrudOperations Konfigürasyonu

```typescript
useCrudOperations({
  loadItems: getServices,        // Listeleme fonksiyonu
  createItem: createService,     // Oluşturma fonksiyonu
  updateItem: updateService,     // Güncelleme fonksiyonu
  deleteItem: deleteService,     // Silme fonksiyonu
  loadByIdItem: getServiceById,  // ID ile getirme fonksiyonu
  itemName: 'Hizmet',            // Toast mesajlarında kullanılacak isim
  // Server-side pagination için:
  serverSidePagination: true,    // Backend'den sayfalı veri çek
  initialPageSize: 10,           // Varsayılan sayfa boyutu
  initialData: asyncData         // SSR için useAsyncData sonucu
})
```

**Server-Side Pagination:** Backend `page`, `pageSize`, `searchTerm` parametreleriyle `{ items, totalCount, totalPages, pageNumber }` formatında yanıt dönmeli. `pages/users/index.vue` ve `pages/roles/index.vue` örnek alınabilir. Kullanımda:
- `getPaginatedEndpoint()` ile URL oluştur (search parametresi `searchTerm` olarak gönderilir)
- BaseDataTable'a `server-side-pagination`, `server-total-count`, `server-current-page` prop'ları ve `@page-change`, `@page-size-change` event'leri eklenmeli

**Döndürdüğü değerler:**

| Alan | Tip | Açıklama |
|------|-----|----------|
| `items` | `Ref<any[]>` | Liste verileri |
| `isLoading` | `Ref<boolean>` | Yükleme durumu |
| `isDeleting` | `Ref<boolean>` | Silme durumu |
| `dialogs` | `Ref<{create, view, edit, delete}>` | Dialog durumları |
| `selectedItem` | `Ref<any>` | Seçili öğe (edit/view) |
| `itemToDelete` | `Ref<any>` | Silinecek öğe |
| `isEditMode` | `Ref<boolean>` | Edit mi Create mi |
| `openCreateDialog()` | `Function` | Oluşturma dialogu aç |
| `openViewDialog(item)` | `Function` | Görüntüleme dialogu aç |
| `openEditDialog(item)` | `Function` | Düzenleme dialogu aç |
| `openDeleteDialog(item)` | `Function` | Silme dialogu aç |
| `closeCreateDialog()` | `Function` | Oluşturma dialogu kapat |
| `closeDeleteDialog()` | `Function` | Silme dialogu kapat |
| `handleSubmit(data)` | `Function` | Create/Update işlemi |
| `confirmDelete()` | `Function` | Silme onayı |
| `handleSearch(term)` | `Function` | Arama tetikle (server-side'da term parametresi) |
| `handlePageChange(page)` | `Function` | Sayfa değiştir (server-side) |
| `handlePageSizeChange(size)` | `Function` | Sayfa boyutu değiştir (server-side) |
| `loadItemsData()` | `Function` | Verileri yükle |
| `refreshData()` | `Function` | Verileri yenile |
| `currentPage`, `pageSize`, `totalCount`, `totalPages` | `Ref` | Pagination state (server-side) |

---

## Permission & Auth Sistemi

### definePageMeta ile Erişim Kontrolü

```typescript
definePageMeta({
  title: 'Sayfa Başlığı',
  requiresAuth: true,                    // Giriş gerekli
  permissions: ['services.read'],        // Gerekli izinler
  // veya
  middleware: ['auth', 'admin'],         // Middleware zinciri
  // veya
  roles: ['Admin', 'SuperAdmin'],       // Gerekli roller
})
```

### Middleware Seçenekleri

| Middleware | Açıklama |
|-----------|----------|
| `auth.global.ts` | Otomatik, tüm sayfalarda çalışır |
| `auth.ts` | Route seviyesinde auth kontrolü |
| `admin.ts` | Admin veya SuperAdmin rolü gerektirir |
| `super-admin.ts` | Sadece SuperAdmin rolü |
| `permission.ts` | meta.permission / meta.permissions kontrol eder |
| `guest.ts` | Giriş yapmış kullanıcıyı dashboard'a yönlendirir |

### Navigasyonda Permission

```typescript
{
  title: 'Hizmetler',
  icon: 'mdi-room-service',
  to: '/services',
  permission: 'Services.Read',    // Tek permission
  // veya
  roles: ['Admin', 'SuperAdmin'], // Role bazlı
}
```

---

## Sık Kullanılan Vuetify İkonları (mdi)

| İkon | Kullanım |
|------|---------|
| `mdi-account-multiple` | Kullanıcılar |
| `mdi-shield-account` | Roller |
| `mdi-domain` | Tenantlar |
| `mdi-key` | İzinler |
| `mdi-room-service` | Hizmet |
| `mdi-medical-bag` | Tedavi |
| `mdi-tooth-outline` | Diş |
| `mdi-gender-male` / `mdi-gender-female` | Cinsiyet |
| `mdi-check-circle` | Onay / Tamamlandı |
| `mdi-close-circle` | İptal |
| `mdi-pencil` | Düzenle |
| `mdi-delete` | Sil |
| `mdi-plus` | Ekle |
| `mdi-magnify` | Ara |

---

## FormData ile Resim Yükleme (Opsiyonel)

Eğer entity'de resim yükleme gerekiyorsa, composable'da:

```typescript
const createServiceWithImage = async (serviceData: CreateServiceRequest) => {
  try {
    const formData = new FormData()
    formData.append('name', serviceData.data.name)
    formData.append('description', serviceData.data.description || '')
    formData.append('price', serviceData.data.price.toString())

    if (serviceData.image && serviceData.image instanceof File) {
      formData.append('ProfileImage', serviceData.image)
    }

    const { $api } = useNuxtApp()
    const response = await $api.post(API_ENDPOINTS.SERVICES.CREATE, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return response
  } catch (error) {
    console.error('Create service error:', error)
    throw error
  }
}
```

Ve sayfa dosyasında image handler ekleyin:

```typescript
const handleUploadImage = async ({ id, file }: { id: number, file: File }) => {
  try {
    await uploadServiceImage(id, file)
    await refreshData()
  } catch (e) {
    console.error('Hizmet görsel yükleme hatası', e)
  }
}
```

---

## Özet Akış Diyagramı

```
1. types/service.ts          → Tip tanımla
2. types/index.ts            → Export et
3. utils/apiEndpoints.ts     → Endpoint ekle
4. composables/useServices.ts → CRUD fonksiyonları yaz
5. components/Services/       → Form bileşeni oluştur
6. pages/services/index.vue  → Liste sayfası oluştur
   ├── BreadCrumb
   ├── BaseDataTable + useCrudOperations
   ├── ResizableDrawer + ServiceForm
   └── ConfirmDialog
7. useNavigation.ts          → Menüye ekle
8. (opsiyonel) pages/services/[id].vue → Detay sayfası
```
