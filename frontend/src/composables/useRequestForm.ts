import { reactive, ref } from 'vue'
import { createRequest } from '../api'
import type { CreateRequestPayload } from '../types'
import { i18n } from '../i18n'

export function useRequestForm() {
  const form = reactive<CreateRequestPayload>({
    employeeId: 'E001',
    overtimeStartAt: new Date().toISOString(),
    overtimeEndAt: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(),
    overtimeProject: '',
    overtimeContent: '',
    overtimeReason: '',
  })

  const message = ref('')
  const errorMessage = ref('')
  const loading = ref(false)

  async function handleCreate(): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''

    if (!form.overtimeProject.trim() || !form.overtimeContent.trim() || !form.overtimeReason.trim()) {
      errorMessage.value = i18n.global.t('requestForm.validation.requiredFields')
      return false
    }

    loading.value = true
    try {
      await createRequest({
        ...form,
        overtimeProject: form.overtimeProject.trim(),
        overtimeContent: form.overtimeContent.trim(),
        overtimeReason: form.overtimeReason.trim(),
      })
      message.value = i18n.global.t('requestForm.createDraft')
      form.overtimeProject = ''
      form.overtimeContent = ''
      form.overtimeReason = ''
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      return false
    } finally {
      loading.value = false
    }
  }

  return {
    form,
    message,
    errorMessage,
    loading,
    handleCreate,
  }
}
