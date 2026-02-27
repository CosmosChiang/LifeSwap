import type { NotificationItem } from '../../types'

export function createNotificationItem(overrides?: Partial<NotificationItem>): NotificationItem {
    return {
        id: 'n1',
        recipientEmployeeId: 'E001',
        title: '通知標題',
        message: '通知內容',
        isRead: false,
        createdAt: new Date().toISOString(),
        ...overrides,
    }
}

export function createNotifications(overrides?: Partial<NotificationItem>[]): NotificationItem[] {
    if (!overrides || overrides.length === 0) {
        return [
            createNotificationItem(),
            createNotificationItem({
                id: 'n2',
                title: '通知 2',
                message: '訊息 2',
                isRead: true,
            }),
        ]
    }

    return overrides.map((item, index) =>
        createNotificationItem({
            id: `n${index + 1}`,
            ...item,
        }),
    )
}
