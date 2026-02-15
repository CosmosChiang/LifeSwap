<script setup lang="ts">
import { ref } from 'vue'

// Mock notifications data
const notifications = ref([
  {
    id: '1',
    title: '申請已核准',
    message: '您的加班申請已被主管核准',
    timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000),
    read: false,
  },
  {
    id: '2',
    title: '申請待審核',
    message: '您有 1 個申請待主管審核',
    timestamp: new Date(Date.now() - 5 * 60 * 60 * 1000),
    read: false,
  },
  {
    id: '3',
    title: '法規預警',
    message: '您本月加班時數已接近上限，請注意',
    timestamp: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000),
    read: true,
  },
  {
    id: '4',
    title: '系統通知',
    message: '系統已於 2026-02-15 執行週期報表生成',
    timestamp: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000),
    read: true,
  },
])

function formatTimestamp(date: Date): string {
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

function markAsRead(id: string) {
  const notification = notifications.value.find(n => n.id === id)
  if (notification) {
    notification.read = true
  }
}

const unreadCount = computed(() => notifications.value.filter(n => !n.read).length)

import { computed } from 'vue'
</script>

<template>
  <div style="display: grid; gap: 24px">
    <a-card :title="`通知中心 (${unreadCount} 未讀)`">
      <a-list :data-source="notifications" :bordered="false">
        <template #renderItem="{ item }">
          <a-list-item
            :style="{
              backgroundColor: !item.read ? '#f5f5f5' : 'white',
              paddingLeft: '16px',
              paddingRight: '16px',
            }"
          >
            <template #actions>
              <a-button
                v-if="!item.read"
                type="text"
                size="small"
                @click="markAsRead(item.id)"
              >
                標記已讀
              </a-button>
              <a-tag v-if="!item.read" color="blue">未讀</a-tag>
            </template>

            <a-list-item-meta>
              <template #title>
                <span :style="{ fontWeight: !item.read ? 'bold' : 'normal' }">
                  {{ item.title }}
                </span>
              </template>
              <template #description>
                <div>
                  <p style="margin: 4px 0; color: #666">{{ item.message }}</p>
                  <span style="font-size: 12px; color: #999">
                    {{ formatTimestamp(item.timestamp) }}
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
