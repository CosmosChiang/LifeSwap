<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'
import { fetchNotifications, markNotificationAsRead } from '../api'
import type { NotificationItem } from '../types'

const notifications = ref<NotificationItem[]>([])
const loading = ref(false)
const { t, locale } = useI18n()

function formatTimestamp(value: string): string {
  const date = new Date(value)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)

  if (minutes < 1) return t('notifications.time.justNow')
  if (minutes < 60) return t('notifications.time.minutesAgo', { count: minutes })
  if (hours < 24) return t('notifications.time.hoursAgo', { count: hours })
  if (days < 7) return t('notifications.time.daysAgo', { count: days })
  return date.toLocaleDateString(locale.value === 'zh-TW' ? 'zh-TW' : 'en-US')
}

async function loadNotifications() {
  loading.value = true
  try {
    notifications.value = await fetchNotifications()
  } catch {
    message.error(t('notifications.loadFailed'))
  } finally {
    loading.value = false
  }
}

async function markAsRead(id: string) {
  try {
    await markNotificationAsRead(id)
    const target = notifications.value.find(item => item.id === id)
    if (target) {
      target.isRead = true
    }
  } catch {
    message.error(t('notifications.updateFailed'))
  }
}

const unreadCount = computed(() => notifications.value.filter(item => !item.isRead).length)

onMounted(() => {
  loadNotifications()
})
</script>

<template>
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">{{ t('notifications.pageTitle') }}</h2>
    </div>

    <a-card :title="t('notifications.cardTitle', { count: unreadCount })" :loading="loading">
      <a-list :data-source="notifications" :bordered="false">
        <template #renderItem="{ item }">
          <a-list-item
            :class="['notification-item', { 'notification-item--unread': !item.isRead }]"
          >
            <template #actions>
              <a-button
                v-if="!item.isRead"
                type="text"
                size="small"
                @click="markAsRead(item.id)"
              >
                {{ t('notifications.markRead') }}
              </a-button>
              <a-tag v-if="!item.isRead" color="blue">{{ t('notifications.unread') }}</a-tag>
            </template>

            <a-list-item-meta>
              <template #title>
                <span :class="['notification-title', { 'notification-title--unread': !item.isRead }]">
                  {{ item.title }}
                </span>
              </template>
              <template #description>
                <div>
                  <p class="notification-message">{{ item.message }}</p>
                  <span class="notification-time">
                    {{ formatTimestamp(item.createdAt) }}
                  </span>
                </div>
              </template>
            </a-list-item-meta>
          </a-list-item>
        </template>
      </a-list>
    </a-card>
  </div>
</template>

<style scoped>
.notification-item {
  padding-left: 16px;
  padding-right: 16px;
  border-radius: 10px;
}

.notification-item--unread {
  background: #eef2ff;
}

.notification-title {
  color: #334155;
}

.notification-title--unread {
  font-weight: 700;
  color: #1e293b;
}

.notification-message {
  margin: 4px 0;
  color: #475569;
}

.notification-time {
  font-size: 12px;
  color: #94a3b8;
}
</style>
