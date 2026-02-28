<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuth } from '../../composables/useAuth'
import { getRoleNameLabel } from '../../utils/roles'
import {
  HomeOutlined,
  FileTextOutlined,
  CheckSquareOutlined,
  BarChartOutlined,
  BellOutlined,
  LogoutOutlined,
  LockOutlined,
  UserOutlined,
  SyncOutlined,
} from '@ant-design/icons-vue'

const router = useRouter()
const route = useRoute()
const { currentUser, hasAnyRole, logout } = useAuth()
const { t } = useI18n()

function roleLabel(roleName: string): string {
  return getRoleNameLabel(roleName, t)
}

const allMenuItems = computed(() => [
    {
      key: '/admin/users',
      label: t('nav.adminUsers'),
      icon: UserOutlined,
      route: '/admin/users',
      roles: ['Administrator'],
    },
    {
      key: '/admin/roles',
      label: t('nav.adminRoles'),
      icon: UserOutlined,
      route: '/admin/roles',
      roles: ['Administrator'],
    },
    {
      key: '/admin/automation',
      label: t('nav.adminAutomation'),
      icon: SyncOutlined,
      route: '/admin/automation',
      roles: ['Administrator'],
    },
  {
    key: '/',
    label: t('nav.home'),
    icon: HomeOutlined,
    route: '/',
    roles: [], // Empty means accessible to all authenticated users
  },
  {
    key: '/requests',
    label: t('nav.myRequests'),
    icon: FileTextOutlined,
    route: '/requests',
    roles: [],
  },
  {
    key: '/review',
    label: t('nav.toReview'),
    icon: CheckSquareOutlined,
    route: '/review',
    roles: ['Manager', 'Administrator'],
  },
  {
    key: '/reports',
    label: t('nav.reports'),
    icon: BarChartOutlined,
    route: '/reports',
    roles: ['Manager', 'Administrator'],
  },
  {
    key: '/notifications',
    label: t('nav.notifications'),
    icon: BellOutlined,
    route: '/notifications',
    roles: [],
  },
  {
    key: '/password',
    label: t('nav.changePassword'),
    icon: LockOutlined,
    route: '/password',
    roles: [],
  },
])

// Filter menu items based on user roles
const menuItems = computed(() => {
  return allMenuItems.value.filter(item => {
    if (item.roles.length === 0) return true
    return hasAnyRole(...item.roles)
  })
})

const selectedKeys = computed(() => [route.path])

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
            {{ roleLabel(role) }}
          </a-tag>
        </div>
      </div>
    </div>

    <!-- Menu Items -->
    <a-menu theme="dark" mode="inline" :selected-keys="selectedKeys" @click="handleMenuClick">
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
        {{ t('app.logout') }}
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
  background: rgba(15, 23, 42, 0.2);
  border-bottom: 1px solid rgba(148, 163, 184, 0.2);
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%);
  flex-shrink: 0;
}

.user-details {
  flex: 1;
  min-width: 0;
  color: #e2e8f0;
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
  color: rgba(226, 232, 240, 0.7);
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
  border-top: 1px solid rgba(148, 163, 184, 0.2);
}

.logout-section .ant-btn {
  color: rgba(226, 232, 240, 0.9);
}

.logout-section .ant-btn:hover {
  color: #fff;
  background: rgba(79, 70, 229, 0.25);
}

:deep(.ant-menu-dark) {
  background: transparent;
}

:deep(.ant-menu-dark .ant-menu-item-selected) {
  background: rgba(79, 70, 229, 0.35) !important;
  border-right: 3px solid #818cf8;
}
</style>
