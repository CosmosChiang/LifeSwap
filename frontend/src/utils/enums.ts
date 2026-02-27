import type { RequestType } from '../types'
import { i18n } from '../i18n'

export function getRequestTypeLabel(value: RequestType): string {
  return value === 0 ? i18n.global.t('requestType.overtime') : i18n.global.t('requestType.compOff')
}

export function getRequestStatusLabel(value: number): string {
  switch (value) {
    case 0:
      return i18n.global.t('requestStatus.draft')
    case 1:
      return i18n.global.t('requestStatus.submitted')
    case 2:
      return i18n.global.t('requestStatus.approved')
    case 3:
      return i18n.global.t('requestStatus.rejected')
    case 4:
      return i18n.global.t('requestStatus.cancelled')
    case 5:
      return i18n.global.t('requestStatus.returned')
    default:
      return i18n.global.t('requestStatus.unknown')
  }
}

export function getRequestStatusColor(value: number): string {
  switch (value) {
    case 0:
      return 'default'
    case 1:
      return 'processing'
    case 2:
      return 'success'
    case 3:
      return 'error'
    case 4:
      return 'default'
    case 5:
      return 'warning'
    default:
      return 'default'
  }
}
