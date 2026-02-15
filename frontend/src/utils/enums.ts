import type { RequestType } from '../types'

export function getRequestTypeLabel(value: RequestType): string {
  return value === 0 ? 'Overtime' : 'CompOff'
}

export function getRequestStatusLabel(value: number): string {
  switch (value) {
    case 0:
      return 'Draft'
    case 1:
      return 'Submitted'
    case 2:
      return 'Approved'
    case 3:
      return 'Rejected'
    case 4:
      return 'Cancelled'
    default:
      return 'Unknown'
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
    default:
      return 'default'
  }
}
