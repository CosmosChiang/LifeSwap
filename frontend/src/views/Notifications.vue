<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { fetchNotifications, markNotificationAsRead } from '../api'
import type { NotificationItem } from '../types'

const notifications = ref<NotificationItem[]>([])
const loading = ref(false)

function formatTimestamp(value: string): string {
  const date = new Date(value)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)

  if (minutes < 1) return '剛剛'
  if (minutes < 60) return `${minutes} 分鐘前`
  if (hours < 24) return `${hours} 小時前`
  if (days < 7) return `${days} 天前`
  return date.toLocaleDateString('zh-TW')
}

async function loadNotifications() {
  loading.value = true
  try {
    notifications.value = await fetchNotifications()
  } catch {
    message.error('無法取得通知資料')
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
    message.error('更新通知失敗')
  }
}

const unreadCount = computed(() => notifications.value.filter(item => !item.isRead).length)

onMounted(() => {
  loadNotifications()
})
</script>

<template>
  <div style="display: grid; gap: 24px">
    <a-card :title="`通知中心 (${unreadCount} 未讀)`" :loading="loading">
      <a-list :data-source="notifications" :bordered="false">
        <template #renderItem="{ item }">
          <a-list-item
            :style="{
              backgroundColor: !item.isRead ? '#f5f5f5' : 'white',
              paddingLeft: '16px',
              paddingRight: '16px',
            }"
          >
            <template #actions>
              <a-button
                v-if="!item.isRead"
                type="text"
                size="small"
                @click="markAsRead(item.id)"
              >
                標記已讀
              </a-button>
              <a-tag v-if="!item.isRead" color="blue">未讀</a-tag>
            </template>

            <a-list-item-meta>
              <template #title>
                <span :style="{ fontWeight: !item.isRead ? 'bold' : 'normal' }">
                  {{ item.title }}
                </span>
              </template>
              <template #description>
                <div>
                  <p style="margin: 4px 0; color: #666">{{ item.message }}</p>
                  <span style="font-size: 12px; color: #999">
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
