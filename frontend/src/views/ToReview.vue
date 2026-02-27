<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
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
const { t } = useI18n()

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

const columns = computed(() => [
  {
    title: t('review.columns.id'),
    dataIndex: 'id',
    key: 'id',
    width: 150,
  },
  {
    title: t('review.columns.employeeId'),
    dataIndex: 'employeeId',
    key: 'employeeId',
    width: 100,
  },
  {
    title: t('review.columns.requestType'),
    dataIndex: 'requestType',
    key: 'requestType',
    width: 100,
  },
  {
    title: t('review.columns.requestDate'),
    dataIndex: 'requestDate',
    key: 'requestDate',
    width: 120,
  },
  {
    title: t('review.columns.reason'),
    dataIndex: 'reason',
    key: 'reason',
  },
  {
    title: t('review.columns.actions'),
    key: 'actions',
    width: 220,
  },
])

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

onMounted(() => {
  loadRequests()
})
</script>

<template>
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">{{ t('review.pageTitle') }}</h2>
    </div>

    <a-card :title="t('review.cardTitle', { count: submittedRequests.length })" :loading="loading">
      <a-table :columns="columns" :data-source="submittedRequests" row-key="id" :pagination="{ pageSize: 10 }"
        size="small" bordered>
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'requestType'">
            {{ getRequestTypeLabel(record.requestType) }}
          </template>

          <template v-if="column.key === 'actions'">
            <div class="actions-row">
              <a-button size="small" type="primary" @click="openApproveModal(record)">
                {{ t('review.actions.approve') }}
              </a-button>
              <a-button size="small" danger @click="openRejectModal(record)">
                {{ t('review.actions.reject') }}
              </a-button>
              <a-button size="small" @click="openReturnModal(record)">
                {{ t('review.actions.return') }}
              </a-button>
            </div>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- Approve Modal -->
    <a-modal :open="approveModalVisible" :title="t('review.modals.approve.title')" :ok-text="t('review.modals.approve.ok')" :cancel-text="t('common.cancel')" @ok="handleApproveSubmit"
      @update:open="approveModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item :label="t('review.modals.reviewerId')">
          <a-input :value="reviewerId" @update:value="reviewerId = $event" />
        </a-form-item>
        <a-form-item :label="t('review.modals.comment')">
          <a-textarea :value="reviewComment" :rows="4" @update:value="reviewComment = $event" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- Reject Modal -->
    <a-modal :open="rejectModalVisible" :title="t('review.modals.reject.title')" :ok-text="t('review.modals.reject.ok')" :cancel-text="t('common.cancel')" ok-button-danger
      @ok="handleRejectSubmit" @update:open="rejectModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item :label="t('review.modals.reviewerId')">
          <a-input :value="reviewerId" @update:value="reviewerId = $event" />
        </a-form-item>
        <a-form-item :label="t('review.modals.reject.reason')">
          <a-textarea :value="reviewComment" :rows="4" :placeholder="t('review.modals.reject.placeholder')" @update:value="reviewComment = $event" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- Return Modal -->
    <a-modal :open="returnModalVisible" :title="t('review.modals.return.title')" :ok-text="t('review.modals.return.ok')" :cancel-text="t('common.cancel')" @ok="handleReturnSubmit"
      @update:open="returnModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item :label="t('review.modals.reviewerId')">
          <a-input :value="reviewerId" @update:value="reviewerId = $event" />
        </a-form-item>
        <a-form-item :label="t('review.modals.return.reason')">
          <a-textarea :value="reviewComment" :rows="4" :placeholder="t('review.modals.return.placeholder')"
            @update:value="reviewComment = $event" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>
