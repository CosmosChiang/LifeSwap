<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import {
  approveRequest,
  cancelRequest,
  createRequest,
  fetchComplianceWarnings,
  fetchReportSummary,
  fetchReportTrends,
  fetchRequests,
  rejectRequest,
  submitRequest,
} from './api'
import type {
  ComplianceWarning,
  CreateRequestPayload,
  ReportSummary,
  RequestType,
  TimeOffRequest,
  TrendPoint,
} from './types'

const requests = ref<TimeOffRequest[]>([])
const loading = ref(false)
const reportLoading = ref(false)
const message = ref('')
const errorMessage = ref('')
const reportErrorMessage = ref('')

const reportSummary = ref<ReportSummary | null>(null)
const reportTrends = ref<TrendPoint[]>([])
const complianceWarnings = ref<ComplianceWarning[]>([])

const form = reactive<CreateRequestPayload>({
  requestType: 0,
  employeeId: 'E001',
  departmentCode: 'ENG',
  requestDate: new Date().toISOString().slice(0, 10),
  startTime: '18:00',
  endTime: '20:00',
  reason: '',
})

const reviewerId = ref('M001')
const reviewComment = ref('')

const reportFilters = reactive({
  startDate: new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().slice(0, 10),
  endDate: new Date().toISOString().slice(0, 10),
  requestType: '',
  department: '',
  monthlyOvertimeHourLimit: 46,
})

function clearAlerts(): void {
  message.value = ''
  errorMessage.value = ''
}

function clearReportAlerts(): void {
  reportErrorMessage.value = ''
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

function getOptionalRequestType(): RequestType | undefined {
  if (reportFilters.requestType === '') {
    return undefined
  }

  return Number(reportFilters.requestType) as RequestType
}

async function loadReports(): Promise<void> {
  reportLoading.value = true
  clearReportAlerts()

  try {
    const query = {
      startDate: reportFilters.startDate,
      endDate: reportFilters.endDate,
      requestType: getOptionalRequestType(),
      department: reportFilters.department,
      monthlyOvertimeHourLimit: reportFilters.monthlyOvertimeHourLimit,
    }

    const [summary, trends, warnings] = await Promise.all([
      fetchReportSummary(query),
      fetchReportTrends(query),
      fetchComplianceWarnings(query),
    ])

    reportSummary.value = summary
    reportTrends.value = trends
    complianceWarnings.value = warnings
  } catch (error) {
    reportErrorMessage.value = (error as Error).message
  } finally {
    reportLoading.value = false
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
  await loadReports()
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
          部門代碼
          <input v-model="form.departmentCode" type="text" />
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
            <th>部門</th>
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
            <td>{{ request.departmentCode }}</td>
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

    <section class="panel">
      <h2>Phase 2：報表與法規預警</h2>

      <div class="form-grid">
        <label>
          起始日期
          <input v-model="reportFilters.startDate" type="date" />
        </label>

        <label>
          結束日期
          <input v-model="reportFilters.endDate" type="date" />
        </label>

        <label>
          類型
          <select v-model="reportFilters.requestType">
            <option value="">全部</option>
            <option value="0">加班</option>
            <option value="1">補休</option>
          </select>
        </label>

        <label>
          部門代碼
          <input v-model="reportFilters.department" type="text" placeholder="例如 ENG" />
        </label>

        <label>
          月加班上限（小時）
          <input v-model.number="reportFilters.monthlyOvertimeHourLimit" type="number" min="1" />
        </label>
      </div>

      <button type="button" @click="loadReports">重新產生報表</button>

      <p v-if="reportLoading">報表載入中...</p>
      <p v-if="reportErrorMessage" class="message error">{{ reportErrorMessage }}</p>

      <div v-if="reportSummary" class="report-grid">
        <p>總申請：{{ reportSummary.totalRequests }}</p>
        <p>送審中：{{ reportSummary.submittedCount }}</p>
        <p>核准：{{ reportSummary.approvedCount }}</p>
        <p>拒絕：{{ reportSummary.rejectedCount }}</p>
        <p>取消：{{ reportSummary.cancelledCount }}</p>
        <p>核准加班時數：{{ reportSummary.approvedOvertimeHours }}</p>
        <p>核准率：{{ (reportSummary.approvalRate * 100).toFixed(2) }}%</p>
      </div>

      <h3>趨勢（日）</h3>
      <table v-if="reportTrends.length > 0" class="table">
        <thead>
          <tr>
            <th>日期</th>
            <th>總量</th>
            <th>核准</th>
            <th>拒絕</th>
            <th>取消</th>
            <th>核准加班時數</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="trend in reportTrends" :key="trend.date">
            <td>{{ trend.date }}</td>
            <td>{{ trend.totalRequests }}</td>
            <td>{{ trend.approvedCount }}</td>
            <td>{{ trend.rejectedCount }}</td>
            <td>{{ trend.cancelledCount }}</td>
            <td>{{ trend.approvedOvertimeHours }}</td>
          </tr>
        </tbody>
      </table>
      <p v-else>目前區間沒有趨勢資料。</p>

      <h3>法規預警</h3>
      <table v-if="complianceWarnings.length > 0" class="table">
        <thead>
          <tr>
            <th>員工</th>
            <th>年月</th>
            <th>加班時數</th>
            <th>上限</th>
            <th>等級</th>
            <th>訊息</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="warning in complianceWarnings"
            :key="`${warning.employeeId}-${warning.year}-${warning.month}`"
          >
            <td>{{ warning.employeeId }}</td>
            <td>{{ warning.year }}-{{ String(warning.month).padStart(2, '0') }}</td>
            <td>{{ warning.approvedOvertimeHours }}</td>
            <td>{{ warning.monthlyOvertimeHourLimit }}</td>
            <td>{{ warning.severity }}</td>
            <td>{{ warning.message }}</td>
          </tr>
        </tbody>
      </table>
      <p v-else>目前沒有超限或接近上限的預警。</p>
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

.report-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 8px;
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
