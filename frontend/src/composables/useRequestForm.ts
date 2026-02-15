import { reactive, ref } from 'vue'
import { createRequest } from '../api'
import type { CreateRequestPayload } from '../types'

export function useRequestForm() {
  const form = reactive<CreateRequestPayload>({
    requestType: 0,
    employeeId: 'E001',
    departmentCode: 'ENG',
    requestDate: new Date().toISOString().slice(0, 10),
    startTime: '18:00',
    endTime: '20:00',
    reason: '',
  })

  const message = ref('')
  const errorMessage = ref('')
  const loading = ref(false)

  async function handleCreate(): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''

    if (!form.reason.trim()) {
      errorMessage.value = '請輸入申請原因。'
      return false
    }

    loading.value = true
    try {
      await createRequest({
        ...form,
        reason: form.reason.trim(),
      })
      message.value = '申請草稿已建立。'
      form.reason = ''
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
