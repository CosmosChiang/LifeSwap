export type RequestType = 0 | 1

export type RequestStatus = 0 | 1 | 2 | 3 | 4

export interface TimeOffRequest {
    id: string
    requestType: RequestType
    employeeId: string
    departmentCode: string
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
    departmentCode?: string | null
    requestDate: string
    startTime: string | null
    endTime: string | null
    reason: string
}

export interface ReviewPayload {
    reviewerId: string
    comment: string
}

export interface ReportSummary {
    startDate: string
    endDate: string
    requestType: RequestType | null
    department: string | null
    totalRequests: number
    submittedCount: number
    approvedCount: number
    rejectedCount: number
    cancelledCount: number
    approvedOvertimeHours: number
    approvalRate: number
}

export interface TrendPoint {
    date: string
    totalRequests: number
    approvedCount: number
    rejectedCount: number
    cancelledCount: number
    approvedOvertimeHours: number
}

export interface ComplianceWarning {
    employeeId: string
    year: number
    month: number
    approvedOvertimeHours: number
    monthlyOvertimeHourLimit: number
    severity: string
    message: string
}

export interface ReportQuery {
    startDate: string
    endDate: string
    requestType?: RequestType
    department?: string
    monthlyOvertimeHourLimit?: number
}
// Authentication types
export interface LoginRequest {
    username: string
    password: string
}

export interface LoginResponse {
    token: string
    username: string
    employeeId: string
    email: string
    departmentCode: string
    roles: string[]
}

export interface UserInfo {
    username: string
    employeeId: string
    email: string
    departmentCode: string
    roles: string[]
}
