<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import { useAuth } from '../../composables/useAuth'
import { LogoutOutlined } from '@ant-design/icons-vue'

const collapsed = ref(false)
const router = useRouter()
const { logout } = useAuth()

function handleBreakpoint(broken: boolean) {
  collapsed.value = broken
}

function handleLogout() {
  logout()
  message.success('已登出')
  router.push({ name: 'Login' })
}
</script>

<template>
  <a-layout class="app-shell">
    <a-layout-sider :collapsed="collapsed" class="app-sider" :trigger="null" collapsible breakpoint="lg" :width="250"
      :collapsed-width="72" @breakpoint="handleBreakpoint" @update:collapsed="collapsed = $event">
      <div class="app-brand">
        <h2 class="app-brand-title">LifeSwap</h2>
      </div>
      <AppSidebar />
    </a-layout-sider>

    <a-layout class="app-main-layout">
      <a-layout-header class="app-header">
        <div class="app-header-inner">
          <h3 class="app-header-title">員工加班/補休管理系統</h3>
          <button type="button" class="app-logout-btn" @click="handleLogout">
            <LogoutOutlined />
            登出
          </button>
        </div>
      </a-layout-header>

      <a-layout-content class="app-content">
        <div class="app-content-inner">
          <slot />
        </div>
      </a-layout-content>

      <a-layout-footer class="app-footer">
        <p class="app-footer-text">LifeSwap © 2026 All Rights Reserved. Version 1.0.0</p>
      </a-layout-footer>
    </a-layout>
  </a-layout>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
}

.app-sider {
  background: #001529;
}

.app-brand {
  padding: 16px;
  text-align: center;
}

.app-brand-title {
  color: #fff;
  margin: 0;
  font-size: 28px;
}

.app-main-layout {
  min-width: 0;
}

.app-header {
  background: #fff;
  padding: 0 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  position: sticky;
  top: 0;
  z-index: 10;
}

.app-header-inner {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.app-header-title {
  margin: 0;
  font-size: 20px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.app-logout-btn {
  background: none;
  border: none;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 4px;
  transition: background 0.2s;
  flex-shrink: 0;
}

.app-logout-btn:hover {
  background: #f5f5f5;
}

.app-content {
  background: #f5f5f5;
  padding: 20px;
}

.app-content-inner {
  width: 100%;
  max-width: 1440px;
  margin: 0 auto;
}

.app-footer {
  text-align: center;
  background: #fff;
  padding: 16px;
}

.app-footer-text {
  margin: 0;
  font-size: 12px;
  color: #888;
}

@media (max-width: 768px) {
  .app-content {
    padding: 12px;
  }

  .app-header-title {
    font-size: 16px;
  }

  .app-brand-title {
    font-size: 20px;
  }
}
</style>
