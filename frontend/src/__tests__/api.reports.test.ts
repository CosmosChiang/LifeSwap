import { describe, expect, it } from 'vitest'
import { fetchReportSummary, fetchComplianceWarnings } from '../api'
import { mockFetchJsonOnce } from './helpers/fetchResponse'
import { requireFirstFetchCall, toRequestUrl } from './helpers/fetchMock'
import { useAuthTokenLifecycle } from './helpers/lifecycle'
import {
    baseReportQuery,
    createComplianceWarnings,
    createReportSummary,
} from './helpers/reportFixtures'

const baseQuery = baseReportQuery
const mockSummary = createReportSummary({
    startDate: '2025-01-01',
    endDate: '2025-01-31',
    totalRequests: 5,
    submittedCount: 5,
    approvedCount: 3,
    rejectedCount: 1,
    cancelledCount: 1,
    approvedOvertimeHours: 12,
    approvalRate: 0.6,
})

describe('fetchReportSummary', () => {
    useAuthTokenLifecycle()

    it('builds query string with startDate and endDate', async () => {
        const fetchSpy = mockFetchJsonOnce(mockSummary)

        await fetchReportSummary(baseQuery)

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expect(requestUrl).toContain('startDate=2025-01-01')
        expect(requestUrl).toContain('endDate=2025-01-31')
    })

    it('includes departmentCode in query string when provided', async () => {
        const fetchSpy = mockFetchJsonOnce(mockSummary)

        await fetchReportSummary({ ...baseQuery, departmentCode: 'ENG' })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expect(requestUrl).toContain('departmentCode=ENG')
    })

    it('omits departmentCode when empty string', async () => {
        const fetchSpy = mockFetchJsonOnce(mockSummary)

        await fetchReportSummary({ ...baseQuery, departmentCode: '' })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expect(requestUrl).not.toContain('departmentCode')
    })

    it('includes requestType when numeric value is provided', async () => {
        const fetchSpy = mockFetchJsonOnce(mockSummary)

        await fetchReportSummary({ ...baseQuery, requestType: 0 })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expect(requestUrl).toContain('requestType=0')
    })
})

describe('fetchComplianceWarnings', () => {
    useAuthTokenLifecycle()

    it('calls compliance-warnings endpoint with departmentCode when provided', async () => {
        const warnings = createComplianceWarnings()
        const fetchSpy = mockFetchJsonOnce(warnings)

        await fetchComplianceWarnings({ ...baseQuery, departmentCode: 'EN' })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expect(requestUrl).toContain('/api/reports/compliance-warnings')
        expect(requestUrl).toContain('departmentCode=EN')
    })
})
