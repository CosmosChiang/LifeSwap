<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import dayjs, { type Dayjs } from 'dayjs'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'
import { createRequest } from '../../../api'
import type { CreateRequestPayload } from '../../../types'
import { useAuth } from '../../../composables/useAuth'

const props = defineProps<{
  initialData?: Partial<CreateRequestPayload>
}>()

const emit = defineEmits<{
  success: []
  cancel: []
}>()

interface RequestFormModel {
  employeeId: string
  applicantName: string
  overtimeStartAt: Dayjs
  overtimeEndAt: Dayjs
  overtimeProject: string
  overtimeContent: string
  overtimeReason: string
}

const { currentUser } = useAuth()

const form = reactive<RequestFormModel>({
  employeeId: props.initialData?.employeeId ?? currentUser.value?.employeeId ?? 'E001',
  applicantName: currentUser.value?.username ?? '',
  overtimeStartAt: props.initialData?.overtimeStartAt ? dayjs(props.initialData.overtimeStartAt) : dayjs(),
  overtimeEndAt: props.initialData?.overtimeEndAt ? dayjs(props.initialData.overtimeEndAt) : dayjs().add(2, 'hour'),
  overtimeProject: props.initialData?.overtimeProject ?? '',
  overtimeContent: props.initialData?.overtimeContent ?? '',
  overtimeReason: props.initialData?.overtimeReason ?? '',
})

const loading = ref(false)
const errorMessage = ref('')
const { t } = useI18n()
const compTimeHours = computed(() => {
  const diffHours = form.overtimeEndAt.diff(form.overtimeStartAt, 'minute') / 60
  if (diffHours <= 0) {
    return 0
  }

  return Number(diffHours.toFixed(2))
})

async function handleSubmit() {
  errorMessage.value = ''

  if (!form.overtimeProject.trim() || !form.overtimeContent.trim() || !form.overtimeReason.trim()) {
    errorMessage.value = t('requestForm.validation.requiredFields')
    return
  }

  if (compTimeHours.value <= 0) {
    errorMessage.value = t('requestForm.validation.invalidOvertimeRange')
    return
  }

  loading.value = true
  try {
    const payload: CreateRequestPayload = {
      employeeId: form.employeeId,
      overtimeStartAt: form.overtimeStartAt.toISOString(),
      overtimeEndAt: form.overtimeEndAt.toISOString(),
      overtimeProject: form.overtimeProject.trim(),
      overtimeContent: form.overtimeContent.trim(),
      overtimeReason: form.overtimeReason.trim(),
    }

    await createRequest({
      ...payload,
    })
    message.success(t('requestForm.createDraftSuccess'))
    emit('success')
    form.overtimeProject = ''
    form.overtimeContent = ''
    form.overtimeReason = ''
  } catch (error) {
    errorMessage.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

function handleReset() {
  form.overtimeProject = ''
  form.overtimeContent = ''
  form.overtimeReason = ''
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
          <a-form-item :label="t('requestForm.employeeId')">
            <a-input :value="form.employeeId" @update:value="form.employeeId = $event" />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.applicant')">
            <a-input :value="form.applicantName" readonly />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.overtimeStartAt')">
            <a-date-picker :value="form.overtimeStartAt" show-time format="YYYY-MM-DD HH:mm" style="width: 100%"
              @update:value="form.overtimeStartAt = $event ?? dayjs()" />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.overtimeEndAt')">
            <a-date-picker :value="form.overtimeEndAt" show-time format="YYYY-MM-DD HH:mm" style="width: 100%"
              @update:value="form.overtimeEndAt = $event ?? dayjs()" />
          </a-form-item>
        </a-col>
      </a-row>

      <a-row :gutter="16">
        <a-col :xs="24" :sm="12" :md="6">
          <a-form-item :label="t('requestForm.compTimeHours')">
            <a-input :value="compTimeHours.toFixed(2)" readonly />
          </a-form-item>
        </a-col>

        <a-col :xs="24" :sm="12" :md="18">
          <a-form-item :label="t('requestForm.overtimeProject')">
            <a-input :value="form.overtimeProject" :placeholder="t('requestForm.overtimeProjectPlaceholder')"
              @update:value="form.overtimeProject = $event" />
          </a-form-item>
        </a-col>
      </a-row>

      <a-row :gutter="16">
        <a-col :xs="24">
          <a-form-item :label="t('requestForm.overtimeContent')">
            <a-textarea :value="form.overtimeContent" :placeholder="t('requestForm.overtimeContentPlaceholder')" :rows="3"
              @update:value="form.overtimeContent = $event" />
          </a-form-item>
        </a-col>

        <a-col :xs="24">
          <a-form-item :label="t('requestForm.overtimeReason')">
            <a-textarea :value="form.overtimeReason" :placeholder="t('requestForm.overtimeReasonPlaceholder')" :rows="3"
              @update:value="form.overtimeReason = $event" />
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
