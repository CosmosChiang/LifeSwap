import { describe, expect, it, vi } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import type { NotificationItem } from '../types'
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

// Shallow-stub all ant-design-vue components so they never actually mount
const antStubs = {
    'a-card': {
        props: ['title', 'loading'],
        template: '<div class="a-card" :data-title="title"><slot /></div>',
    },
    'a-list': {
        props: ['dataSource', 'bordered'],
        template:
            '<ul><template v-for="item in dataSource" :key="item.id"><slot name="renderItem" :item="item" /></template></ul>',
    },
    'a-list-item': {
        template: '<li><slot /><slot name="actions" /></li>',
    },
    'a-list-item-meta': {
        template: '<div><slot name="title" /><slot name="description" /></div>',
    },
    'a-button': {
        props: ['type', 'size'],
        emits: ['click'],
        template: '<button @click="$emit(\'click\')"><slot /></button>',
    },
    'a-tag': {
        props: ['color'],
        template: '<span><slot /></span>',
    },
}

const mountOptions = { global: { stubs: antStubs } }

describe('Notifications.vue', () => {
    useMockLifecycle()

    it('calls fetchNotifications on mount', async () => {
        mockFetchNotifications.mockResolvedValue([])

        mount(NotificationsVue, mountOptions)
        await flushPromises()

        expect(mockFetchNotifications).toHaveBeenCalledOnce()
    })

    it('unreadCount computed is correct after loading 2 notifications (1 unread)', async () => {
        const items: NotificationItem[] = [
            {
                id: 'n1',
                recipientEmployeeId: 'E001',
                title: '申請已通過',
                message: '您的補休申請已核准',
                isRead: false,
                createdAt: new Date().toISOString(),
            },
            {
                id: 'n2',
                recipientEmployeeId: 'E001',
                title: '已讀通知',
                message: '歷史訊息',
                isRead: true,
                createdAt: new Date().toISOString(),
            },
        ]
        mockFetchNotifications.mockResolvedValue(items)

        const wrapper = mount(NotificationsVue, mountOptions)
        await flushPromises()

        expect(mockFetchNotifications).toHaveBeenCalledOnce()
        const cardEl = wrapper.find('.a-card')
        expect(cardEl.exists()).toBe(true)
        expect(cardEl.attributes('data-title')).toBe('通知中心 (1 未讀)')
    })

    it('calls markNotificationAsRead with correct id when 標記已讀 button clicked', async () => {
        const items: NotificationItem[] = [
            {
                id: 'n1',
                recipientEmployeeId: 'E001',
                title: '待讀通知',
                message: '點我標記',
                isRead: false,
                createdAt: new Date().toISOString(),
            },
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
