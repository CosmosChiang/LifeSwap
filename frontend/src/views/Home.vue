<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { CheckCircleOutlined, CloseCircleOutlined, FileTextOutlined, HourglassOutlined } from '@ant-design/icons-vue'
import { useI18n } from 'vue-i18n'
import { fetchRequests } from '../api'
import type { TimeOffRequest } from '../types'
import { getRequestTypeLabel, getRequestStatusLabel } from '../utils/enums'

const requests = ref<TimeOffRequest[]>([])
const loading = ref(false)
const { t } = useI18n()

// Computed statistics
const draftCount = computed(() => requests.value.filter(r => r.status === 0).length)
const submittedCount = computed(() => requests.value.filter(r => r.status === 1).length)
const approvedCount = computed(() => requests.value.filter(r => r.status === 2).length)
const rejectedCount = computed(() => requests.value.filter(r => r.status === 3).length)
const recentRequests = computed(() => requests.value.slice(0, 5))

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

onMounted(() => {
  loadRequests()
})

const columns = [
  {
    title: t('home.requestId'),
    dataIndex: 'id',
    key: 'id',
    width: 150,
  },
  {
    title: t('home.requestType'),
    dataIndex: 'requestType',
    key: 'requestType',
    width: 80,
  },
  {
    title: t('home.requestDate'),
    dataIndex: 'requestDate',
    key: 'requestDate',
    width: 100,
  },
  {
    title: t('home.requestStatus'),
    dataIndex: 'status',
    key: 'status',
    width: 100,
  },
]
</script>

<template>
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">{{ t('home.title') }}</h2>
    </div>

    <a-row :gutter="[16, 16]">
      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">{{ t('home.draft') }}</span>
            <span class="stat-tile-icon"><FileTextOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ draftCount }}</div>
        </div>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">{{ t('home.submitted') }}</span>
            <span class="stat-tile-icon"><HourglassOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ submittedCount }}</div>
        </div>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">{{ t('home.approved') }}</span>
            <span class="stat-tile-icon"><CheckCircleOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ approvedCount }}</div>
        </div>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">{{ t('home.rejected') }}</span>
            <span class="stat-tile-icon"><CloseCircleOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ rejectedCount }}</div>
        </div>
      </a-col>
    </a-row>

    <!-- Recent Requests Card -->
    <a-card :title="t('home.recentRequests')" :loading="loading">
      <a-table
        :columns="columns"
        :data-source="recentRequests"
        row-key="id"
        :pagination="false"
        size="small"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'requestType'">
            {{ getRequestTypeLabel(record.requestType) }}
          </template>
          <template v-if="column.key === 'status'">
            <a-tag color="blue">
              {{ getRequestStatusLabel(record.status) }}
            </a-tag>
          </template>
        </template>
      </a-table>
    </a-card>
  </div>
</template>
