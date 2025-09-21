<template>
  <div class="modern-data-table">
    <!-- Header Section -->
    <div class="table-header">
      <div class="header-content">
        <!-- Title & Description -->
        <div class="header-left">
          <div class="title-section">
            <div class="title-wrapper">
              <div class="title-icon">
                <v-icon :icon="toolbarIcon" size="24" />
              </div>
              <div class="title-content">
                <h2 class="page-title">{{ title }}</h2>
                <p class="page-description">{{ description }}</p>
              </div>
            </div>
          </div>
        </div>
        <!-- Search -->
        <div class="header-right-search">
          <div class="search-input-wrapper">
            <svg class="search-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              type="text"
              v-model="searchQuery"
              :placeholder="searchPlaceholder"
              class="search-input"
              @input="handleSearch"
            />
            <button
              v-if="searchQuery"
              @click="clearSearch"
              class="clear-search-btn"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
        <!-- Actions Section -->
        <div class="header-right">
          <!-- Advanced Filters Toggle -->
          <button
            v-if="showAdvancedFilters"
            @click="toggleAdvancedFilters"
            class="filter-toggle-btn"
            :class="{ 'active': showFilters }"
          >
            <svg class="btn-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 2v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
            </svg>
            Filtreler
            <span v-if="activeFiltersCount" class="filter-count">{{ activeFiltersCount }}</span>
          </button>

          <!-- Export Button -->
          <button
            v-if="showExportButton"
            @click="$emit('export')"
            class="export-btn"
          >
            <svg class="btn-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            Dışa Aktar
          </button>

          <!-- Add Button -->
          <button
            v-if="canCreate"
            @click="$emit('add')"
            class="add-button"
          >
            <svg class="btn-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            {{ addButtonText }}
          </button>
        </div>
      </div>

      <!-- Advanced Filters Panel -->
      <div v-if="showAdvancedFilters && showFilters" class="filters-panel">
        <div class="filters-header">
          <h3 class="filters-title">Gelişmiş Filtreler</h3>
          <button @click="clearAllFilters" class="clear-filters-btn">
            Tümünü Temizle
          </button>
        </div>
        
        <div class="filters-grid">
          <div
            v-for="column in filterableColumns"
            :key="column.key"
            class="filter-item mb-3"
          >
            <!-- Text Filter -->
            <v-text-field
            :label="column.label"
              v-if="column.filterType === 'text' || !column.filterType"
              v-model="columnFilters[column.key]"
              :placeholder="`${column.label} filtrele...`"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              class="filter-input"
              @update:model-value="debouncedApplyFilters"
            />
            
            <!-- Select Filter -->
            <v-select
            :label="column.label"
              v-else-if="column.filterType === 'select'"
              v-model="columnFilters[column.key]"
              :items="getColumnSelectOptions(column.key)"
              item-title="title"
              item-value="value"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              placeholder="Tümü"
              class="filter-select"
              @update:model-value="applyFilters"
            />
            
            <!-- Date Range Filter -->
            <div v-else-if="column.filterType === 'date'" class="date-range-filter">
              <v-text-field
              :label="column.label"
                v-model="columnFilters[column.key + '_start']"
                type="date"
                variant="outlined"
                density="compact"
                hide-details
                 clearable
                class="filter-input date-input"
                @update:model-value="debouncedApplyFilters"
              />
              <span class="date-separator">-</span>
              <v-text-field
                v-model="columnFilters[column.key + '_end']"
                type="date"
                variant="outlined"
                density="compact"
                hide-details
                class="filter-input date-input"
                @update:model-value="debouncedApplyFilters"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
 
    <!-- Table Container -->
    <div class="table-container">
      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <div class="loading-spinner">
          <div class="spinner"></div>
        </div>
        <p class="loading-text">{{ loadingText }}</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredItems.length === 0" class="empty-state">
        <div class="empty-icon">
          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h3 class="empty-title">{{ emptyTitle }}</h3>
        <p class="empty-description">{{ emptyDescription }}</p>
      </div>

      <!-- Data Table -->
      <div v-else class="table-wrapper">
        <div class="table-responsive">
          <table class="data-table">
            <thead class="table-header-section">
              <tr class="table-header-row">
                <th
                  v-for="column in columns"
                  :key="column.key"
                  class="table-header-cell"
                  :class="[
                    column.align || 'left',
                    { 'sortable': column.sortable !== false },
                    { 'sorted': sortBy === column.key }
                  ]"
                  :style="{ width: column.width }"
                  @click="column.sortable !== false ? handleSort(column.key) : null"
                >
                  <div class="header-cell-content">
                    <span class="header-text">{{ column.label }}</span>
                    <div v-if="column.sortable !== false" class="sort-indicators">
                      <svg
                        class="sort-icon sort-asc"
                        :class="{ 'active': sortBy === column.key && sortOrder === 'asc' }"
                        fill="none" viewBox="0 0 24 24" stroke="currentColor"
                      >
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
                      </svg>
                      <svg
                        class="sort-icon sort-desc"
                        :class="{ 'active': sortBy === column.key && sortOrder === 'desc' }"
                        fill="none" viewBox="0 0 24 24" stroke="currentColor"
                      >
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                      </svg>
                    </div>
                  </div>
                </th>
                <th v-if="showActions" class="table-header-cell actions-header">
                  <span class="header-text">İşlemler</span>
                </th>
              </tr>
            </thead>
            <tbody class="table-body">
              <tr
                v-for="(item, index) in paginatedItems"
                :key="item.id || index"
                class="table-row"
                @click="$emit('row-click', item)"
              >
                <td
                  v-for="column in columns"
                  :key="column.key"
                  class="table-cell"
                  :class="column.align || 'left'"
                >
                  <slot :name="`cell-${column.key}`" :item="item" :value="getNestedValue(item, column.key)">
                    <div class="cell-content">
                      <span class="cell-text" :class="column.cellClass">
                        {{ formatCellValue(getNestedValue(item, column.key), column) }}
                      </span>
                    </div>
                  </slot>
                </td>
                <td v-if="showActions" class="table-cell actions-cell">
                  <div class="action-buttons">
                    <slot name="actions" :item="item">
                      <div class="actions-inline">
                        <button
                          v-if="canRead"
                          @click="$emit('view', item)"
                          class="action-button view-button"
                          title="Görüntüle"
                        >
                          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                        </button>
                        <button
                          v-if="canUpdate"
                          @click="$emit('edit', item)"
                          class="action-button edit-button"
                          title="Düzenle"
                        >
                          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                        </button>
                        <button
                          v-if="canDelete"
                          @click="$emit('delete', item)"
                          class="action-button delete-button"
                          title="Sil"
                        >
                          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                        </button>
                      </div>
                    </slot>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="pagination">
          <div class="pagination-info">
            <div class="results-info">
              <span class="results-text">Result per page</span>
              <select v-model="itemsPerPageLocal" @change="changeItemsPerPage" class="items-per-page-select">
                <option :value="5">5</option>
                <option :value="10">10</option>
                <option :value="25">25</option>
                <option :value="50">50</option>
                <option :value="100">100</option>
              </select>
              <span class="total-count">{{ (currentPage - 1) * itemsPerPageLocal + 1 }}-{{ Math.min(currentPage * itemsPerPageLocal, totalItems) }} of {{ totalItems }}</span>
            </div>
          </div>
          <div class="pagination-controls">
            <button
              @click="goToPage(1)"
              :disabled="currentPage === 1"
              class="pagination-button"
              title="İlk sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
              </svg>
            </button>
            
            <button
              @click="goToPage(currentPage - 1)"
              :disabled="currentPage === 1"
              class="pagination-button"
              title="Önceki sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            
            <button
              v-for="page in visiblePages"
              :key="page"
              @click="goToPage(page)"
              class="pagination-button page-button"
              :class="{ 'active': page === currentPage }"
            >
              {{ page }}
            </button>
            
            <button
              @click="goToPage(currentPage + 1)"
              :disabled="currentPage === totalPages"
              class="pagination-button"
              title="Sonraki sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
            
            <button
              @click="goToPage(totalPages)"
              :disabled="currentPage === totalPages"
              class="pagination-button"
              title="Son sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 5l7 7-7 7M5 5l7 7-7 7" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
