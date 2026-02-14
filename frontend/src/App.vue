<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import {
  approveRequest,
  cancelRequest,
  createRequest,
  fetchRequests,
  rejectRequest,
  submitRequest,
} from './api'
import type { CreateRequestPayload, RequestType, TimeOffRequest } from './types'

const requests = ref<TimeOffRequest[]>([])
const loading = ref(false)
const message = ref('')
const errorMessage = ref('')

const form = reactive<CreateRequestPayload>({
  requestType: 0,
  employeeId: 'E001',
  requestDate: new Date().toISOString().slice(0, 10),
  startTime: '18:00',
  endTime: '20:00',
  reason: '',
})

const reviewerId = ref('M001')
const reviewComment = ref('')

function clearAlerts(): void {
  message.value = ''
  errorMessage.value = ''
}

function getRequestTypeLabel(value: RequestType): string {
  return value === 0 ? 'Overtime' : 'CompOff'
}

function getRequestStatusLabel(value: number): string {
  switch (value) {
    case 0:
      return 'Draft'
    case 1:
      return 'Submitted'
    case 2:
      return 'Approved'
    case 3:
      return 'Rejected'
    case 4:
      return 'Cancelled'
    default:
      return 'Unknown'
  }
}

async function loadRequests(): Promise<void> {
  loading.value = true
  clearAlerts()

  try {
    requests.value = await fetchRequests()
  } catch (error) {
    errorMessage.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

async function handleCreate(): Promise<void> {
  clearAlerts()

  if (!form.reason.trim()) {
    errorMessage.value = '請輸入申請原因。'
    return
  }

  try {
    await createRequest({
      ...form,
      reason: form.reason.trim(),
    })

    message.value = '申請草稿已建立。'
    form.reason = ''
    await loadRequests()
  } catch (error) {
    errorMessage.value = (error as Error).message
  }
}

async function handleSubmit(requestId: string): Promise<void> {
  clearAlerts()
  try {
    await submitRequest(requestId)
    message.value = '申請已送審。'
    await loadRequests()
  } catch (error) {
    errorMessage.value = (error as Error).message
  }
}

async function handleApprove(requestId: string): Promise<void> {
  clearAlerts()
  try {
    await approveRequest(requestId, {
      reviewerId: reviewerId.value,
      comment: reviewComment.value,
    })
    message.value = '申請已核准。'
    await loadRequests()
  } catch (error) {
    errorMessage.value = (error as Error).message
  }
}

async function handleReject(requestId: string): Promise<void> {
  clearAlerts()
  try {
    await rejectRequest(requestId, {
      reviewerId: reviewerId.value,
      comment: reviewComment.value,
    })
    message.value = '申請已拒絕。'
    await loadRequests()
  } catch (error) {
    errorMessage.value = (error as Error).message
  }
}

async function handleCancel(requestId: string): Promise<void> {
  clearAlerts()
  try {
    await cancelRequest(requestId)
    message.value = '申請已取消。'
    await loadRequests()
  } catch (error) {
    errorMessage.value = (error as Error).message
  }
}

onMounted(async () => {
  await loadRequests()
})
</script>

<template>
  <main class="page">
    <header class="header">
      <h1>LifeSwap MVP</h1>
      <p>加班/補休申請流程（建立、送審、核准、拒絕、取消）</p>
    </header>

    <section class="panel">
      <h2>建立申請</h2>
      <div class="form-grid">
        <label>
          類型
          <select v-model="form.requestType">
            <option :value="0">加班</option>
            <option :value="1">補休</option>
          </select>
        </label>

        <label>
          員工編號
          <input v-model="form.employeeId" type="text" />
        </label>

        <label>
          日期
          <input v-model="form.requestDate" type="date" />
        </label>

        <label>
          開始時間
          <input v-model="form.startTime" type="time" />
        </label>

        <label>
          結束時間
          <input v-model="form.endTime" type="time" />
        </label>
      </div>

      <label class="full-width">
        原因
        <textarea v-model="form.reason" rows="3" />
      </label>

      <button type="button" @click="handleCreate">建立草稿</button>
    </section>

    <section class="panel">
      <h2>審核參數</h2>
      <div class="form-grid">
        <label>
          審核者編號
          <input v-model="reviewerId" type="text" />
        </label>

        <label>
          審核備註
          <input v-model="reviewComment" type="text" />
        </label>
      </div>
    </section>

    <section class="panel">
      <h2>申請列表</h2>
      <p v-if="loading">載入中...</p>
      <p v-if="message" class="message success">{{ message }}</p>
      <p v-if="errorMessage" class="message error">{{ errorMessage }}</p>

      <table v-if="requests.length > 0" class="table">
        <thead>
          <tr>
            <th>員工</th>
            <th>類型</th>
            <th>日期</th>
            <th>狀態</th>
            <th>原因</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="request in requests" :key="request.id">
            <td>{{ request.employeeId }}</td>
            <td>{{ getRequestTypeLabel(request.requestType) }}</td>
            <td>{{ request.requestDate }}</td>
            <td>{{ getRequestStatusLabel(request.status) }}</td>
            <td>{{ request.reason }}</td>
            <td class="actions">
              <button type="button" @click="handleSubmit(request.id)">送審</button>
              <button type="button" @click="handleApprove(request.id)">核准</button>
              <button type="button" @click="handleReject(request.id)">拒絕</button>
              <button type="button" @click="handleCancel(request.id)">取消</button>
            </td>
          </tr>
        </tbody>
      </table>

      <p v-else>目前沒有申請資料。</p>
    </section>
  </main>
</template>

<style scoped>
.page {
  display: grid;
  gap: 16px;
}

.header {
  display: grid;
  gap: 4px;
}

.panel {
  background: white;
  border-radius: 8px;
  padding: 16px;
  display: grid;
  gap: 12px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 8px;
}

label {
  display: grid;
  gap: 4px;
  font-size: 14px;
}

input,
select,
textarea {
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 8px;
}

.full-width {
  width: 100%;
}

button {
  border: 1px solid #d1d5db;
  background: #f3f4f6;
  border-radius: 6px;
  padding: 6px 10px;
  cursor: pointer;
}

.table {
  width: 100%;
  border-collapse: collapse;
}

.table th,
.table td {
  border-bottom: 1px solid #e5e7eb;
  padding: 8px;
  text-align: left;
  vertical-align: top;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.message {
  margin: 0;
}

.success {
  color: #166534;
}

.error {
  color: #b91c1c;
}
</style>
