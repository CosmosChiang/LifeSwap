import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchReportSummary, fetchComplianceWarnings } from '../api'
import type { ReportQuery, ReportSummary, ComplianceWarning } from '../types'

const baseQuery: ReportQuery = {
    startDate: '2025-01-01',
    endDate: '2025-01-31',
}

const mockSummary: ReportSummary = {
    startDate: '2025-01-01',
    endDate: '2025-01-31',
    requestType: null,
    employeeId: null,
    departmentCode: null,
    totalRequests: 5,
    submittedCount: 5,
    approvedCount: 3,
    rejectedCount: 1,
    cancelledCount: 1,
    approvedOvertimeHours: 12,
    approvalRate: 0.6,
}

describe('fetchReportSummary', () => {
    beforeEach(() => {
        localStorage.setItem('auth_token', 'test-token')
    })

    afterEach(() => {
        localStorage.clear()
        vi.restoreAllMocks()
    })

    it('builds query string with startDate and endDate', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(JSON.stringify(mockSummary), { status: 200 }),
        )

        await fetchReportSummary(baseQuery)

        const url = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0] as string
        expect(url).toContain('startDate=2025-01-01')
        expect(url).toContain('endDate=2025-01-31')
    })

    it('includes departmentCode in query string when provided', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(JSON.stringify(mockSummary), { status: 200 }),
        )

        await fetchReportSummary({ ...baseQuery, departmentCode: 'ENG' })

        const url = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0] as string
        expect(url).toContain('departmentCode=ENG')
    })

    it('omits departmentCode when empty string', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(JSON.stringify(mockSummary), { status: 200 }),
        )

        await fetchReportSummary({ ...baseQuery, departmentCode: '' })

        const url = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0] as string
        expect(url).not.toContain('departmentCode')
    })

    it('includes requestType when numeric value is provided', async () => {
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(JSON.stringify(mockSummary), { status: 200 }),
        )

        await fetchReportSummary({ ...baseQuery, requestType: 0 })

        const url = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0] as string
        expect(url).toContain('requestType=0')
    })
})

describe('fetchComplianceWarnings', () => {
    beforeEach(() => {
        localStorage.setItem('auth_token', 'test-token')
    })

    afterEach(() => {
        localStorage.clear()
        vi.restoreAllMocks()
    })

    it('calls compliance-warnings endpoint with departmentCode when provided', async () => {
        const warnings: ComplianceWarning[] = []
        vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
            new Response(JSON.stringify(warnings), { status: 200 }),
        )

        await fetchComplianceWarnings({ ...baseQuery, departmentCode: 'EN' })

        const url = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0] as string
        expect(url).toContain('/api/reports/compliance-warnings')
        expect(url).toContain('departmentCode=EN')
    })
})
