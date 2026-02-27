<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { CheckCircleOutlined, CloseCircleOutlined, FileTextOutlined, HourglassOutlined } from '@ant-design/icons-vue'
import { fetchRequests } from '../api'
import type { TimeOffRequest } from '../types'
import { getRequestTypeLabel, getRequestStatusLabel } from '../utils/enums'

const requests = ref<TimeOffRequest[]>([])
const loading = ref(false)

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
    title: '申請ID',
    dataIndex: 'id',
    key: 'id',
    width: 150,
  },
  {
    title: '類型',
    dataIndex: 'requestType',
    key: 'requestType',
    width: 80,
  },
  {
    title: '日期',
    dataIndex: 'requestDate',
    key: 'requestDate',
    width: 100,
  },
  {
    title: '狀態',
    dataIndex: 'status',
    key: 'status',
    width: 100,
  },
]
</script>

<template>
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">首頁總覽</h2>
    </div>

    <a-row :gutter="[16, 16]">
      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">草稿</span>
            <span class="stat-tile-icon"><FileTextOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ draftCount }}</div>
        </div>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">審核中</span>
            <span class="stat-tile-icon"><HourglassOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ submittedCount }}</div>
        </div>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">已核准</span>
            <span class="stat-tile-icon"><CheckCircleOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ approvedCount }}</div>
        </div>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <div class="stat-tile">
          <div class="stat-tile-head">
            <span class="stat-tile-label">已拒絕</span>
            <span class="stat-tile-icon"><CloseCircleOutlined /></span>
          </div>
          <div class="stat-tile-value">{{ rejectedCount }}</div>
        </div>
      </a-col>
    </a-row>

    <!-- Recent Requests Card -->
    <a-card title="最近申請" :loading="loading">
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
