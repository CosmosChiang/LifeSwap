<script setup lang="ts">
import { onMounted, reactive, ref, computed } from 'vue'
import { fetchReportSummary, fetchReportTrends, fetchComplianceWarnings } from '../api'
import type { ReportSummary, TrendPoint, ComplianceWarning } from '../types'
import type { Dayjs } from 'dayjs'
import dayjs from 'dayjs'

const reportSummary = ref<ReportSummary | null>(null)
const reportTrends = ref<TrendPoint[]>([])
const complianceWarnings = ref<ComplianceWarning[]>([])
const reportLoading = ref(false)
const errorMessage = ref('')

const reportFilters = reactive({
  startDate: dayjs().subtract(30, 'days') as Dayjs,
  endDate: dayjs() as Dayjs,
  requestType: '' as string | undefined,
  department: '',
  monthlyOvertimeHourLimit: 46,
})

async function loadReports() {
  reportLoading.value = true
  errorMessage.value = ''

  try {
    const query = {
      startDate: reportFilters.startDate?.format('YYYY-MM-DD'),
      endDate: reportFilters.endDate?.format('YYYY-MM-DD'),
      requestType: reportFilters.requestType ? (Number(reportFilters.requestType) as 0 | 1) : undefined,
      department: reportFilters.department || undefined,
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
    errorMessage.value = (error as Error).message
  } finally {
    reportLoading.value = false
  }
}

const trendColumns = [
  {
    title: '日期',
    dataIndex: 'date',
    key: 'date',
  },
  {
    title: '總量',
    dataIndex: 'totalRequests',
    key: 'totalRequests',
  },
  {
    title: '核准',
    dataIndex: 'approvedCount',
    key: 'approvedCount',
  },
  {
    title: '拒絕',
    dataIndex: 'rejectedCount',
    key: 'rejectedCount',
  },
  {
    title: '取消',
    dataIndex: 'cancelledCount',
    key: 'cancelledCount',
  },
  {
    title: '核准加班時數',
    dataIndex: 'approvedOvertimeHours',
    key: 'approvedOvertimeHours',
  },
]

const complianceColumns = [
  {
    title: '員工',
    dataIndex: 'employeeId',
    key: 'employeeId',
  },
  {
    title: '年月',
    dataIndex: 'yearMonth',
    key: 'yearMonth',
  },
  {
    title: '加班時數',
    dataIndex: 'approvedOvertimeHours',
    key: 'approvedOvertimeHours',
  },
  {
    title: '月上限',
    dataIndex: 'monthlyOvertimeHourLimit',
    key: 'monthlyOvertimeHourLimit',
  },
  {
    title: '等級',
    dataIndex: 'severity',
    key: 'severity',
  },
  {
    title: '訊息',
    dataIndex: 'message',
    key: 'message',
  },
]

const complianceDataSource = computed(() =>
  complianceWarnings.value.map(w => ({
    ...w,
    key: `${w.employeeId}-${w.year}-${w.month}`,
    yearMonth: `${w.year}-${String(w.month).padStart(2, '0')}`,
  }))
)

onMounted(() => {
  loadReports()
})
</script>

<template>
  <div style="display: grid; gap: 24px">
    <!-- Filters -->
    <a-card title="篩選條件">
      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="開始日期">
            <a-date-picker v-model:value="reportFilters.startDate" style="width: 100%" />
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="結束日期">
            <a-date-picker v-model:value="reportFilters.endDate" style="width: 100%" />
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="申請類型">
            <a-select
              v-model:value="reportFilters.requestType"
              placeholder="全部"
              allow-clear
            >
              <a-select-option value="">全部</a-select-option>
              <a-select-option value="0">加班</a-select-option>
              <a-select-option value="1">補休</a-select-option>
            </a-select>
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="部門代碼">
            <a-input
              v-model:value="reportFilters.department"
              placeholder="例如 ENG"
            />
          </a-form-item>
        </a-col>
      </a-row>
      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="月加班上限（小時）">
            <a-input-number
              v-model:value="reportFilters.monthlyOvertimeHourLimit"
              :min="1"
              style="width: 100%"
            />
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="24" :md="18">
          <a-form-item label=" ">
            <a-button type="primary" :loading="reportLoading" @click="loadReports">
              重新產生報表
            </a-button>
          </a-form-item>
        </a-col>
      </a-row>
    </a-card>

    <!-- Error Alert -->
    <a-alert
      v-if="errorMessage"
      :message="errorMessage"
      type="error"
      show-icon
      closable
      @close="errorMessage = ''"
    />

    <!-- Report Summary -->
    <a-card
      v-if="reportSummary"
      title="摘要統計"
      :loading="reportLoading"
    >
      <a-row :gutter="[16, 16]">
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic
            title="總申請"
            :value="reportSummary.totalRequests"
          />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic
            title="送審中"
            :value="reportSummary.submittedCount"
            value-style="{ color: '#faad14' }"
          />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic
            title="已核准"
            :value="reportSummary.approvedCount"
            value-style="{ color: '#52c41a' }"
          />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic
            title="已拒絕"
            :value="reportSummary.rejectedCount"
            value-style="{ color: '#f5222d' }"
          />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic
            title="已取消"
            :value="reportSummary.cancelledCount"
          />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic
            title="核准加班時數"
            :value="reportSummary.approvedOvertimeHours"
            suffix=" hrs"
          />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic
            title="核准率"
            :value="(reportSummary.approvalRate * 100).toFixed(2)"
            suffix="%"
          />
        </a-col>
      </a-row>
    </a-card>

    <!-- Trends Table -->
    <a-card
      title="日趨勢分析"
      :loading="reportLoading"
    >
      <a-table
        v-if="reportTrends.length > 0"
        :columns="trendColumns"
        :data-source="reportTrends"
        :row-key="(record: TrendPoint) => record.date"
        :pagination="{ pageSize: 20 }"
        size="small"
        bordered
      />
      <a-empty v-else description="目前區間沒有趨勢資料" />
    </a-card>

    <!-- Compliance Warnings Table -->
    <a-card
      title="法規預警"
      :loading="reportLoading"
    >
      <a-table
        v-if="complianceDataSource.length > 0"
        :columns="complianceColumns"
        :data-source="complianceDataSource"
        :row-key="(record: any) => record.key"
        :pagination="{ pageSize: 20 }"
        size="small"
        bordered
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'severity'">
            <a-tag
              :color="record.severity === 'HIGH' ? 'red' : record.severity === 'MEDIUM' ? 'orange' : 'blue'"
            >
              {{ record.severity }}
            </a-tag>
          </template>
        </template>
      </a-table>
      <a-empty v-else description="目前沒有超限或接近上限的預警" />
    </a-card>
  </div>
</template>
