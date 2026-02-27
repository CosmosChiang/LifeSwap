<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { fetchRequests } from '../api'
import type { TimeOffRequest } from '../types'
import RequestForm from '../components/feature/requests/RequestForm.vue'
import RequestList from '../components/feature/requests/RequestList.vue'

const requests = ref<TimeOffRequest[]>([])
const loading = ref(false)

async function loadRequests() {
  loading.value = true
  try {
    requests.value = await fetchRequests()
  } catch (error) {
    console.error('Failed to load requests:', error)
  } finally {
    loading.value = false
  }
}

async function handleFormSuccess() {
  await loadRequests()
}

onMounted(() => {
  loadRequests()
})
</script>

<template>
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">我的申請</h2>
    </div>

    <a-card title="建立申請">
      <RequestForm @success="handleFormSuccess" />
    </a-card>

    <a-card title="我的申請" :loading="loading">
      <RequestList :requests="requests" @refresh="loadRequests" />
    </a-card>
  </div>
</template>