// Props
const props = defineProps({
  // Data
  items: {
    type: Array,
    default: () => []
  },
  columns: {
    type: Array,
    required: true
  },
  
  // Display options
  title: {
    type: String,
    default: 'Veri Tablosu'
  },
  description: {
    type: String,
    default: ''
  },
  toolbarIcon: {
    type: String,
    default: 'mdi-table'
  },
  searchPlaceholder: {
    type: String,
    default: 'Tabloda ara...'
  },
  addButtonText: {
    type: String,
    default: 'Yeni Ekle'
  },
  
  // Loading and empty states
  loading: {
    type: Boolean,
    default: false
  },
  loadingText: {
    type: String,
    default: 'Veriler yükleniyor...'
  },
  emptyTitle: {
    type: String,
    default: 'Veri bulunamadı'
  },
  emptyDescription: {
    type: String,
    default: 'Henüz hiç veri eklenmemiş veya arama kriterlerinize uygun veri bulunamadı.'
  },
  
  // Visibility controls
  showAddButton: {
    type: Boolean,
    default: true
  },
  showExportButton: {
    type: Boolean,
    default: false
  },
  showAdvancedFilters: {
    type: Boolean,
    default: true
  },
  showActions: {
    type: Boolean,
    default: true
  },
  showViewButton: {
    type: Boolean,
    default: false
  },
  showEditButton: {
    type: Boolean,
    default: false
  },
  showDeleteButton: {
    type: Boolean,
    default: false
  },
  showPagination: {
    type: Boolean,
    default: true
  },
  
  // Pagination
  itemsPerPage: {
    type: Number,
    default: 10
  }
})

