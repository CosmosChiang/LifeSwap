import { ref } from 'vue'
import {
  approveRequest,
  cancelRequest,
  rejectRequest,
  submitRequest,
} from '../api'

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
      message.value = '申請已送審。'
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
    reviewerId: string,
    comment: string,
  ): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''
    loading.value = true

    try {
      await approveRequest(requestId, {
        reviewerId,
        comment,
      })
      message.value = '申請已核准。'
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
    reviewerId: string,
    comment: string,
  ): Promise<boolean> {
    message.value = ''
    errorMessage.value = ''
    loading.value = true

    try {
      await rejectRequest(requestId, {
        reviewerId,
        comment,
      })
      message.value = '申請已拒絕。'
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
      message.value = '申請已取消。'
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
    handleCancel,
  }
}
