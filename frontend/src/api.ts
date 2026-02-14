import type {
    CreateRequestPayload,
    ReviewPayload,
    TimeOffRequest,
} from './types'

const baseUrl = '/api/requests'

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