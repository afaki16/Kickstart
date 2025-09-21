<template>
  <div class="min-h-screen bg-gray-50">
    <v-container class="py-6">
      <v-row>
        <v-col cols="12">
          <h1 class="text-h4 font-weight-bold text-gray-900 mb-2">
            Dashboard
          </h1>
          <p class="text-body-1 text-gray-600">
            Hoş geldiniz! Bu dashboard sayfasıdır.
          </p>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script setup>
// Page metadata
definePageMeta({
  title: 'Dashboard',
  requiresAuth: true
})
</script>

<style scoped>
.container {
  max-width: 1200px;
}
</style>