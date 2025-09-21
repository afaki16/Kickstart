<template>
  <div class="empty-state text-center py-12">
    <v-icon
      :size="iconSize"
      :color="iconColor"
      class="mb-4"
    >
      {{ icon }}
    </v-icon>
    
    <h3 class="text-h5 mb-2">{{ title }}</h3>
    
    <p class="text-body-1 text-grey-600 mb-6">
      {{ description }}
    </p>
    
    <div v-if="$slots.actions">
      <slot name="actions" />
    </div>
    
    <v-btn
      v-else-if="actionText && actionTo"
      :color="actionColor"
      :to="actionTo"
      size="large"
    >
      <v-icon start>{{ actionIcon }}</v-icon>
      {{ actionText }}
    </v-btn>
    
    <v-btn
      v-else-if="actionText"
      :color="actionColor"
      size="large"
      @click="$emit('action')"
    >
      <v-icon start>{{ actionIcon }}</v-icon>
      {{ actionText }}
    </v-btn>
  </div>
</template>

<script setup lang="ts">
// Props
const props = withDefaults(defineProps<{
  icon?: string
  iconSize?: number
  iconColor?: string
  title: string
  description: string
  actionText?: string
  actionIcon?: string
  actionColor?: string
  actionTo?: string
}>(), {
  icon: 'mdi-inbox',
  iconSize: 80,
  iconColor: 'grey-400',
  actionIcon: 'mdi-plus',
  actionColor: 'primary'
})

// Emits
const emit = defineEmits<{
  action: []
}>()
</script> 