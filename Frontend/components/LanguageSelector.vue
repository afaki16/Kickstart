<template>
  <div class="relative">
    <button
      @click="isOpen = !isOpen"
      class="flex items-center space-x-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
    >
      <span class="fi" :class="`fi-${currentLanguage.flag}`"></span>
      <span>{{ currentLanguage.name }}</span>
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
      </svg>
    </button>

    <div
      v-if="isOpen"
      class="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg z-50 border border-gray-200"
    >
      <div class="py-1">
        <button
          v-for="language in languages"
          :key="language.code"
          @click="selectLanguage(language)"
          class="flex items-center w-full px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
        >
          <span class="fi mr-3" :class="`fi-${language.flag}`"></span>
          <span>{{ language.name }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
const isOpen = ref(false)
const currentLanguage = ref({
  code: 'tr',
  name: 'Türkçe',
  flag: 'tr'
})

const languages = [
  { code: 'tr', name: 'Türkçe', flag: 'tr' },
  { code: 'en', name: 'English', flag: 'us' },
  { code: 'de', name: 'Deutsch', flag: 'de' },
  { code: 'fr', name: 'Français', flag: 'fr' }
]

const selectLanguage = (language) => {
  currentLanguage.value = language
  isOpen.value = false
  // Burada dil değiştirme işlemi yapılabilir
  console.log('Dil değiştirildi:', language.code)
}

// Dışarı tıklandığında menüyü kapat
onMounted(() => {
  document.addEventListener('click', (e) => {
    if (!e.target.closest('.relative')) {
      isOpen.value = false
    }
  })
})
</script>

<style scoped>
.fi {
  width: 1.2em;
  height: 1.2em;
  border-radius: 2px;
}
</style> 