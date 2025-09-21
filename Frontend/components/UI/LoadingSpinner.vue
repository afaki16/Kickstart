<template>
  <div class="loading-container" :class="containerClass">
    <v-progress-circular
      :size="size"
      :width="width"
      :color="color"
      indeterminate
    />
    <div v-if="text" class="loading-text mt-3">
      <p class="text-body-2 text-grey-600">{{ text }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
// Props
const props = withDefaults(defineProps<{
  size?: number
  width?: number
  color?: string
  text?: string
  overlay?: boolean
  fullPage?: boolean
}>(), {
  size: 40,
  width: 4,
  color: 'primary',
  text: '',
  overlay: false,
  fullPage: false
})

// Computed
const containerClass = computed(() => ({
  'loading-overlay': props.overlay,
  'loading-full-page': props.fullPage
}))
</script>

<style scoped>
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  z-index: 1000;
}

.loading-full-page {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.9);
  z-index: 9999;
}

.loading-text {
  text-align: center;
}
</style> 