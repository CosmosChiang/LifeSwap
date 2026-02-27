import { afterEach, beforeEach, vi } from 'vitest'

export function useAuthTokenLifecycle(token = 'test-token'): void {
    beforeEach(() => {
        localStorage.setItem('auth_token', token)
    })

    afterEach(() => {
        localStorage.clear()
        vi.restoreAllMocks()
    })
}

export function useMockLifecycle(): void {
    beforeEach(() => {
        vi.clearAllMocks()
    })

    afterEach(() => {
        vi.restoreAllMocks()
    })
}
