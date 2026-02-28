import { describe, expect, it } from 'vitest'
import {
    fetchAutomationStatus,
    runAutomationReminder,
    runAutomationReport,
    updateAutomationSchedulerState,
} from '../api'
import {
    expectRequestPath,
    mockFetchEmptyOnce,
    mockFetchJsonOnce,
    mockFetchTextOnce,
    requireFirstFetchCall,
    requireRequestInit,
    toRequestUrl,
} from './helpers/httpTestHelpers'
import { useAuthTokenLifecycle } from './helpers/lifecycle'

describe('fetchAutomationStatus', () => {
    useAuthTokenLifecycle()

    it('calls /api/automation/status and returns payload', async () => {
        const fetchSpy = mockFetchJsonOnce({
            schedulerEnabled: true,
            reminderIntervalMinutes: 5,
            reportIntervalMinutes: 60,
            maxRetryCount: 2,
            workflows: [],
        })

        const result = await fetchAutomationStatus()

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expectRequestPath(requestUrl, '/api/automation/status')
        expect(result.schedulerEnabled).toBe(true)
    })
})

describe('runAutomationReminder', () => {
    useAuthTokenLifecycle()

    it('sends POST to /api/automation/run-reminder', async () => {
        const fetchSpy = mockFetchEmptyOnce()

        await runAutomationReminder()

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url, init] = firstCall
        const requestUrl = toRequestUrl(url)
        expectRequestPath(requestUrl, '/api/automation/run-reminder')
        const requestInit = requireRequestInit(init)
        expect(requestInit.method).toBe('POST')
    })

    it('throws when response is not ok', async () => {
        mockFetchTextOnce('Reminder workflow failed.', 500)

        await expect(runAutomationReminder()).rejects.toThrow('Reminder workflow failed.')
    })
})

describe('runAutomationReport', () => {
    useAuthTokenLifecycle()

    it('sends POST to /api/automation/run-report', async () => {
        const fetchSpy = mockFetchEmptyOnce()

        await runAutomationReport()

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url, init] = firstCall
        const requestUrl = toRequestUrl(url)
        expectRequestPath(requestUrl, '/api/automation/run-report')
        const requestInit = requireRequestInit(init)
        expect(requestInit.method).toBe('POST')
    })

})

describe('updateAutomationSchedulerState', () => {
    useAuthTokenLifecycle()

    it('sends POST to /api/automation/scheduler-state with payload', async () => {
        const fetchSpy = mockFetchEmptyOnce()

        await updateAutomationSchedulerState(false)

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url, init] = firstCall
        const requestUrl = toRequestUrl(url)
        expectRequestPath(requestUrl, '/api/automation/scheduler-state')
        const requestInit = requireRequestInit(init)
        expect(requestInit.method).toBe('POST')
        expect(requestInit.body).toBe(JSON.stringify({ enabled: false }))
    })
})
