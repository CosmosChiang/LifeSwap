<script setup lang="ts">
import { ref } from 'vue'
import { useRequestWorkflow } from '../../../composables/useRequestWorkflow'
import { getRequestTypeLabel, getRequestStatusLabel, getRequestStatusColor } from '../../../utils/enums'
import type { TimeOffRequest, RequestType } from '../../../types'

const props = defineProps<{
  requests: TimeOffRequest[]
}>()

const emit = defineEmits<{
  refresh: []
}>()

const { handleSubmit, handleApprove, handleReject, handleCancel } = useRequestWorkflow()

const detailsModalVisible = ref(false)
const selectedRequest = ref<TimeOffRequest | null>(null)
const approveModalVisible = ref(false)
const rejectModalVisible = ref(false)
const reviewerId = ref('M001')
const reviewComment = ref('')

const columns = [
  {
    title: '員工',
    dataIndex: 'employeeId',
    key: 'employeeId',
    width: 100,
  },
  {
    title: '部門',
    dataIndex: 'departmentCode',
    key: 'departmentCode',
    width: 100,
  },
  {
    title: '類型',
    dataIndex: 'requestType',
    key: 'requestType',
    width: 100,
    customRender: ({ text }: { text: RequestType }) => getRequestTypeLabel(text),
  },
  {
    title: '日期',
    dataIndex: 'requestDate',
    key: 'requestDate',
    width: 120,
  },
  {
    title: '狀態',
    dataIndex: 'status',
    key: 'status',
    width: 100,
    customRender: ({ text }: { text: number }) => ({
      children: getRequestStatusLabel(text),
      attrs: {
        status: getRequestStatusColor(text),
      },
    }),
  },
]

async function handleRowSubmit(requestId: string) {
  const success = await handleSubmit(requestId)
  if (success) {
    emit('refresh')
  }
}

function openDetailsModal(request: TimeOffRequest) {
  selectedRequest.value = request
  detailsModalVisible.value = true
}

function openApproveModal(request: TimeOffRequest) {
  selectedRequest.value = request
  approveModalVisible.value = true
}

function openRejectModal(request: TimeOffRequest) {
  selectedRequest.value = request
  rejectModalVisible.value = true
}

async function handleApproveSubmit() {
  if (!selectedRequest.value) return
  const success = await handleApprove(selectedRequest.value.id, reviewerId.value, reviewComment.value)
  if (success) {
    approveModalVisible.value = false
    reviewComment.value = ''
    emit('refresh')
  }
}

async function handleRejectSubmit() {
  if (!selectedRequest.value) return
  const success = await handleReject(selectedRequest.value.id, reviewerId.value, reviewComment.value)
  if (success) {
    rejectModalVisible.value = false
    reviewComment.value = ''
    emit('refresh')
  }
}

async function handleRowCancel(requestId: string) {
  const success = await handleCancel(requestId)
  if (success) {
    emit('refresh')
  }
}
</script>

<template>
  <div>
    <a-table
      :columns="columns"
      :data-source="props.requests"
      :row-key="(record: TimeOffRequest) => record.id"
      :pagination="{ pageSize: 10 }"
      bordered
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'status'">
          <a-tag :color="getRequestStatusColor((record as TimeOffRequest).status)">
            {{ getRequestStatusLabel((record as TimeOffRequest).status) }}
          </a-tag>
        </template>

        <template v-if="column.key === 'requestType'">
          {{ getRequestTypeLabel((record as TimeOffRequest).requestType) }}
        </template>

        <template v-if="!column.key">
          <div style="display: flex; gap: 8px; flex-wrap: wrap">
            <a-button
              v-if="(record as TimeOffRequest).status === 0"
              size="small"
              type="primary"
              @click="handleRowSubmit((record as TimeOffRequest).id)"
            >
              送審
            </a-button>
            <a-button
              v-if="(record as TimeOffRequest).status === 1"
              size="small"
              @click="openApproveModal(record as TimeOffRequest)"
            >
              核准
            </a-button>
            <a-button
              v-if="(record as TimeOffRequest).status === 1"
              size="small"
              danger
              @click="openRejectModal(record as TimeOffRequest)"
            >
              拒絕
            </a-button>
            <a-button
              v-if="[0, 1].includes((record as TimeOffRequest).status)"
              size="small"
              @click="handleRowCancel((record as TimeOffRequest).id)"
            >
              取消
            </a-button>
            <a-button
              size="small"
              type="text"
              @click="openDetailsModal(record as TimeOffRequest)"
            >
              詳情
            </a-button>
          </div>
        </template>
      </template>
    </a-table>

    <!-- Details Modal -->
    <a-modal
      v-model:visible="detailsModalVisible"
      title="申請詳情"
      :footer="null"
      width="600px"
    >
      <a-descriptions :column="1" v-if="selectedRequest" bordered>
        <a-descriptions-item label="申請ID">
          {{ selectedRequest.id }}
        </a-descriptions-item>
        <a-descriptions-item label="員工編號">
          {{ selectedRequest.employeeId }}
        </a-descriptions-item>
        <a-descriptions-item label="部門">
          {{ selectedRequest.departmentCode }}
        </a-descriptions-item>
        <a-descriptions-item label="類型">
          {{ getRequestTypeLabel(selectedRequest.requestType) }}
        </a-descriptions-item>
        <a-descriptions-item label="日期">
          {{ selectedRequest.requestDate }}
        </a-descriptions-item>
        <a-descriptions-item label="時間">
          {{ selectedRequest.startTime }} - {{ selectedRequest.endTime }}
        </a-descriptions-item>
        <a-descriptions-item label="原因">
          {{ selectedRequest.reason }}
        </a-descriptions-item>
        <a-descriptions-item label="狀態">
          <a-tag :color="getRequestStatusColor(selectedRequest.status)">
            {{ getRequestStatusLabel(selectedRequest.status) }}
          </a-tag>
        </a-descriptions-item>
      </a-descriptions>
    </a-modal>

    <!-- Approve Modal -->
    <a-modal
      v-model:visible="approveModalVisible"
      title="核准申請"
      ok-text="確認核准"
      cancel-text="取消"
      @ok="handleApproveSubmit"
    >
      <a-form layout="vertical">
        <a-form-item label="審核者編號">
          <a-input v-model:value="reviewerId" />
        </a-form-item>
        <a-form-item label="審核備註">
          <a-textarea v-model:value="reviewComment" :rows="4" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- Reject Modal -->
    <a-modal
      v-model:visible="rejectModalVisible"
      title="拒絕申請"
      ok-text="確認拒絕"
      cancel-text="取消"
      ok-button-danger
      @ok="handleRejectSubmit"
    >
      <a-form layout="vertical">
        <a-form-item label="審核者編號">
          <a-input v-model:value="reviewerId" />
        </a-form-item>
        <a-form-item label="拒絕原因（必填）">
          <a-textarea v-model:value="reviewComment" :rows="4" placeholder="請說明拒絕原因" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>
