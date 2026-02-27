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

const { handleSubmit, handleCancel } = useRequestWorkflow()

const detailsModalVisible = ref(false)
const selectedRequest = ref<TimeOffRequest | null>(null)

const columns = [
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
  {
    title: '操作',
    key: 'actions',
    width: 260,
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

async function handleRowCancel(requestId: string) {
  const success = await handleCancel(requestId)
  if (success) {
    emit('refresh')
  }
}
</script>

<template>
  <div>
    <a-table :columns="columns" :data-source="props.requests" row-key="id" :pagination="{ pageSize: 10 }" bordered>
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'status'">
          <a-tag :color="getRequestStatusColor(record.status)">
            {{ getRequestStatusLabel(record.status) }}
          </a-tag>
        </template>

        <template v-if="column.key === 'requestType'">
          {{ getRequestTypeLabel(record.requestType) }}
        </template>

        <template v-if="column.key === 'actions'">
          <div class="actions-row">
            <a-button v-if="[0, 5].includes(record.status)" size="small" type="primary"
              @click="handleRowSubmit(record.id)">
              送審
            </a-button>
            <a-button v-if="[0, 1, 5].includes(record.status)" size="small" @click="handleRowCancel(record.id)">
              取消
            </a-button>
            <a-button size="small" type="text" @click="openDetailsModal(record)">
              詳情
            </a-button>
          </div>
        </template>
      </template>
    </a-table>

    <!-- Details Modal -->
    <a-modal :open="detailsModalVisible" title="申請詳情" :footer="null" width="600px"
      @update:open="detailsModalVisible = $event">
      <a-descriptions :column="1" v-if="selectedRequest" bordered>
        <a-descriptions-item label="申請ID">
          {{ selectedRequest.id }}
        </a-descriptions-item>
        <a-descriptions-item label="員工編號">
          {{ selectedRequest.employeeId }}
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

  </div>
</template>
