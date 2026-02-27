import { describe, expect, it, vi } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createAntDesignMountOptions } from './helpers/antDesignStubs'
import {
    createNotificationItem,
    createNotifications,
} from './helpers/notificationFixtures'
import { useMockLifecycle } from './helpers/lifecycle'

// ---------------------------------------------------------------------------
// Mock the api module before importing the component
// ---------------------------------------------------------------------------
const mockFetchNotifications = vi.fn()
const mockMarkNotificationAsRead = vi.fn()

vi.mock('../api', () => ({
    fetchNotifications: (...args: unknown[]) => mockFetchNotifications(...args),
    markNotificationAsRead: (...args: unknown[]) => mockMarkNotificationAsRead(...args),
}))

vi.mock('ant-design-vue', async (importOriginal) => {
    const actual = await importOriginal<Record<string, unknown>>()
    return {
        ...actual,
        message: { error: vi.fn(), success: vi.fn(), warning: vi.fn() },
    }
})

// ---------------------------------------------------------------------------
// Import component under test AFTER mocks are registered
// ---------------------------------------------------------------------------
import NotificationsVue from '../views/Notifications.vue'

const mountOptions = createAntDesignMountOptions()

describe('Notifications.vue', () => {
    useMockLifecycle()

    it('calls fetchNotifications on mount', async () => {
        mockFetchNotifications.mockResolvedValue([])

        mount(NotificationsVue, mountOptions)
        await flushPromises()

        expect(mockFetchNotifications).toHaveBeenCalledOnce()
    })

    it('unreadCount computed is correct after loading 2 notifications (1 unread)', async () => {
        const items = createNotifications([
            {
                title: '申請已通過',
                message: '您的補休申請已核准',
                isRead: false,
            },
            {
                title: '已讀通知',
                message: '歷史訊息',
                isRead: true,
            },
        ])
        mockFetchNotifications.mockResolvedValue(items)

        const wrapper = mount(NotificationsVue, mountOptions)
        await flushPromises()

        expect(mockFetchNotifications).toHaveBeenCalledOnce()
        const cardEl = wrapper.find('.a-card')
        expect(cardEl.exists()).toBe(true)
        expect(cardEl.attributes('data-title')).toBe('通知中心 (1 未讀)')
    })

    it('calls markNotificationAsRead with correct id when 標記已讀 button clicked', async () => {
        const items = [
            createNotificationItem({
                id: 'n1',
                title: '待讀通知',
                message: '點我標記',
                isRead: false,
            }),
        ]
        mockFetchNotifications.mockResolvedValue(items)
        mockMarkNotificationAsRead.mockResolvedValue(undefined)

        const wrapper = mount(NotificationsVue, mountOptions)
        await flushPromises()

        const btn = wrapper.find('button')
        expect(btn.exists()).toBe(true)
        await btn.trigger('click')
        await flushPromises()

        expect(mockMarkNotificationAsRead).toHaveBeenCalledWith('n1')
    })

    it('shows zero unread count when no notifications', async () => {
        mockFetchNotifications.mockResolvedValue([])

        const wrapper = mount(NotificationsVue, mountOptions)
        await flushPromises()

        const cardEl = wrapper.find('.a-card')
        expect(cardEl.attributes('data-title')).toBe('通知中心 (0 未讀)')
        expect(wrapper.findAll('li')).toHaveLength(0)
    })
})
