export type RequestType = 0 | 1

export type RequestStatus = 0 | 1 | 2 | 3 | 4

export interface TimeOffRequest {
    id: string
    requestType: RequestType
    employeeId: string
    requestDate: string
    startTime: string | null
    endTime: string | null
    reason: string
    status: RequestStatus
    reviewerId: string | null
    reviewComment: string | null
    createdAt: string
    submittedAt: string | null
    reviewedAt: string | null
    cancelledAt: string | null
}

export interface CreateRequestPayload {
    requestType: RequestType
    employeeId: string
    requestDate: string
    startTime: string | null
    endTime: string | null
    reason: string
}

export interface ReviewPayload {
    reviewerId: string
    comment: string
}