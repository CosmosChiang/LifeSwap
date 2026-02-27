import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchNotifications, markNotificationAsRead } from '../api'
import type { NotificationItem } from '../types'

const mockNotifications: NotificationItem[] = [
    {
        id: 'n1',
        recipientEmployeeId: 'E001',
        title: '您的申請已通過',
        message: '補休申請已獲批准',
        isRead: false,
        createdAt: new Date().toISOString(),
    },
    {
        id: 'n2',
        recipientEmployeeId: 'E001',
        title: '通知 2',
        message: '訊息 2',
        isRead: true,
        createdAt: new Date().toISOString(),
    },
]

describe('fetchNotifications', () => {
    beforeEach(() => {
        localStorage.setItem('auth_token', 'test-token')
    })

    afterEach(() => {
        localStorage.clear()
        vi.restoreAllMocks()
    })

    it('calls /api/notifications without query string when unreadOnly is false', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(JSON.stringify(mockNotifications), { status: 200 }),
        )

        const result = await fetchNotifications(false)

        expect(fetch).toHaveBeenCalledOnce()
        const url = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0] as string
        expect(url).toBe('/api/notifications')
        expect(result).toHaveLength(2)
    })

    it('calls /api/notifications?unreadOnly=true when unreadOnly is true', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(JSON.stringify([mockNotifications[0]]), { status: 200 }),
        )

        const result = await fetchNotifications(true)

        const url = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0] as string
        expect(url).toBe('/api/notifications?unreadOnly=true')
        expect(result).toHaveLength(1)
    })

    it('throws an error when the response is not ok', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response('Unauthorized', { status: 401 }),
        )

        await expect(fetchNotifications()).rejects.toThrow('Unauthorized')
    })
})

describe('markNotificationAsRead', () => {
    beforeEach(() => {
        localStorage.setItem('auth_token', 'test-token')
    })

    afterEach(() => {
        localStorage.clear()
        vi.restoreAllMocks()
    })

    it('sends POST to /api/notifications/{id}/read', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(null, { status: 200 }),
        )

        await markNotificationAsRead('n1')

        expect(fetch).toHaveBeenCalledOnce()
        const [url, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string, RequestInit]
        expect(url).toBe('/api/notifications/n1/read')
        expect(init.method).toBe('POST')
    })

    it('throws when the response is not ok', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response('Not Found', { status: 404 }),
        )

        await expect(markNotificationAsRead('n-missing')).rejects.toThrow()
    })
})
