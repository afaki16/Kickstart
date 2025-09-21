<template>
  <v-card class="quick-action-card h-100">
    <v-card-title class="d-flex align-center">
      <v-icon :color="iconColor" class="me-2">{{ icon }}</v-icon>
      {{ title }}
    </v-card-title>
    
    <v-card-text>
      <p v-if="description" class="text-body-2 text-grey-600 mb-4">
        {{ description }}
      </p>
      
      <div class="d-flex flex-column gap-2">
        <v-btn
          v-for="action in actions"
          :key="action.title"
          :color="action.color || 'primary'"
          :variant="action.variant || 'outlined'"
          :size="action.size || 'default'"
          :prepend-icon="action.icon"
          :to="action.to"
          class="text-none justify-start"
          @click="action.action"
        >
          {{ action.title }}
        </v-btn>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
// Types
interface QuickAction {
  title: string
  icon?: string
  color?: string
  variant?: 'flat' | 'text' | 'outlined' | 'plain'
  size?: 'x-small' | 'small' | 'default' | 'large' | 'x-large'
  to?: string
  action?: () => void
}

// Props
const props = defineProps<{
  title: string
  icon: string
  iconColor?: string
  description?: string
  actions: QuickAction[]
}>()
</script>

<style scoped>
.quick-action-card {
  transition: transform 0.2s ease-in-out;
}

.quick-action-card:hover {
  transform: translateY(-2px);
}

.gap-2 {
  gap: 0.5rem;
}
</style> 