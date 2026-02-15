<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../../composables/useAuth'
import {
  HomeOutlined,
  FileTextOutlined,
  CheckSquareOutlined,
  BarChartOutlined,
  BellOutlined,
  LogoutOutlined,
  UserOutlined,
} from '@ant-design/icons-vue'

const router = useRouter()
const { currentUser, hasAnyRole, logout } = useAuth()

const allMenuItems = [
  {
    key: '/',
    label: '首頁',
    icon: HomeOutlined,
    route: '/',
    roles: [], // Empty means accessible to all authenticated users
  },
  {
    key: '/requests',
    label: '我的申請',
    icon: FileTextOutlined,
    route: '/requests',
    roles: [],
  },
  {
    key: '/review',
    label: '待審核',
    icon: CheckSquareOutlined,
    route: '/review',
    roles: ['Manager', 'HR', 'Administrator'],
  },
  {
    key: '/reports',
    label: '報表與預警',
    icon: BarChartOutlined,
    route: '/reports',
    roles: ['HR', 'Administrator'],
  },
  {
    key: '/notifications',
    label: '通知',
    icon: BellOutlined,
    route: '/notifications',
    roles: [],
  },
]

// Filter menu items based on user roles
const menuItems = computed(() => {
  return allMenuItems.filter(item => {
    if (item.roles.length === 0) return true
    return hasAnyRole(...item.roles)
  })
})

interface MenuInfo {
  key: string
}

function handleMenuClick(info: MenuInfo) {
  const item = menuItems.value.find(i => i.key === info.key)
  if (item) {
    router.push(item.route)
  }
}

function handleLogout() {
  logout()
  router.push('/login')
}
</script>

<template>
  <div class="sidebar-container">
    <!-- User Info Section -->
    <div v-if="currentUser" class="user-info">
      <a-avatar :size="48" class="user-avatar">
        <template #icon>
          <UserOutlined />
        </template>
      </a-avatar>
      <div class="user-details">
        <div class="user-name">{{ currentUser.username }}</div>
        <div class="user-employee-id">{{ currentUser.employeeId }}</div>
        <div class="user-roles">
          <a-tag
            v-for="role in currentUser.roles"
            :key="role"
            size="small"
            color="blue"
          >
            {{ role }}
          </a-tag>
        </div>
      </div>
    </div>

    <!-- Menu Items -->
    <a-menu theme="dark" mode="inline" @click="handleMenuClick">
      <a-menu-item v-for="item in menuItems" :key="item.key">
        <template #icon>
          <component :is="item.icon" />
        </template>
        <span>{{ item.label }}</span>
      </a-menu-item>
    </a-menu>

    <!-- Logout Button -->
    <div class="logout-section">
      <a-button
        type="text"
        block
        danger
        @click="handleLogout"
      >
        <template #icon>
          <LogoutOutlined />
        </template>
        登出
      </a-button>
    </div>
  </div>
</template>

<style scoped>
.sidebar-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.user-info {
  padding: 24px 16px;
  background: rgba(0, 0, 0, 0.2);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  background-color: #1890ff;
  flex-shrink: 0;
}

.user-details {
  flex: 1;
  min-width: 0;
  color: #fff;
}

.user-name {
  font-weight: 600;
  font-size: 14px;
  margin-bottom: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-employee-id {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.65);
  margin-bottom: 8px;
}

.user-roles {
  display: flex;
  gap: 4px;
  flex-wrap: wrap;
}

.logout-section {
  margin-top: auto;
  padding: 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.logout-section .ant-btn {
  color: rgba(255, 255, 255, 0.85);
}

.logout-section .ant-btn:hover {
  color: #fff;
  background: rgba(255, 77, 79, 0.1);
}
</style>
