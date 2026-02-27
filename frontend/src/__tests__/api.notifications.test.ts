import { describe, expect, it } from 'vitest'
import { fetchNotifications, markNotificationAsRead } from '../api'
import {
    expectQueryParam,
    expectRequestPath,
    mockFetchEmptyOnce,
    mockFetchJsonOnce,
    mockFetchTextOnce,
    requireFirstFetchCall,
    requireRequestInit,
    toRequestUrl,
} from './helpers/httpTestHelpers'
import { createNotifications } from './helpers/notificationFixtures'
import { useAuthTokenLifecycle } from './helpers/lifecycle'

const mockNotifications = createNotifications([
    {
        title: '您的申請已通過',
        message: '補休申請已獲批准',
        isRead: false,
    },
    {
        title: '通知 2',
        message: '訊息 2',
        isRead: true,
    },
])

describe('fetchNotifications', () => {
    useAuthTokenLifecycle()

    it('calls /api/notifications without query string when unreadOnly is false', async () => {
        const fetchSpy = mockFetchJsonOnce(mockNotifications)

        const result = await fetchNotifications(false)

        expect(fetchSpy).toHaveBeenCalledOnce()
        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expectRequestPath(requestUrl, '/api/notifications')
        expect(result).toHaveLength(2)
    })

    it('calls /api/notifications?unreadOnly=true when unreadOnly is true', async () => {
        const fetchSpy = mockFetchJsonOnce([mockNotifications[0]])

        const result = await fetchNotifications(true)

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expectRequestPath(requestUrl, '/api/notifications')
        expectQueryParam(requestUrl, 'unreadOnly', 'true')
        expect(result).toHaveLength(1)
    })

    it('throws an error when the response is not ok', async () => {
        mockFetchTextOnce('Unauthorized', 401)

        await expect(fetchNotifications()).rejects.toThrow('Unauthorized')
    })
})

describe('markNotificationAsRead', () => {
    useAuthTokenLifecycle()

    it('sends POST to /api/notifications/{id}/read', async () => {
        const fetchSpy = mockFetchEmptyOnce()

        await markNotificationAsRead('n1')

        expect(fetchSpy).toHaveBeenCalledOnce()
        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url, init] = firstCall
        const requestUrl = toRequestUrl(url)
        expectRequestPath(requestUrl, '/api/notifications/n1/read')
        const requestInit = requireRequestInit(init)
        expect(requestInit.method).toBe('POST')
    })

    it('throws when the response is not ok', async () => {
        mockFetchTextOnce('Not Found', 404)

        await expect(markNotificationAsRead('n-missing')).rejects.toThrow()
    })
})
