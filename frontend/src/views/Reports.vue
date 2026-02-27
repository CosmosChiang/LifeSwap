<script setup lang="ts">
import { onMounted, reactive, ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { fetchReportSummary, fetchReportTrends, fetchComplianceWarnings } from '../api'
import type { ReportSummary, TrendPoint, ComplianceWarning } from '../types'
import type { Dayjs } from 'dayjs'
import dayjs from 'dayjs'

const reportSummary = ref<ReportSummary | null>(null)
const reportTrends = ref<TrendPoint[]>([])
const complianceWarnings = ref<ComplianceWarning[]>([])
const reportLoading = ref(false)
const errorMessage = ref('')
const { t } = useI18n()

const reportFilters = reactive({
  startDate: dayjs().subtract(30, 'days') as Dayjs,
  endDate: dayjs() as Dayjs,
  requestType: '' as string | undefined,
  employeeId: '',
  departmentCode: '',
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
      employeeId: reportFilters.employeeId || undefined,
      departmentCode: reportFilters.departmentCode || undefined,
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

const trendColumns = computed(() => [
  {
    title: t('reports.columns.date'),
    dataIndex: 'date',
    key: 'date',
  },
  {
    title: t('reports.columns.totalRequests'),
    dataIndex: 'totalRequests',
    key: 'totalRequests',
  },
  {
    title: t('reports.columns.approvedCount'),
    dataIndex: 'approvedCount',
    key: 'approvedCount',
  },
  {
    title: t('reports.columns.rejectedCount'),
    dataIndex: 'rejectedCount',
    key: 'rejectedCount',
  },
  {
    title: t('reports.columns.cancelledCount'),
    dataIndex: 'cancelledCount',
    key: 'cancelledCount',
  },
  {
    title: t('reports.columns.approvedOvertimeHours'),
    dataIndex: 'approvedOvertimeHours',
    key: 'approvedOvertimeHours',
  },
])

const complianceColumns = computed(() => [
  {
    title: t('reports.columns.employeeId'),
    dataIndex: 'employeeId',
    key: 'employeeId',
  },
  {
    title: t('reports.columns.yearMonth'),
    dataIndex: 'yearMonth',
    key: 'yearMonth',
  },
  {
    title: t('reports.columns.approvedOvertimeHoursValue'),
    dataIndex: 'approvedOvertimeHours',
    key: 'approvedOvertimeHours',
  },
  {
    title: t('reports.columns.monthlyLimit'),
    dataIndex: 'monthlyOvertimeHourLimit',
    key: 'monthlyOvertimeHourLimit',
  },
  {
    title: t('reports.columns.severity'),
    dataIndex: 'severity',
    key: 'severity',
  },
  {
    title: t('reports.columns.message'),
    dataIndex: 'message',
    key: 'message',
  },
])

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
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">{{ t('reports.pageTitle') }}</h2>
    </div>

    <!-- Filters -->
    <a-card :title="t('reports.filters.title')">
      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('reports.filters.startDate')">
            <a-date-picker :value="reportFilters.startDate" style="width: 100%"
              @update:value="reportFilters.startDate = $event" />
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('reports.filters.endDate')">
            <a-date-picker :value="reportFilters.endDate" style="width: 100%"
              @update:value="reportFilters.endDate = $event" />
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('reports.filters.requestType')">
            <a-select :value="reportFilters.requestType" :placeholder="t('reports.filters.all')" allow-clear
              @update:value="reportFilters.requestType = $event">
              <a-select-option value="">{{ t('reports.filters.all') }}</a-select-option>
              <a-select-option value="0">{{ t('requestType.overtime') }}</a-select-option>
              <a-select-option value="1">{{ t('requestType.compOff') }}</a-select-option>
            </a-select>
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('reports.filters.employeeId')">
            <a-input :value="reportFilters.employeeId" :placeholder="t('reports.filters.employeePlaceholder')"
              @update:value="reportFilters.employeeId = $event" />
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('reports.filters.departmentCode')">
            <a-input :value="reportFilters.departmentCode" :placeholder="t('reports.filters.departmentPlaceholder')"
              @update:value="reportFilters.departmentCode = $event" />
          </a-form-item>
        </a-col>
      </a-row>
      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('reports.filters.monthlyLimit')">
            <a-input-number :value="reportFilters.monthlyOvertimeHourLimit" :min="1" style="width: 100%"
              @update:value="reportFilters.monthlyOvertimeHourLimit = $event" />
          </a-form-item>
        </a-col>
        <a-col :xs="24" :sm="24" :md="18">
          <a-form-item label=" ">
            <a-button type="primary" :loading="reportLoading" @click="loadReports">
              {{ t('reports.filters.generate') }}
            </a-button>
          </a-form-item>
        </a-col>
      </a-row>
    </a-card>

    <!-- Error Alert -->
    <a-alert v-if="errorMessage" :message="errorMessage" type="error" show-icon closable @close="errorMessage = ''" />

    <!-- Report Summary -->
    <a-card v-if="reportSummary" :title="t('reports.summary.title')" :loading="reportLoading">
      <a-row :gutter="[16, 16]">
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic :title="t('reports.summary.totalRequests')" :value="reportSummary.totalRequests" />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic :title="t('reports.summary.submittedCount')" :value="reportSummary.submittedCount" value-style="{ color: '#faad14' }" />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic :title="t('reports.summary.approvedCount')" :value="reportSummary.approvedCount" value-style="{ color: '#52c41a' }" />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic :title="t('reports.summary.rejectedCount')" :value="reportSummary.rejectedCount" value-style="{ color: '#f5222d' }" />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic :title="t('reports.summary.cancelledCount')" :value="reportSummary.cancelledCount" />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic :title="t('reports.summary.approvedOvertimeHours')" :value="reportSummary.approvedOvertimeHours" :suffix="t('reports.summary.hours')" />
        </a-col>
        <a-col :xs="24" :sm="12" :md="8">
          <a-statistic :title="t('reports.summary.approvalRate')" :value="(reportSummary.approvalRate * 100).toFixed(2)" suffix="%" />
        </a-col>
      </a-row>
    </a-card>

    <!-- Trends Table -->
    <a-card :title="t('reports.trends.title')" :loading="reportLoading">
      <a-table v-if="reportTrends.length > 0" :columns="trendColumns" :data-source="reportTrends" row-key="date"
        :pagination="{ pageSize: 20 }" size="small" bordered />
      <a-empty v-else :description="t('reports.trends.empty')" />
    </a-card>

    <!-- Compliance Warnings Table -->
    <a-card :title="t('reports.compliance.title')" :loading="reportLoading">
      <a-table v-if="complianceDataSource.length > 0" :columns="complianceColumns" :data-source="complianceDataSource"
        row-key="key" :pagination="{ pageSize: 20 }" size="small" bordered>
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'severity'">
            <a-tag :color="record.severity === 'Critical' ? 'red' : 'orange'">
              {{ t(`reports.severity.${String(record.severity).toLowerCase()}`) }}
            </a-tag>
          </template>
        </template>
      </a-table>
      <a-empty v-else :description="t('reports.compliance.empty')" />
    </a-card>
  </div>
</template>