// Emits
const emit = defineEmits([
  'add', 'view', 'edit', 'delete', 'export', 'search', 'sort', 'row-click', 'filter'
])

// Composables
const { hasPermission } = useAuth()
const route = useRoute()

// Auto-detect resource from route path
const getResourceFromRoute = () => {
  const path = route.path
  // Extract resource from path like /users -> users, /roles -> roles
  const segments = path.split('/').filter(Boolean)
  return segments[0] || 'unknown'
}

// Auto-generate permission names based on resource
const getPermissionName = (action) => {
  const resource = getResourceFromRoute()
  // Convert to PascalCase and add action
  const resourceName = resource.charAt(0).toUpperCase() + resource.slice(1)
  return `${resourceName}.${action}`
}

// Permission control functions - Auto-detect from route
const canCreate = computed(() => {
  if (!props.showAddButton) return false
  const permission = getPermissionName('Create')
  return hasPermission(permission)
})

const canRead = computed(() => {
  if (!props.showViewButton) return false
  const permission = getPermissionName('Read')
  return hasPermission(permission)
})

const canUpdate = computed(() => {
  if (!props.showEditButton) return false
  const permission = getPermissionName('Update')
  return hasPermission(permission)
})

const canDelete = computed(() => {
  if (!props.showDeleteButton) return false
  const permission = getPermissionName('Delete')
  return hasPermission(permission)
})

// Reactive data
const searchQuery = ref('')
const sortBy = ref('')
const sortOrder = ref('asc')
const currentPage = ref(1)
const itemsPerPageLocal = ref(props.itemsPerPage)
const showFilters = ref(false)
const columnFilters = ref({})

// Computed properties
const filterableColumns = computed(() => {
  return props.columns.filter(col => col.filterable !== false)
})

const activeFiltersCount = computed(() => {
  return Object.values(columnFilters.value).filter(val => val && val.toString().trim()).length
})

const filteredItems = computed(() => {
  let filtered = [...props.items]
  
  // Global search filtering
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(item => {
      return props.columns.some(column => {
        const value = getNestedValue(item, column.key)
        return value && value.toString().toLowerCase().includes(query)
      })
    })
  }
  
  // Column-specific filtering
  Object.entries(columnFilters.value).forEach(([key, value]) => {
    if (value && value.toString().trim()) {
      if (key.endsWith('_start') || key.endsWith('_end')) {
        // Date range filtering logic
        const baseKey = key.replace('_start', '').replace('_end', '')
        const startDate = columnFilters.value[baseKey + '_start']
        const endDate = columnFilters.value[baseKey + '_end']
        
        if (startDate || endDate) {
          filtered = filtered.filter(item => {
            const itemDate = new Date(getNestedValue(item, baseKey))
            if (startDate && itemDate < new Date(startDate)) return false
            if (endDate && itemDate > new Date(endDate)) return false
            return true
          })
        }
      } else {
        const column = props.columns.find(col => col.key === key)
        if (column && column.filterType === 'select') {
          filtered = filtered.filter(item => {
            const itemValue = getNestedValue(item, key)
            return itemValue === value
          })
        } else {
          filtered = filtered.filter(item => {
            const itemValue = getNestedValue(item, key)
            return itemValue && itemValue.toString().toLowerCase().includes(value.toLowerCase())
          })
        }
      }
    }
  })
  
  // Sorting
  if (sortBy.value) {
    filtered.sort((a, b) => {
      const aValue = getNestedValue(a, sortBy.value)
      const bValue = getNestedValue(b, sortBy.value)
      
      if (aValue === bValue) return 0
      
      const comparison = aValue < bValue ? -1 : 1
      return sortOrder.value === 'asc' ? comparison : -comparison
    })
  }
  
  return filtered
})

