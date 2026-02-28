import { describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createAntDesignMountOptions } from './helpers/antDesignStubs'
import {
    createComplianceWarnings,
    createReportSummary,
    createTrendPoints,
} from './helpers/reportFixtures'
import { useMockLifecycle } from './helpers/lifecycle'

const mockFetchReportSummary = vi.fn()
const mockFetchReportTrends = vi.fn()
const mockFetchComplianceWarnings = vi.fn()

vi.mock('../api', () => ({
    fetchReportSummary: (...args: unknown[]) => mockFetchReportSummary(...args),
    fetchReportTrends: (...args: unknown[]) => mockFetchReportTrends(...args),
    fetchComplianceWarnings: (...args: unknown[]) => mockFetchComplianceWarnings(...args),
}))

import ReportsVue from '../views/Reports.vue'

const mountOptions = createAntDesignMountOptions()

describe('Reports.vue', () => {
    useMockLifecycle()

    it('loads summary, trends, and compliance warnings on mount', async () => {
        mockFetchReportSummary.mockResolvedValue(createReportSummary())
        mockFetchReportTrends.mockResolvedValue(createTrendPoints())
        mockFetchComplianceWarnings.mockResolvedValue(createComplianceWarnings())

        const wrapper = mount(ReportsVue, mountOptions)
        await flushPromises()

        expect(mockFetchReportSummary).toHaveBeenCalledOnce()
        expect(mockFetchReportTrends).toHaveBeenCalledOnce()
        expect(mockFetchComplianceWarnings).toHaveBeenCalledOnce()

        const cardTitles = wrapper.findAll('.a-card').map(card => card.attributes('data-title'))
        expect(cardTitles).toContain('加班申請摘要')
    })

    it('shows error alert when report loading fails', async () => {
        mockFetchReportSummary.mockRejectedValue(new Error('報表載入失敗'))
        mockFetchReportTrends.mockResolvedValue([])
        mockFetchComplianceWarnings.mockResolvedValue([])

        const wrapper = mount(ReportsVue, mountOptions)
        await flushPromises()

        const alert = wrapper.find('.a-alert')
        expect(alert.exists()).toBe(true)
        expect(alert.attributes('data-message')).toBe('報表載入失敗')
    })

    it('re-fetches with departmentCode when user updates department filter and clicks reload', async () => {
        mockFetchReportSummary.mockResolvedValue(createReportSummary())
        mockFetchReportTrends.mockResolvedValue(createTrendPoints())
        mockFetchComplianceWarnings.mockResolvedValue(createComplianceWarnings())

        const wrapper = mount(ReportsVue, mountOptions)
        await flushPromises()

        mockFetchReportSummary.mockClear()
        mockFetchReportTrends.mockClear()
        mockFetchComplianceWarnings.mockClear()

        const departmentInput = wrapper.find('input[placeholder="例如 ENG"]')
        expect(departmentInput.exists()).toBe(true)
        await departmentInput.setValue('ENG')

        const reloadButton = wrapper.find('button')
        expect(reloadButton.exists()).toBe(true)
        await reloadButton.trigger('click')
        await flushPromises()

        expect(mockFetchReportSummary).toHaveBeenCalledOnce()
        expect(mockFetchReportTrends).toHaveBeenCalledOnce()
        expect(mockFetchComplianceWarnings).toHaveBeenCalledOnce()

        const summaryQuery = mockFetchReportSummary.mock.calls[0]?.[0] as { departmentCode?: string } | undefined
        expect(summaryQuery?.departmentCode).toBe('ENG')
    })
})
