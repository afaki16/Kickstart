<template>
  <Teleport to="body">
    <Transition name="drawer">
      <div v-if="modelValue" class="drawer-overlay">
        <div 
          class="drawer-container" 
          :style="{ width: isFullscreen ? '100vw' : drawerWidth + 'px' }"
          :class="{ 'fullscreen': isFullscreen, 'resizing': isResizingActive }"
          @click.stop
        >
          <!-- Resize Handle with Dots -->
          <div 
            v-if="!isFullscreen"
            class="resize-handle"
            @mousedown="startResize"
          >
            <div class="resize-dots">
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
            </div>
          </div>

          <div class="drawer-header">
            <h2 class="drawer-title">
              <v-icon v-if="icon" size="24" class="mr-2">{{ icon }}</v-icon>
              {{ title }}
            </h2>
            <div class="drawer-actions">
              <v-btn
                :icon="isFullscreen ? 'mdi-fullscreen-exit' : 'mdi-fullscreen'"
                variant="text"
                size="small"
                @click="toggleFullscreen"
                class="mr-2"
              ></v-btn>
              <v-btn
                icon="mdi-close"
                variant="text"
                size="small"
                @click="$emit('update:modelValue', false)"
              ></v-btn>
            </div>
          </div>

          <v-divider></v-divider>

          <div class="drawer-content">
            <slot></slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  title: {
    type: String,
    required: true
  },
  icon: {
    type: String,
    default: ''
  },
  defaultWidth: {
    type: Number,
    default: 600
  },
  minWidth: {
    type: Number,
    default: 400
  }
})

const emit = defineEmits(['update:modelValue'])

const drawerWidth = ref(props.defaultWidth)
const isFullscreen = ref(false)
const previousWidth = ref(props.defaultWidth)
const isResizingActive = ref(false)
const maxDrawerWidth = typeof window !== 'undefined' ? window.innerWidth * 0.9 : 1200

let isResizing = false
let startX = 0
let startWidth = 0

const toggleFullscreen = () => {
  if (!isFullscreen.value) {
    previousWidth.value = drawerWidth.value
    isFullscreen.value = true
  } else {
    isFullscreen.value = false
    drawerWidth.value = previousWidth.value
  }
}

const startResize = (e) => {
  isResizing = true
  isResizingActive.value = true
  startX = e.clientX
  startWidth = drawerWidth.value
  document.body.style.cursor = 'ew-resize'
  document.body.style.userSelect = 'none'
  
  document.addEventListener('mousemove', handleResize)
  document.addEventListener('mouseup', stopResize)
}

const handleResize = (e) => {
  if (!isResizing) return
  
  const diff = startX - e.clientX
  let newWidth = startWidth + diff
  
  newWidth = Math.max(props.minWidth, Math.min(newWidth, maxDrawerWidth))
  drawerWidth.value = newWidth
}

const stopResize = () => {
  isResizing = false
  isResizingActive.value = false
  document.body.style.cursor = ''
  document.body.style.userSelect = ''
  
  document.removeEventListener('mousemove', handleResize)
  document.removeEventListener('mouseup', stopResize)
}

const handleKeyDown = (e) => {
  if ((e.key === 'Escape' || e.keyCode === 27) && props.modelValue) {
    e.preventDefault()
    if (isFullscreen.value) {
      toggleFullscreen()
    } else {
      emit('update:modelValue', false)
    }
  }
}

watch(() => props.modelValue, (newValue) => {
  if (!newValue) {
    isFullscreen.value = false
  }
})

onMounted(() => {
  if (process.client) {
    document.addEventListener('keydown', handleKeyDown)
  }
})

onUnmounted(() => {
  if (process.client) {
    document.removeEventListener('mousemove', handleResize)
    document.removeEventListener('mouseup', stopResize)
    document.removeEventListener('keydown', handleKeyDown)
  }
})
</script>

<style scoped>
.drawer-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 9999;
  display: flex;
  justify-content: flex-end;
}

.drawer-container {
  max-width: 90vw;
  min-width: 400px;
  height: 100vh;
  background: white;
  box-shadow: -4px 0 16px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  position: relative;
  transition: width 0.3s ease;
}

.drawer-container.resizing {
  transition: none;
}

.drawer-container.fullscreen {
  max-width: 100vw;
  min-width: 100vw;
  box-shadow: none;
}

.resize-handle {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 10px;
  cursor: ew-resize;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s ease;
}

.resize-handle:hover {
  background: rgba(0, 0, 0, 0.02);
}

.resize-dots {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 3px;
  padding: 2px;
}

.dot {
  width: 3px;
  height: 3px;
  background: #9ca3af;
  border-radius: 50%;
  transition: all 0.2s ease;
}

.resize-handle:hover .dot {
  background: #6b7280;
  transform: scale(1.2);
}

.resize-handle:active .dot {
  background: var(--theme-primary, #2563eb);
}

.drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  background: #f5f5f5;
  flex-shrink: 0;
}

.drawer-title {
  display: flex;
  align-items: center;
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0;
}

.drawer-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.drawer-content {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
}

.drawer-content::-webkit-scrollbar {
  width: 8px;
}

.drawer-content::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.drawer-content::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 4px;
}

.drawer-content::-webkit-scrollbar-thumb:hover {
  background: #555;
}

.drawer-enter-active,
.drawer-leave-active {
  transition: opacity 0.3s ease;
}

.drawer-enter-active .drawer-container,
.drawer-leave-active .drawer-container {
  transition: transform 0.3s ease;
}

.drawer-enter-from,
.drawer-leave-to {
  opacity: 0;
}

.drawer-enter-from .drawer-container {
  transform: translateX(100%);
}

.drawer-leave-to .drawer-container {
  transform: translateX(100%);
}
</style>
