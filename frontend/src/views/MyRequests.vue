<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'
import { fetchRequests } from '../api'
import type { TimeOffRequest } from '../types'
import RequestForm from '../components/feature/requests/RequestForm.vue'
import RequestList from '../components/feature/requests/RequestList.vue'

const requests = ref<TimeOffRequest[]>([])
const loading = ref(false)
const { t } = useI18n()

async function loadRequests() {
  loading.value = true
  try {
    requests.value = await fetchRequests()
  } catch (error) {
    console.error('Failed to load requests:', error)
    message.error(t('requests.loadFailed'))
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
      <h2 class="page-title">{{ t('requests.pageTitle') }}</h2>
    </div>

    <a-card :title="t('requests.createCard')">
      <RequestForm @success="handleFormSuccess" />
    </a-card>

    <a-card :title="t('requests.listCard')" :loading="loading">
      <RequestList :requests="requests" @refresh="loadRequests" />
    </a-card>
  </div>
</template>
