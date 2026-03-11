<template>
  <SplashScreen v-if="showSplash" @complete="onSplashComplete" />

  <template v-else-if="!isLoading">
    <component
      :is="loginComponent"
      :show-splash="false"
    />
  </template>

  <div v-else class="login-loading">
    <v-progress-circular indeterminate color="primary" size="48" />
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false,
  middleware: 'guest'
})

const showSplash = ref(true)
const onSplashComplete = () => { showSplash.value = false }

const { loadAppData, isLoading, appData } = useAppData()

const loginPage = computed(() => appData?.value?.tenant?.loginPage ?? 'default')

const loginComponent = computed(() => {
  const page = loginPage.value
  if (page === 'default') return resolveComponent('AuthLoginDefault')
  const name = 'AuthLogin' + page.charAt(0).toUpperCase() + page.slice(1)
  try {
    return resolveComponent(name)
  } catch {
    return resolveComponent('AuthLoginDefault')
  }
})

onMounted(async () => {
  await loadAppData()
})

useHead({
  title: 'Kickstart - Authentication System',
  meta: [
    { name: 'description', content: 'Sign in to your Kickstart account' }
  ]
})
</script>

<style scoped>
.login-loading {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--theme-gradient-sidebar, #14532d);
}
</style>
