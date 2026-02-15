<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
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
  <div style="display: grid; gap: 24px">
    <!-- Statistics Cards -->
    <a-row :gutter="[16, 16]">
      <a-col :xs="24" :sm="12" :md="6">
        <a-statistic
          title="草稿"
          :value="draftCount"
          value-style="{ color: '#1890ff' }"
        >
          <template #prefix>
            <a-icon type="file-text" />
          </template>
        </a-statistic>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <a-statistic
          title="審核中"
          :value="submittedCount"
          value-style="{ color: '#faad14' }"
        >
          <template #prefix>
            <a-icon type="clock-circle" />
          </template>
        </a-statistic>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <a-statistic
          title="已核准"
          :value="approvedCount"
          value-style="{ color: '#52c41a' }"
        >
          <template #prefix>
            <a-icon type="check-circle" />
          </template>
        </a-statistic>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <a-statistic
          title="已拒絕"
          :value="rejectedCount"
          value-style="{ color: '#f5222d' }"
        >
          <template #prefix>
            <a-icon type="close-circle" />
          </template>
        </a-statistic>
      </a-col>
    </a-row>

    <!-- Recent Requests Card -->
    <a-card title="最近申請" :loading="loading">
      <a-table
        :columns="columns"
        :data-source="recentRequests"
        :row-key="(record: TimeOffRequest) => record.id"
        :pagination="false"
        size="small"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'requestType'">
            {{ getRequestTypeLabel((record as TimeOffRequest).requestType) }}
          </template>
          <template v-if="column.key === 'status'">
            <a-tag color="blue">
              {{ getRequestStatusLabel((record as TimeOffRequest).status) }}
            </a-tag>
          </template>
        </template>
      </a-table>
    </a-card>
  </div>
</template>
