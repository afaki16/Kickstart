<!-- components/UI/PageHeader.vue -->
<template>
  <div class="page-header">
    <div class="header-content">
      <div class="header-icon" :class="iconClass">
        <v-icon :size="iconSize" :color="iconColor">{{ icon }}</v-icon>
      </div>
      <div class="header-text">
        <h2 class="title">{{ title }}</h2>
        <p v-if="subtitle" class="subtitle">{{ subtitle }}</p>
      </div>
      <div v-if="$slots.actions" class="header-actions">
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title: string
  subtitle?: string
  icon?: string
  iconSize?: string | number
  iconColor?: string
  variant?: 'default' | 'primary' | 'success' | 'warning' | 'error'
}

const props = withDefaults(defineProps<Props>(), {
  subtitle: '',
  icon: 'mdi-information',
  iconSize: 32,
  iconColor: 'primary',
  variant: 'default'
})

const iconClass = computed(() => {
  return `header-icon-${props.variant}`
})
</script>

<style scoped>
.page-header {
  margin-bottom: 32px;
}

.header-content {
  display: flex;
  align-items: center;
  gap: 16px;
  background: white;
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.header-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  border-radius: 16px;
  color: white;
  flex-shrink: 0;
}

.header-icon-default {
  background: linear-gradient(135deg, #667eea 0%, #ffff 100%);
}

.header-icon-primary {
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
}

.header-icon-success {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
}

.header-icon-warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.header-icon-error {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
}

.header-text {
  flex: 1;
}

.title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.subtitle {
  color: #64748b;
  margin: 4px 0 0 0;
  font-size: 0.875rem;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-left: auto;
}

/* Responsive */
@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    text-align: center;
    gap: 12px;
  }
  
  .header-actions {
    margin-left: 0;
  }
}
</style>