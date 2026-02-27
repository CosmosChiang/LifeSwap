import { expect, vi } from 'vitest'

export type FetchCall = [input: RequestInfo | URL, init?: RequestInit]

export function requireFirstFetchCall(calls: FetchCall[]): FetchCall {
    const firstCall = calls[0]
    if (!firstCall) {
        throw new Error('Expected fetch to be called at least once.')
    }

    return firstCall
}

export function toRequestUrl(input: RequestInfo | URL): string {
    if (typeof input === 'string') {
        return input
    }

    if (input instanceof URL) {
        return input.toString()
    }

    return input.url
}

export function requireRequestInit(init: RequestInit | undefined): RequestInit {
    if (!init) {
        throw new Error('Expected fetch init options.')
    }

    return init
}

function toAbsoluteUrl(url: string): URL {
    return new URL(url, 'https://test.local')
}

export function expectRequestPath(url: string, expectedPath: string): void {
    const parsed = toAbsoluteUrl(url)
    expect(parsed.pathname).toBe(expectedPath)
}

export function expectQueryParam(url: string, key: string, expectedValue: string): void {
    const parsed = toAbsoluteUrl(url)
    expect(parsed.searchParams.get(key)).toBe(expectedValue)
}

export function expectNoQueryParam(url: string, key: string): void {
    const parsed = toAbsoluteUrl(url)
    expect(parsed.searchParams.has(key)).toBe(false)
}

export function mockFetchJsonOnce(payload: unknown, status = 200) {
    return vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
        new Response(JSON.stringify(payload), { status }),
    )
}

export function mockFetchTextOnce(text: string, status: number) {
    return vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
        new Response(text, { status }),
    )
}

export function mockFetchEmptyOnce(status = 200) {
    return vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
        new Response(null, { status }),
    )
}
