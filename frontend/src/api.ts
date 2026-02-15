import type {
    ComplianceWarning,
    CreateRequestPayload,
    ReportQuery,
    ReportSummary,
    ReviewPayload,
    TrendPoint,
    TimeOffRequest,
} from './types'

const baseUrl = '/api/requests'
const reportsBaseUrl = '/api/reports'

async function handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
        const text = await response.text()
        throw new Error(text || 'API request failed')
    }

    return (await response.json()) as T
}

export async function fetchRequests(): Promise<TimeOffRequest[]> {
    const response = await fetch(baseUrl)
    return handleResponse<TimeOffRequest[]>(response)
}

export async function createRequest(payload: CreateRequestPayload): Promise<TimeOffRequest> {
    const response = await fetch(baseUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function submitRequest(requestId: string): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/submit`, { method: 'POST' })
    return handleResponse<TimeOffRequest>(response)
}

export async function approveRequest(requestId: string, payload: ReviewPayload): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/approve`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function rejectRequest(requestId: string, payload: ReviewPayload): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/reject`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    })

    return handleResponse<TimeOffRequest>(response)
}

export async function cancelRequest(requestId: string): Promise<TimeOffRequest> {
    const response = await fetch(`${baseUrl}/${requestId}/cancel`, { method: 'POST' })
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
    const response = await fetch(`${reportsBaseUrl}/summary?${buildReportQueryString(query)}`)
    return handleResponse<ReportSummary>(response)
}

export async function fetchReportTrends(query: ReportQuery): Promise<TrendPoint[]> {
    const response = await fetch(`${reportsBaseUrl}/trends?${buildReportQueryString(query)}`)
    return handleResponse<TrendPoint[]>(response)
}

export async function fetchComplianceWarnings(query: ReportQuery): Promise<ComplianceWarning[]> {
    const response = await fetch(
        `${reportsBaseUrl}/compliance-warnings?${buildReportQueryString(query)}`,
    )
    return handleResponse<ComplianceWarning[]>(response)
}