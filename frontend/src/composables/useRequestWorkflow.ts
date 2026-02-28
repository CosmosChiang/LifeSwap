import { ref } from 'vue'
import {
  approveRequest,
  cancelRequest,
  rejectRequest,
  returnRequest,
  submitRequest,
} from '../api'
import { i18n } from '../i18n'

export function useRequestWorkflow() {
  const message = ref('')
  const errorMessage = ref('')
  const loading = ref(false)

  async function handleSubmit(requestId: string): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''
    loading.value = true

    try {
      await submitRequest(requestId)
      message.value = i18n.global.t('workflow.submitSuccess')
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      return false
    } finally {
      loading.value = false
    }
  }

  async function handleApprove(
    requestId: string,
    comment: string,
  ): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''
    loading.value = true

    try {
      await approveRequest(requestId, {
        comment,
      })
      message.value = i18n.global.t('workflow.approveSuccess')
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      return false
    } finally {
      loading.value = false
    }
  }

  async function handleReject(
    requestId: string,
    comment: string,
  ): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''
    loading.value = true

    try {
      await rejectRequest(requestId, {
        comment,
      })
      message.value = i18n.global.t('workflow.rejectSuccess')
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      return false
    } finally {
      loading.value = false
    }
  }

  async function handleReturn(
    requestId: string,
    comment: string,
  ): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''
    loading.value = true

    try {
      await returnRequest(requestId, {
        comment,
      })
      message.value = i18n.global.t('workflow.returnSuccess')
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      return false
    } finally {
      loading.value = false
    }
  }

  async function handleCancel(requestId: string): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''
    loading.value = true

    try {
      await cancelRequest(requestId)
      message.value = i18n.global.t('workflow.cancelSuccess')
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      return false
    } finally {
      loading.value = false
    }
  }

  return {
    message,
    errorMessage,
    loading,
    handleSubmit,
    handleApprove,
    handleReject,
    handleReturn,
    handleCancel,
  }
}