const totalItems = computed(() => filteredItems.value.length)
const totalPages = computed(() => Math.ceil(totalItems.value / itemsPerPageLocal.value))

const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPageLocal.value
  const end = start + itemsPerPageLocal.value
  return filteredItems.value.slice(start, end)
})

const paginationInfo = computed(() => {
  return `Toplam Kayıt : ${totalItems.value}`
})

const visiblePages = computed(() => {
  const pages = []
  const maxVisible = 5
  let start = Math.max(1, currentPage.value - Math.floor(maxVisible / 2))
  let end = Math.min(totalPages.value, start + maxVisible - 1)
  
  if (end - start + 1 < maxVisible) {
    start = Math.max(1, end - maxVisible + 1)
  }
  
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  
  return pages
})

// Methods
const getNestedValue = (obj, path) => {
  return path.split('.').reduce((current, key) => {
    return current && current[key] !== undefined ? current[key] : ''
  }, obj)
}

const formatCellValue = (value, column) => {
  if (column.formatter && typeof column.formatter === 'function') {
    return column.formatter(value)
  }
  return value || '-'
}

const getColumnSelectOptions = (columnKey) => {
  const values = props.items
    .map(item => getNestedValue(item, columnKey))
    .filter(val => val !== null && val !== undefined && val !== '')
  const uniqueValues = [...new Set(values)]
  
  return uniqueValues.map(value => {
    // Özel değerler için metin dönüşümü
    let displayText = value.toString()
    
    // Boolean değerler için
    if (value === 0 || value === '0') {
      displayText = 'Pasif'
    } else if (value === 1 || value === '1') {
      displayText = 'Aktif'
    }
    return {
      title: displayText,
      value: value
    }
  })
}

// Debounced filter function to prevent excessive API calls
let filterTimeout = null
const debouncedApplyFilters = () => {
  if (filterTimeout) {
    clearTimeout(filterTimeout)
  }
  
  filterTimeout = setTimeout(() => {
    applyFilters()
  }, 500) // 500ms delay
}

const handleSearch = () => {
  currentPage.value = 1
}

const clearSearch = () => {
  searchQuery.value = ''
  handleSearch()
}

const handleSort = (key) => {
  if (sortBy.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = key
    sortOrder.value = 'asc'
  }
  
  currentPage.value = 1
  emit('sort', { key, order: sortOrder.value })
}

const toggleAdvancedFilters = () => {
  showFilters.value = !showFilters.value
}

const applyFilters = () => {
  currentPage.value = 1
  
  // Select filter'lar için değer dönüşümü yap
  const processedFilters = { ...columnFilters.value }
  
  // Her column için select filter varsa, display text'i gerçek değere çevir
  props.columns.forEach(column => {
    if (column.filterType === 'select' && processedFilters[column.key]) {
      const selectOptions = getColumnSelectOptions(column.key)
      const selectedOption = selectOptions.find(option => option.title === processedFilters[column.key])
      if (selectedOption) {
        processedFilters[column.key] = selectedOption.value
      }
    }
  })
  
  emit('filter', processedFilters)
}

const clearAllFilters = () => {
  columnFilters.value = {}
  applyFilters()
}

const goToPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
  }
}

const changeItemsPerPage = () => {
  currentPage.value = 1
}

// Watch for items changes
watch(() => props.items, () => {
  currentPage.value = 1
})

watch(() => props.itemsPerPage, (newValue) => {
  itemsPerPageLocal.value = newValue
})
</script>

<style scoped>
.modern-data-table {
  @apply bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden;
}

/* Header Section */
.table-header {
  @apply border-b border-gray-100;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
}

.header-content {
  @apply px-8 py-6 flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6;
}

