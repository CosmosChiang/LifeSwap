export type RequestType = 0 | 1

export type RequestStatus = 0 | 1 | 2 | 3 | 4 | 5

export interface TimeOffRequest {
    id: string
    requestType: RequestType
    employeeId: string
    applicantName: string
    departmentCode: string
    requestDate: string
    startTime: string | null
    endTime: string | null
    overtimeStartAt: string | null
    overtimeEndAt: string | null
    compTimeHours: number
    overtimeProject: string
    overtimeContent: string
    overtimeReason: string
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
    employeeId: string
    overtimeStartAt: string
    overtimeEndAt: string
    overtimeProject: string
    overtimeContent: string
    overtimeReason: string
}

export interface ReviewPayload {
    comment: string
}

export interface ReportSummary {
    startDate: string
    endDate: string
    requestType: RequestType | null
    employeeId: string | null
    departmentCode: string | null
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
    employeeId?: string
    departmentCode?: string
    monthlyOvertimeHourLimit?: number
}

export interface NotificationItem {
    id: string
    recipientEmployeeId: string
    title: string
    message: string
    isRead: boolean
    createdAt: string
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

export interface RoleItem {
    id: string
    name: string
    description: string
}

export interface UserItem {
    id: string
    username: string
    email: string
    employeeId: string
    departmentCode: string
    isActive: boolean
    roles: RoleItem[]
}

export interface CreateUserPayload {
    username: string
    email: string
    employeeId: string
    password: string
    roleIds: string[]
}

export interface UpdateUserPayload {
    email: string
    isActive: boolean
    roleIds: string[]
}

export interface AutomationWorkflowStatus {
    name: string
    lastStartedAt: string | null
    lastCompletedAt: string | null
    lastSucceeded: boolean
    lastAttemptCount: number
    consecutiveFailures: number
    lastError: string | null
}

export interface AutomationStatusResponse {
    schedulerEnabled: boolean
    reminderIntervalMinutes: number
    reportIntervalMinutes: number
    maxRetryCount: number
    workflows: AutomationWorkflowStatus[]
}
