import { ref } from 'vue'
import { message as antMessage } from 'ant-design-vue'
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
      antMessage.success(message.value)
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      antMessage.error(errorMessage.value)
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
      antMessage.success(message.value)
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      antMessage.error(errorMessage.value)
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
      antMessage.success(message.value)
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      antMessage.error(errorMessage.value)
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
      antMessage.success(message.value)
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      antMessage.error(errorMessage.value)
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
      antMessage.success(message.value)
      return true
    } catch (error) {
      errorMessage.value = (error as Error).message
      antMessage.error(errorMessage.value)
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