.header-left {
  @apply flex-1;
}

.title-section {
  @apply flex items-start gap-4;
}

.title-wrapper {
  @apply flex items-center gap-4;
}

.title-icon {
  @apply w-12 h-12 rounded-xl flex items-center justify-center;
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: white;
  box-shadow: 0 8px 16px rgba(59, 130, 246, 0.3);
}

.title-content {
  @apply flex flex-col;
}

.page-title {
  @apply text-2xl font-bold text-gray-900 mb-1;
}

.page-description {
  @apply text-sm text-gray-600;
}

.header-right {
  @apply flex flex-wrap items-center gap-3;
}

.header-right-search {
  @apply flex flex-wrap items-center gap-9 pl-4;
}

/* Buttons */
.filter-toggle-btn,
.export-btn,
.add-button {
  @apply inline-flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-xl transition-all duration-200;
}

.filter-toggle-btn {
  @apply bg-white border border-gray-200 text-gray-700 hover:bg-gray-50 hover:border-gray-300 relative;
}

.filter-toggle-btn.active {
  @apply bg-blue-50 border-blue-200 text-blue-700;
}

.filter-count {
  @apply absolute -top-2 -right-2 bg-blue-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center;
}

.export-btn {
  @apply bg-gray-100 border border-gray-200 text-gray-700 hover:bg-gray-200;
}

.add-button {
  @apply bg-gradient-to-r from-blue-500 to-indigo-600 text-white hover:from-blue-600 hover:to-indigo-700 shadow-lg hover:shadow-xl;
}

.btn-icon {
  @apply w-4 h-4;
}

/* Search */
.search-container {
  @apply flex-1 lg:flex-none lg:min-w-[320px];
}

.search-input-wrapper {
  @apply relative;
}

.search-icon {
  @apply absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400;
}

.search-input {
  @apply w-full pl-10 pr-10 py-2.5 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white transition-colors placeholder-gray-400;
  border: 2px solid #9ca3af !important;
}

.clear-search-btn {
  @apply absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400 hover:text-gray-600 transition-colors;
}

/* Filters Panel */
.filters-panel {
  @apply border-t border-gray-100 bg-gray-50 p-6;
}

.filters-header {
  @apply flex items-center justify-between mb-4;
}

.filters-title {
  @apply text-lg font-semibold text-gray-900;
}

.clear-filters-btn {
  @apply text-sm text-blue-600 hover:text-blue-700 font-medium transition-colors;
}

.filters-grid {
  @apply grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4;
}

/* Filter Item Styles */
.filter-item {
  @apply flex flex-col mb-3;
}

.filter-label {
  @apply text-sm font-medium text-gray-700 mb-2;
}

.filter-input :deep(.v-field),
.filter-select :deep(.v-field) {
  @apply bg-white border border-gray-200 rounded-lg transition-colors;
}



.date-range-filter {
  @apply flex items-center gap-2;
}

.date-input {
  @apply flex-1;
}

.date-separator {
  @apply text-gray-400 font-medium px-2;
}

/* Loading State */
.loading-state {
  @apply flex flex-col items-center justify-center py-16;
}

.loading-spinner {
  @apply mb-4;
}

.spinner {
  @apply animate-spin rounded-full h-12 w-12 border-4 border-gray-200 border-t-blue-600;
}

.loading-text {
  @apply text-gray-600 font-medium;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-16 px-6;
}

.empty-icon {
  @apply w-16 h-16 text-gray-300 mb-4;
}

.empty-title {
  @apply text-xl font-semibold text-gray-900 mb-2;
}

.empty-description {
  @apply text-gray-600 text-center mb-6 max-w-md;
}

.empty-add-button {
  @apply inline-flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-500 to-indigo-600 text-white font-medium rounded-xl hover:from-blue-600 hover:to-indigo-700 transition-all shadow-lg hover:shadow-xl;
}

/* Table */
.table-wrapper {
  @apply flex flex-col;
}

.table-responsive {
  @apply overflow-x-auto;
}

.data-table {
  @apply min-w-full;
}

.table-header-section {
  @apply bg-gradient-to-r from-gray-50 to-gray-100;
}

.table-header-row {
  @apply border-b border-gray-200;
}

.table-header-cell {
  @apply px-6 py-4 text-left font-semibold text-gray-900 whitespace-nowrap;
}

