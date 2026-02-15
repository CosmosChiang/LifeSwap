import type {
    ComplianceWarning,
    CreateRequestPayload,
    LoginRequest,
    LoginResponse,
    ReportQuery,
    ReportSummary,
    ReviewPayload,
    TrendPoint,
    TimeOffRequest,
} from './types'

const baseUrl = '/api/requests'
const reportsBaseUrl = '/api/reports'
const authBaseUrl = '/api/auth'

// Token management
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

// Authentication APIs
export async function login(payload: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${authBaseUrl}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    })

    const data = await handleResponse<LoginResponse>(response)
    // Store token in localStorage
    localStorage.setItem('auth_token', data.token)
    return data
}

export function logout(): void {
    localStorage.removeItem('auth_token')
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

    if (query.department && query.department.trim()) {
        params.set('department', query.department.trim())
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