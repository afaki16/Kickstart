<template>
  <div>
    <NuxtRouteAnnouncer />
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
    <AppToast />
  </div>
</template>

<script setup lang="ts">
import AppToast from '~/components/UI/AppToast.vue'
</script>
