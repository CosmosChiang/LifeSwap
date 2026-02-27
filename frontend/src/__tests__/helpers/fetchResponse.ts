import { vi } from 'vitest'

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
