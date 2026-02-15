<script setup lang="ts">
import { reactive, ref } from 'vue'
import { createRequest } from '../../../api'
import type { CreateRequestPayload } from '../../../types'

const props = defineProps<{
  initialData?: Partial<CreateRequestPayload>
}>()

const emit = defineEmits<{
  success: []
  cancel: []
}>()

const form = reactive<CreateRequestPayload>({
  requestType: props.initialData?.requestType ?? 0,
  employeeId: props.initialData?.employeeId ?? 'E001',
  departmentCode: props.initialData?.departmentCode ?? 'ENG',
  requestDate: props.initialData?.requestDate ?? new Date().toISOString().slice(0, 10),
  startTime: props.initialData?.startTime ?? '18:00',
  endTime: props.initialData?.endTime ?? '20:00',
  reason: props.initialData?.reason ?? '',
})

const loading = ref(false)
const errorMessage = ref('')

async function handleSubmit() {
  errorMessage.value = ''

  if (!form.reason.trim()) {
    errorMessage.value = '請輸入申請原因。'
    return
  }

  loading.value = true
  try {
    await createRequest({
      ...form,
      reason: form.reason.trim(),
    })
    emit('success')
    // Reset form
    form.reason = ''
    form.requestDate = new Date().toISOString().slice(0, 10)
  } catch (error) {
    errorMessage.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

function handleReset() {
  form.reason = ''
  errorMessage.value = ''
}
</script>

<template>
  <div>
    <a-alert
      v-if="errorMessage"
      :message="errorMessage"
      type="error"
      show-icon
      closable
      style="margin-bottom: 16px"
      @close="errorMessage = ''"
    />

    <a-form layout="vertical">
      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="申請類型">
            <a-select v-model:value="form.requestType">
              <a-select-option :value="0">加班</a-select-option>
              <a-select-option :value="1">補休</a-select-option>
            </a-select>
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="員工編號">
            <a-input v-model:value="form.employeeId" />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="部門代碼">
            <a-input v-model:value="form.departmentCode" />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="日期">
            <a-date-picker v-model:value="form.requestDate" style="width: 100%" />
          </a-form-item>
        </a-col>
      </a-row>

      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="開始時間">
            <a-time-picker v-model:value="form.startTime" format="HH:mm" style="width: 100%" />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item label="結束時間">
            <a-time-picker v-model:value="form.endTime" format="HH:mm" style="width: 100%" />
          </a-form-item>
        </a-col>

        <a-col :xs="24">
          <a-form-item label="申請原因">
            <a-textarea
              v-model:value="form.reason"
              placeholder="請輸入申請原因（必填）"
              :rows="4"
            />
          </a-form-item>
        </a-col>
      </a-row>

      <a-form-item>
        <a-button type="primary" html-type="submit" :loading="loading" @click="handleSubmit">
          建立草稿
        </a-button>
        <a-button style="margin-left: 8px" @click="handleReset">重置</a-button>
      </a-form-item>
    </a-form>
  </div>
</template>
