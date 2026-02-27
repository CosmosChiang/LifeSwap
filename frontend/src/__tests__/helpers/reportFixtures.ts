import type { ComplianceWarning, ReportQuery, ReportSummary, TrendPoint } from '../../types'

export const baseReportQuery: ReportQuery = {
    startDate: '2025-01-01',
    endDate: '2025-01-31',
}

export function createReportSummary(overrides?: Partial<ReportSummary>): ReportSummary {
    return {
        startDate: '2026-02-01',
        endDate: '2026-02-28',
        requestType: null,
        employeeId: null,
        departmentCode: null,
        totalRequests: 3,
        submittedCount: 1,
        approvedCount: 1,
        rejectedCount: 1,
        cancelledCount: 0,
        approvedOvertimeHours: 2,
        approvalRate: 0.3333,
        ...overrides,
    }
}

export function createTrendPoints(overrides?: Partial<TrendPoint>[]): TrendPoint[] {
    const defaultTrend: TrendPoint = {
        date: '2026-02-01',
        totalRequests: 1,
        approvedCount: 1,
        rejectedCount: 0,
        cancelledCount: 0,
        approvedOvertimeHours: 2,
    }

    if (!overrides || overrides.length === 0) {
        return [defaultTrend]
    }

    return overrides.map(item => {
        const merged: TrendPoint = {
            date: item.date ?? defaultTrend.date,
            totalRequests: item.totalRequests ?? defaultTrend.totalRequests,
            approvedCount: item.approvedCount ?? defaultTrend.approvedCount,
            rejectedCount: item.rejectedCount ?? defaultTrend.rejectedCount,
            cancelledCount: item.cancelledCount ?? defaultTrend.cancelledCount,
            approvedOvertimeHours: item.approvedOvertimeHours ?? defaultTrend.approvedOvertimeHours,
        }

        return merged
    })
}

export function createComplianceWarnings(overrides?: Partial<ComplianceWarning>[]): ComplianceWarning[] {
    const defaultWarning: ComplianceWarning = {
        employeeId: 'E001',
        year: 2026,
        month: 2,
        approvedOvertimeHours: 40,
        monthlyOvertimeHourLimit: 46,
        severity: 'Warning',
        message: '接近上限',
    }

    if (!overrides || overrides.length === 0) {
        return [defaultWarning]
    }

    return overrides.map(item => {
        const merged: ComplianceWarning = {
            employeeId: item.employeeId ?? defaultWarning.employeeId,
            year: item.year ?? defaultWarning.year,
            month: item.month ?? defaultWarning.month,
            approvedOvertimeHours: item.approvedOvertimeHours ?? defaultWarning.approvedOvertimeHours,
            monthlyOvertimeHourLimit: item.monthlyOvertimeHourLimit ?? defaultWarning.monthlyOvertimeHourLimit,
            severity: item.severity ?? defaultWarning.severity,
            message: item.message ?? defaultWarning.message,
        }

        return merged
    })
}
