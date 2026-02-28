<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'
import { fetchAutomationStatus, runAutomationReminder, runAutomationReport, updateAutomationSchedulerState } from '../api'
import type { AutomationStatusResponse } from '../types'

const status = ref<AutomationStatusResponse | null>(null)
const loading = ref(false)
const runningReminder = ref(false)
const runningReport = ref(false)
const togglingScheduler = ref(false)
const { t } = useI18n()

const workflowColumns = computed(() => [
  {
    title: t('adminAutomation.table.name'),
    dataIndex: 'name',
    key: 'name',
  },
  {
    title: t('adminAutomation.table.lastStartedAt'),
    dataIndex: 'lastStartedAt',
    key: 'lastStartedAt',
  },
  {
    title: t('adminAutomation.table.lastCompletedAt'),
    dataIndex: 'lastCompletedAt',
    key: 'lastCompletedAt',
  },
  {
    title: t('adminAutomation.table.lastSucceeded'),
    dataIndex: 'lastSucceeded',
    key: 'lastSucceeded',
  },
  {
    title: t('adminAutomation.table.lastAttemptCount'),
    dataIndex: 'lastAttemptCount',
    key: 'lastAttemptCount',
  },
  {
    title: t('adminAutomation.table.consecutiveFailures'),
    dataIndex: 'consecutiveFailures',
    key: 'consecutiveFailures',
  },
  {
    title: t('adminAutomation.table.lastError'),
    dataIndex: 'lastError',
    key: 'lastError',
  },
])

const schedulerEnabledColor = computed(() =>
  status.value?.schedulerEnabled ? 'green' : 'red',
)

const summaryStats = computed(() => {
  if (!status.value) {
    return []
  }

  return [
    {
      key: 'reminderInterval',
      title: t('adminAutomation.summary.reminderInterval'),
      value: status.value.reminderIntervalMinutes,
    },
    {
      key: 'reportInterval',
      title: t('adminAutomation.summary.reportInterval'),
      value: status.value.reportIntervalMinutes,
    },
    {
      key: 'maxRetryCount',
      title: t('adminAutomation.summary.maxRetryCount'),
      value: status.value.maxRetryCount,
    },
  ]
})

function formatDateTime(value: string | null): string {
  if (!value) {
    return t('adminAutomation.table.none')
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return t('adminAutomation.table.none')
  }

  return date.toLocaleString()
}

async function loadStatus() {
  loading.value = true
  try {
    status.value = await fetchAutomationStatus()
  } catch {
    message.error(t('adminAutomation.loadFailed'))
  } finally {
    loading.value = false
  }
}

async function handleRunReminder() {
  runningReminder.value = true
  try {
    await runAutomationReminder()
    message.success(t('adminAutomation.runReminderSuccess'))
    await loadStatus()
  } catch (error) {
    message.error((error as Error).message)
  } finally {
    runningReminder.value = false
  }
}

async function handleRunReport() {
  runningReport.value = true
  try {
    await runAutomationReport()
    message.success(t('adminAutomation.runReportSuccess'))
    await loadStatus()
  } catch (error) {
    message.error((error as Error).message)
  } finally {
    runningReport.value = false
  }
}

async function handleToggleScheduler() {
  if (!status.value) {
    return
  }

  togglingScheduler.value = true
  const nextEnabled = !status.value.schedulerEnabled

  try {
    await updateAutomationSchedulerState(nextEnabled)
    message.success(
      nextEnabled
        ? t('adminAutomation.schedulerEnabledSuccess')
        : t('adminAutomation.schedulerDisabledSuccess'),
    )
    await loadStatus()
  } catch (error) {
    message.error((error as Error).message)
  } finally {
    togglingScheduler.value = false
  }
}

onMounted(loadStatus)
</script>

<template>
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">{{ t('adminAutomation.pageTitle') }}</h2>
      <a-space>
        <a-button :loading="loading" @click="loadStatus">{{ t('adminAutomation.refresh') }}</a-button>
        <a-button
          :loading="togglingScheduler"
          :type="status?.schedulerEnabled ? 'default' : 'primary'"
          @click="handleToggleScheduler"
        >
          {{ status?.schedulerEnabled ? t('adminAutomation.disableScheduler') : t('adminAutomation.enableScheduler') }}
        </a-button>
        <a-button type="primary" :loading="runningReminder" @click="handleRunReminder">
          {{ runningReminder ? t('adminAutomation.running') : t('adminAutomation.runReminder') }}
        </a-button>
        <a-button type="primary" :loading="runningReport" @click="handleRunReport">
          {{ runningReport ? t('adminAutomation.running') : t('adminAutomation.runReport') }}
        </a-button>
      </a-space>
    </div>

    <a-card :title="t('adminAutomation.summary.title')" :loading="loading">
      <a-space direction="vertical" size="middle" style="width: 100%">
        <a-tag :color="schedulerEnabledColor">
          {{ status?.schedulerEnabled ? t('adminAutomation.schedulerEnabled') : t('adminAutomation.schedulerDisabled') }}
        </a-tag>

        <a-row :gutter="16">
          <a-col v-for="item in summaryStats" :key="item.key" :xs="24" :sm="12" :md="8">
            <a-statistic :title="item.title" :value="item.value" />
          </a-col>
        </a-row>
      </a-space>
    </a-card>

    <a-card :title="t('adminAutomation.table.title')" :loading="loading">
      <a-table
        v-if="status && status.workflows.length > 0"
        :columns="workflowColumns"
        :data-source="status.workflows"
        row-key="name"
        size="small"
        bordered
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'lastStartedAt'">
            {{ formatDateTime(record.lastStartedAt) }}
          </template>
          <template v-if="column.key === 'lastCompletedAt'">
            {{ formatDateTime(record.lastCompletedAt) }}
          </template>
          <template v-if="column.key === 'lastSucceeded'">
            <a-tag :color="record.lastSucceeded ? 'green' : 'red'">
              {{ record.lastSucceeded ? t('adminAutomation.table.success') : t('adminAutomation.table.failed') }}
            </a-tag>
          </template>
          <template v-if="column.key === 'lastError'">
            {{ record.lastError || t('adminAutomation.table.none') }}
          </template>
        </template>
      </a-table>
      <a-empty v-else :description="t('adminAutomation.table.empty')" />
    </a-card>
  </div>
</template>