.table-header-cell.center {
  @apply text-center;
}

.table-header-cell.right {
  @apply text-right;
}

.table-header-cell.sortable {
  @apply cursor-pointer hover:bg-gray-100 transition-colors select-none;
}

.table-header-cell.sorted {
  @apply bg-blue-50 text-blue-700;
}

.header-cell-content {
  @apply flex items-center gap-2;
}

.header-cell-content.center {
  @apply justify-center;
}

.header-cell-content.right {
  @apply justify-end;
}

.header-text {
  @apply font-semibold;
}

.sort-indicators {
  @apply flex flex-col;
}

.sort-icon {
  @apply w-3 h-3 text-gray-400 transition-colors;
}

.sort-icon.active {
  @apply text-blue-600;
}

.sort-asc {
  @apply mb-0.5;
}

.actions-header {
  @apply text-center;
}

/* Table Body */
.table-body {
  @apply divide-y divide-gray-100;
}

.table-row {
  @apply hover:bg-gray-50 transition-colors cursor-pointer;
}

.table-cell {
  @apply px-6 py-4 whitespace-nowrap;
}

.table-cell.center {
  @apply text-center;
}

.table-cell.right {
  @apply text-right;
}

.cell-content {
  @apply flex items-center;
}

.cell-content.center {
  @apply justify-center;
}

.cell-content.right {
  @apply justify-end;
}

.cell-text {
  @apply text-gray-900 text-sm;
}

/* Actions */
.actions-cell {
  @apply text-center;
}

.action-buttons {
  @apply flex justify-center;
}

.actions-inline {
  @apply flex items-center gap-2;
}

.action-button {
  @apply p-2 rounded-lg transition-all duration-200 hover:scale-105;
}

.action-button svg {
  @apply w-4 h-4;
}

.view-button {
  @apply text-blue-600 hover:bg-blue-50 hover:text-blue-700;
}

.edit-button {
  @apply text-yellow-600 hover:bg-yellow-50 hover:text-yellow-700;
}

.delete-button {
  @apply text-red-600 hover:bg-red-50 hover:text-red-700;
}

/* Pagination */
.pagination {
  @apply flex flex-col sm:flex-row sm:items-center sm:justify-between px-6 py-4 border-t border-gray-100 bg-white;
}

.pagination-info {
  @apply mb-4 sm:mb-0;
}

.results-info {
  @apply flex items-center gap-3;
}

.results-text {
  @apply text-sm text-gray-900 ;
}

.items-per-page-select {
  @apply px-3 py-2 border border-gray-200 rounded-md text-sm bg-white text-gray-900 font-medium focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors;
}

.total-count {
  @apply text-sm text-gray-600 font-medium;
}

.pagination-controls {
  @apply flex items-center gap-1;
}

.pagination-button {
  @apply px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-white;
}

.pagination-button.page-button {
  @apply w-10 h-10 flex items-center justify-center;
}

.pagination-button.active {
  @apply text-white bg-blue-600 border-blue-600 hover:bg-blue-700;
}

.pagination-button svg {
  @apply w-4 h-4;
}

/* Responsive Design */
@media (max-width: 1024px) {
  .header-content {
    @apply flex-col items-stretch;
  }
  
  .header-right {
    @apply flex-col sm:flex-row;
  }
  
  .filters-grid {
    @apply grid-cols-1 sm:grid-cols-2;
  }
}

@media (max-width: 768px) {
  .modern-data-table {
    @apply rounded-xl;
  }
  
  .header-content {
    @apply px-4 py-4;
  }
  
  .filters-panel {
    @apply p-4;
  }
  
  .table-header-cell,
  .table-cell {
    @apply px-3 py-3;
  }
  
  .pagination {
    @apply px-4 py-3 flex-col items-stretch;
  }
  
  .pagination-controls {
    @apply justify-center;
  }
  
  .results-info {
    @apply flex-col sm:flex-row items-start sm:items-center text-center sm:text-left;
  }
}

@media (max-width: 640px) {
  .title-wrapper {
    @apply flex-col items-start;
  }
  
  .header-right {
    @apply flex-col;
  }
  
  .search-container {
    @apply min-w-full;
  }
  
  .filters-grid {
    @apply grid-cols-1;
  }
  
  .actions-menu {
    @apply right-auto left-0;
  }
}
</style>