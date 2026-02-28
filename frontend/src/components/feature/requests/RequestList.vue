<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
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
const { t } = useI18n()

const detailsModalVisible = ref(false)
const selectedRequest = ref<TimeOffRequest | null>(null)

const columns = [
  {
    title: t('requestList.columns.employeeId'),
    dataIndex: 'employeeId',
    key: 'employeeId',
    width: 100,
  },
  {
    title: t('requestList.columns.requestType'),
    dataIndex: 'requestType',
    key: 'requestType',
    width: 100,
    customRender: ({ text }: { text: RequestType }) => getRequestTypeLabel(text),
  },
  {
    title: t('requestList.columns.requestDate'),
    dataIndex: 'requestDate',
    key: 'requestDate',
    width: 120,
  },
  {
    title: t('requestList.columns.status'),
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
    title: t('requestList.columns.actions'),
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
              {{ t('requestList.actions.submit') }}
            </a-button>
            <a-button v-if="[0, 1, 5].includes(record.status)" size="small" @click="handleRowCancel(record.id)">
              {{ t('requestList.actions.cancel') }}
            </a-button>
            <a-button size="small" type="text" @click="openDetailsModal(record)">
              {{ t('requestList.actions.details') }}
            </a-button>
          </div>
        </template>
      </template>
    </a-table>

    <!-- Details Modal -->
    <a-modal :open="detailsModalVisible" :title="t('requestList.details.title')" :footer="null" width="600px"
      @update:open="detailsModalVisible = $event">
      <a-descriptions :column="1" v-if="selectedRequest" bordered>
        <a-descriptions-item :label="t('requestList.details.id')">
          {{ selectedRequest.id }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.employeeId')">
          {{ selectedRequest.employeeId }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.applicant')">
          {{ selectedRequest.applicantName || '-' }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.requestType')">
          {{ getRequestTypeLabel(selectedRequest.requestType) }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.overtimeStartAt')">
          {{ selectedRequest.overtimeStartAt || '-' }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.overtimeEndAt')">
          {{ selectedRequest.overtimeEndAt || '-' }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.compTimeHours')">
          {{ selectedRequest.compTimeHours?.toFixed?.(2) ?? '0.00' }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.overtimeProject')">
          {{ selectedRequest.overtimeProject || '-' }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.overtimeContent')">
          {{ selectedRequest.overtimeContent || '-' }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.overtimeReason')">
          {{ selectedRequest.overtimeReason || selectedRequest.reason }}
        </a-descriptions-item>
        <a-descriptions-item :label="t('requestList.details.status')">
          <a-tag :color="getRequestStatusColor(selectedRequest.status)">
            {{ getRequestStatusLabel(selectedRequest.status) }}
          </a-tag>
        </a-descriptions-item>
      </a-descriptions>
    </a-modal>

  </div>
</template>
