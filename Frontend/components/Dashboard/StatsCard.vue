<template>
  <v-card
    class="stats-card"
    :color="color"
    variant="flat"
    @click="$emit('click')"
  >
    <v-card-text>
      <div class="d-flex align-center">
        <div class="flex-grow-1">
          <p class="text-caption text-white text-uppercase mb-1 font-weight-medium">
            {{ title }}
          </p>
          <h2 class="text-h4 font-weight-bold text-white mb-0">
            {{ formattedValue }}
          </h2>
          <div v-if="showTrend" class="d-flex align-center mt-2">
            <v-icon
              :color="trendColor"
              size="16"
              class="me-1"
            >
              {{ trendIcon }}
            </v-icon>
            <span class="text-caption text-white">
              {{ Math.abs(change) }}% {{ trendText }}
            </span>
          </div>
        </div>
        <v-avatar
          :size="iconSize"
          color="rgba(255,255,255,0.2)"
        >
          <v-icon color="white" :size="iconSize * 0.5">
            {{ icon }}
          </v-icon>
        </v-avatar>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
// Props
const props = withDefaults(defineProps<{
  title: string
  value: number | string
  icon: string
  color?: string
  change?: number
  showTrend?: boolean
  iconSize?: number
  formatType?: 'number' | 'currency' | 'percentage'
}>(), {
  color: 'primary',
  change: 0,
  showTrend: true,
  iconSize: 56,
  formatType: 'number'
})

// Emits
const emit = defineEmits<{
  click: []
}>()

// Computed
const formattedValue = computed(() => {
  if (typeof props.value === 'string') return props.value
  
  switch (props.formatType) {
    case 'currency':
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(props.value)
    case 'percentage':
      return `${props.value}%`
    default:
      return new Intl.NumberFormat('en-US').format(props.value)
  }
})

const trendColor = computed(() => {
  return props.change >= 0 ? 'success' : 'error'
})

const trendIcon = computed(() => {
  return props.change >= 0 ? 'mdi-trending-up' : 'mdi-trending-down'
})

const trendText = computed(() => {
  if (props.change === 0) return 'no change'
  return props.change > 0 ? 'increase' : 'decrease'
})
</script>

<style scoped>
.stats-card {
  transition: all 0.2s ease-in-out;
  cursor: pointer;
}

.stats-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.15) !important;
}
</style> 