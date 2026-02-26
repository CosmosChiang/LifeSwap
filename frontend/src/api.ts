import type {
    ComplianceWarning,
    CreateRequestPayload,
    CreateUserPayload,
    LoginRequest,
    LoginResponse,
    ReportQuery,
    ReportSummary,
    ReviewPayload,
    RoleItem,
    TrendPoint,
    TimeOffRequest,
    UpdateUserPayload,
    UserItem,
} from './types'

const baseUrl = '/api/requests'
const reportsBaseUrl = '/api/reports'
const authBaseUrl = '/api/auth'

function getAuthToken(): string | null {
    return localStorage.getItem('auth_token')
}

function getAuthHeaders(): HeadersInit {
    const token = getAuthToken()
    const headers: HeadersInit = {
        'Content-Type': 'application/json',
    }

    if (token) {
        headers['Authorization'] = `Bearer ${token}`
    }

    return headers
}

async function handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
        const text = await response.text()
        throw new Error(text || 'API request failed')
    }

    return (await response.json()) as T
}

export async function login(payload: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${authBaseUrl}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    })

    const data = await handleResponse<LoginResponse>(response)
    localStorage.setItem('auth_token', data.token)
    return data
}

export function logout(): void {
    localStorage.removeItem('auth_token')
}

export async function changeMyPassword(currentPassword: string, newPassword: string): Promise<void> {
    const response = await fetch(`${authBaseUrl}/change-password`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ currentPassword, newPassword }),
    })

    if (!response.ok) {
        const text = await response.text()
        throw new Error(text || '密碼修改失敗')
    }
}

export async function getUsers(): Promise<UserItem[]> {
    const response = await fetch('/api/user', {
        headers: getAuthHeaders(),
    })

    return handleResponse<UserItem[]>(response)
}

export async function deleteUser(userId: string): Promise<void> {
    const response = await fetch(`/api/user/${userId}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
    })

    if (!response.ok) {
        throw new Error('刪除失敗')
    }
}

export async function createUser(payload: CreateUserPayload): Promise<UserItem> {
    const response = await fetch('/api/user', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload),
    })

    return handleResponse<UserItem>(response)
}

export async function updateUser(userId: string, payload: UpdateUserPayload): Promise<UserItem> {
    const response = await fetch(`/api/user/${userId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload),
    })

    return handleResponse<UserItem>(response)
}

export async function resetUserPassword(userId: string, newPassword: string): Promise<void> {
    const response = await fetch(`/api/user/${userId}/reset-password`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ newPassword }),
    })

    if (!response.ok) {
        const text = await response.text()
        throw new Error(text || '密碼重置失敗')
    }
}

export async function getRoles(): Promise<RoleItem[]> {
    const response = await fetch('/api/role', {
        headers: getAuthHeaders(),
    })

    return handleResponse<RoleItem[]>(response)
}

export async function assignUserRoles(userId: string, roleIds: string[]): Promise<void> {
    const response = await fetch(`/api/user/${userId}/roles`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ roleIds }),
    })

    if (!response.ok) {
        const text = await response.text()
        throw new Error(text || '更新角色失敗')
    }
}

export async function fetchRequests(): Promise<TimeOffRequest[]> {
    const response = await fetch(baseUrl, {
        headers: getAuthHeaders(),
    })

    return handleResponse<TimeOffRequest[]>(response)
}

export async function createRequest(payload: CreateRequestPayload): Promise<TimeOffRequest> {
    const response = await fetch(baseUrl, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function submitRequest(requestId: string): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/submit`, {
        method: 'POST',
        headers: getAuthHeaders(),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function approveRequest(requestId: string, payload: ReviewPayload): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/approve`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function rejectRequest(requestId: string, payload: ReviewPayload): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/reject`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function returnRequest(requestId: string, payload: ReviewPayload): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/return`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function cancelRequest(requestId: string): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/cancel`, {
        method: 'POST',
        headers: getAuthHeaders(),
    })

    return handleResponse<TimeOffRequest>(response)
}

function buildReportQueryString(query: ReportQuery): string {
    const params = new URLSearchParams({
        startDate: query.startDate,
        endDate: query.endDate,
    })

    if (query.requestType !== undefined) {
        params.set('requestType', String(query.requestType))
    }

    if (query.employeeId && query.employeeId.trim()) {
        params.set('employeeId', query.employeeId.trim())
    }

    if (query.monthlyOvertimeHourLimit !== undefined) {
        params.set('monthlyOvertimeHourLimit', String(query.monthlyOvertimeHourLimit))
    }

    return params.toString()
}

export async function fetchReportSummary(query: ReportQuery): Promise<ReportSummary> {
    const response = await fetch(`${reportsBaseUrl}/summary?${buildReportQueryString(query)}`, {
        headers: getAuthHeaders(),
    })

    return handleResponse<ReportSummary>(response)
}

export async function fetchReportTrends(query: ReportQuery): Promise<TrendPoint[]> {
    const response = await fetch(`${reportsBaseUrl}/trends?${buildReportQueryString(query)}`, {
        headers: getAuthHeaders(),
    })

    return handleResponse<TrendPoint[]>(response)
}

export async function fetchComplianceWarnings(query: ReportQuery): Promise<ComplianceWarning[]> {
    const response = await fetch(
        `${reportsBaseUrl}/compliance-warnings?${buildReportQueryString(query)}`,
        {
            headers: getAuthHeaders(),
        },
    )

    return handleResponse<ComplianceWarning[]>(response)
}
