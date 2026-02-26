<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { fetchRequests } from '../api'
import type { TimeOffRequest } from '../types'
import { getRequestTypeLabel } from '../utils/enums'
import { useRequestWorkflow } from '../composables/useRequestWorkflow'

const allRequests = ref<TimeOffRequest[]>([])
const loading = ref(false)
const reviewerId = ref('M001')
const reviewComment = ref('')
const approveModalVisible = ref(false)
const rejectModalVisible = ref(false)
const returnModalVisible = ref(false)
const selectedRequest = ref<TimeOffRequest | null>(null)

const { handleApprove, handleReject, handleReturn } = useRequestWorkflow()

// Only show submitted requests
const submittedRequests = computed(() =>
  allRequests.value.filter(r => r.status === 1)
)

async function loadRequests() {
  loading.value = true
  try {
    allRequests.value = await fetchRequests()
  } catch (error) {
    console.error('Failed to load requests:', error)
  } finally {
    loading.value = false
  }
}

const columns = [
  {
    title: '申請ID',
    dataIndex: 'id',
    key: 'id',
    width: 150,
  },
  {
    title: '員工',
    dataIndex: 'employeeId',
    key: 'employeeId',
    width: 100,
  },
  {
    title: '類型',
    dataIndex: 'requestType',
    key: 'requestType',
    width: 100,
  },
  {
    title: '日期',
    dataIndex: 'requestDate',
    key: 'requestDate',
    width: 120,
  },
  {
    title: '原因',
    dataIndex: 'reason',
    key: 'reason',
  },
  {
    title: '操作',
    key: 'actions',
    width: 220,
  },
]

function openApproveModal(request: TimeOffRequest) {
  selectedRequest.value = request
  approveModalVisible.value = true
}

function openRejectModal(request: TimeOffRequest) {
  selectedRequest.value = request
  rejectModalVisible.value = true
}

function openReturnModal(request: TimeOffRequest) {
  selectedRequest.value = request
  returnModalVisible.value = true
}

async function handleApproveSubmit() {
  if (!selectedRequest.value) return
  const success = await handleApprove(selectedRequest.value.id, reviewerId.value, reviewComment.value)
  if (success) {
    approveModalVisible.value = false
    reviewComment.value = ''
    await loadRequests()
  }
}

async function handleRejectSubmit() {
  if (!selectedRequest.value) return
  const success = await handleReject(selectedRequest.value.id, reviewerId.value, reviewComment.value)
  if (success) {
    rejectModalVisible.value = false
    reviewComment.value = ''
    await loadRequests()
  }
}

async function handleReturnSubmit() {
  if (!selectedRequest.value) return
  const success = await handleReturn(selectedRequest.value.id, reviewerId.value, reviewComment.value)
  if (success) {
    returnModalVisible.value = false
    reviewComment.value = ''
    await loadRequests()
  }
}

import { computed } from 'vue'

onMounted(() => {
  loadRequests()
})
</script>

<template>
  <div style="display: grid; gap: 24px">
    <a-card :title="`待審核申請 (${submittedRequests.length})`" :loading="loading">
      <a-table :columns="columns" :data-source="submittedRequests" row-key="id" :pagination="{ pageSize: 10 }"
        size="small" bordered>
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'requestType'">
            {{ getRequestTypeLabel(record.requestType) }}
          </template>

          <template v-if="column.key === 'actions'">
            <div style="display: flex; gap: 8px">
              <a-button size="small" type="primary" @click="openApproveModal(record)">
                核准
              </a-button>
              <a-button size="small" danger @click="openRejectModal(record)">
                拒絕
              </a-button>
              <a-button size="small" @click="openReturnModal(record)">
                退回
              </a-button>
            </div>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- Approve Modal -->
    <a-modal :open="approveModalVisible" title="核准申請" ok-text="確認核准" cancel-text="取消" @ok="handleApproveSubmit"
      @update:open="approveModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item label="審核者編號">
          <a-input :value="reviewerId" @update:value="reviewerId = $event" />
        </a-form-item>
        <a-form-item label="審核備註">
          <a-textarea :value="reviewComment" :rows="4" @update:value="reviewComment = $event" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- Reject Modal -->
    <a-modal :open="rejectModalVisible" title="拒絕申請" ok-text="確認拒絕" cancel-text="取消" ok-button-danger
      @ok="handleRejectSubmit" @update:open="rejectModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item label="審核者編號">
          <a-input :value="reviewerId" @update:value="reviewerId = $event" />
        </a-form-item>
        <a-form-item label="拒絕原因（必填）">
          <a-textarea :value="reviewComment" :rows="4" placeholder="請說明拒絕原因" @update:value="reviewComment = $event" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- Return Modal -->
    <a-modal :open="returnModalVisible" title="退回申請" ok-text="確認退回" cancel-text="取消" @ok="handleReturnSubmit"
      @update:open="returnModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item label="審核者編號">
          <a-input :value="reviewerId" @update:value="reviewerId = $event" />
        </a-form-item>
        <a-form-item label="退回說明（建議填寫）">
          <a-textarea :value="reviewComment" :rows="4" placeholder="請說明需補件或修正內容"
            @update:value="reviewComment = $event" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>
