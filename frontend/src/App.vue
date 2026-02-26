<script setup lang="ts">
</script>

<template>
  <router-view v-slot="{ Component }">
    <component :is="Component" v-if="$route.name === 'Login'" />
    <AppLayout v-else>
      <component :is="Component" />
    </AppLayout>
  </router-view>
</template>
