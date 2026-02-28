import { describe, it } from 'vitest'
import { fetchReportSummary, fetchComplianceWarnings } from '../api'
import {
    expectNoQueryParam,
    expectQueryParam,
    expectRequestPath,
    mockFetchJsonOnce,
    requireFirstFetchCall,
    toRequestUrl,
} from './helpers/httpTestHelpers'
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
        expectRequestPath(requestUrl, '/api/reports/summary')
        expectQueryParam(requestUrl, 'startDate', '2025-01-01')
        expectQueryParam(requestUrl, 'endDate', '2025-01-31')
    })

    it('includes departmentCode in query string when provided', async () => {
        const fetchSpy = mockFetchJsonOnce(mockSummary)

        await fetchReportSummary({ ...baseQuery, departmentCode: 'ENG' })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expectQueryParam(requestUrl, 'departmentCode', 'ENG')
    })

    it('omits departmentCode when empty string', async () => {
        const fetchSpy = mockFetchJsonOnce(mockSummary)

        await fetchReportSummary({ ...baseQuery, departmentCode: '' })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expectNoQueryParam(requestUrl, 'departmentCode')
    })

    it('includes requestType when numeric value is provided', async () => {
        const fetchSpy = mockFetchJsonOnce(mockSummary)

        await fetchReportSummary({ ...baseQuery, requestType: 0 })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expectQueryParam(requestUrl, 'requestType', '0')
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
        expectRequestPath(requestUrl, '/api/reports/compliance-warnings')
        expectQueryParam(requestUrl, 'departmentCode', 'EN')
    })

    it('includes requestType in compliance-warnings query when provided', async () => {
        const warnings = createComplianceWarnings()
        const fetchSpy = mockFetchJsonOnce(warnings)

        await fetchComplianceWarnings({ ...baseQuery, requestType: 1 })

        const firstCall = requireFirstFetchCall(fetchSpy.mock.calls)
        const [url] = firstCall
        const requestUrl = toRequestUrl(url)
        expectQueryParam(requestUrl, 'requestType', '1')
    })
})
