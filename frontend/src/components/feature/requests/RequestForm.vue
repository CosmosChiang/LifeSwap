<script setup lang="ts">
import { reactive, ref } from 'vue'
import dayjs, { type Dayjs } from 'dayjs'
import { useI18n } from 'vue-i18n'
import { createRequest } from '../../../api'
import type { CreateRequestPayload } from '../../../types'

const props = defineProps<{
  initialData?: Partial<CreateRequestPayload>
}>()

const emit = defineEmits<{
  success: []
  cancel: []
}>()

interface RequestFormModel {
  requestType: CreateRequestPayload['requestType']
  employeeId: string
  requestDate: Dayjs
  startTime: Dayjs | null
  endTime: Dayjs | null
  reason: string
}

const form = reactive<RequestFormModel>({
  requestType: props.initialData?.requestType ?? 0,
  employeeId: props.initialData?.employeeId ?? 'E001',
  requestDate: props.initialData?.requestDate ? dayjs(props.initialData.requestDate) : dayjs(),
  startTime: props.initialData?.startTime ? dayjs(props.initialData.startTime, 'HH:mm') : dayjs('18:00', 'HH:mm'),
  endTime: props.initialData?.endTime ? dayjs(props.initialData.endTime, 'HH:mm') : dayjs('20:00', 'HH:mm'),
  reason: props.initialData?.reason ?? '',
})

const loading = ref(false)
const errorMessage = ref('')
const { t } = useI18n()

async function handleSubmit() {
  errorMessage.value = ''

  if (!form.reason.trim()) {
    errorMessage.value = t('requestForm.validation.reasonRequired')
    return
  }

  loading.value = true
  try {
    const payload: CreateRequestPayload = {
      requestType: form.requestType,
      employeeId: form.employeeId,
      requestDate: form.requestDate.format('YYYY-MM-DD'),
      startTime: form.startTime ? form.startTime.format('HH:mm') : null,
      endTime: form.endTime ? form.endTime.format('HH:mm') : null,
      reason: form.reason.trim(),
    }

    await createRequest({
      ...payload,
    })
    emit('success')
    // Reset form
    form.reason = ''
    form.requestDate = dayjs()
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
    <a-alert v-if="errorMessage" :message="errorMessage" type="error" show-icon closable style="margin-bottom: 16px"
      @close="errorMessage = ''" />

    <a-form layout="vertical">
      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.requestType')">
            <a-select :value="form.requestType" @update:value="form.requestType = $event">
              <a-select-option :value="0">{{ t('requestType.overtime') }}</a-select-option>
              <a-select-option :value="1">{{ t('requestType.compOff') }}</a-select-option>
            </a-select>
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.employeeId')">
            <a-input :value="form.employeeId" @update:value="form.employeeId = $event" />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.date')">
            <a-date-picker :value="form.requestDate" style="width: 100%" @update:value="form.requestDate = $event" />
          </a-form-item>
        </a-col>
      </a-row>

      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.startTime')">
            <a-time-picker :value="form.startTime" format="HH:mm" style="width: 100%"
              @update:value="form.startTime = $event" />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.endTime')">
            <a-time-picker :value="form.endTime" format="HH:mm" style="width: 100%"
              @update:value="form.endTime = $event" />
          </a-form-item>
        </a-col>

        <a-col :xs="24">
          <a-form-item :label="t('requestForm.reason')">
            <a-textarea :value="form.reason" :placeholder="t('requestForm.reasonPlaceholder')" :rows="4" @update:value="form.reason = $event" />
          </a-form-item>
        </a-col>
      </a-row>

      <a-form-item>
        <a-button type="primary" html-type="submit" :loading="loading" @click="handleSubmit">
          {{ t('requestForm.createDraft') }}
        </a-button>
        <a-button style="margin-left: 8px" @click="handleReset">{{ t('common.reset') }}</a-button>
      </a-form-item>
    </a-form>
  </div>
</template>
