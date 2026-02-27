<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'
import { useAuth } from '../../composables/useAuth'
import { LogoutOutlined } from '@ant-design/icons-vue'

const collapsed = ref(false)
const router = useRouter()
const { logout } = useAuth()
const { t } = useI18n()

function handleBreakpoint(broken: boolean) {
  collapsed.value = broken
}

function handleLogout() {
  logout()
  message.success(t('app.logoutSuccess'))
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
          <h3 class="app-header-title">{{ t('app.systemTitle') }}</h3>
          <div class="app-header-actions">
            <LocaleSwitcher />
            <button type="button" class="app-logout-btn" @click="handleLogout">
              <LogoutOutlined />
              {{ t('app.logout') }}
            </button>
          </div>
        </div>
      </a-layout-header>

      <a-layout-content class="app-content">
        <div class="app-content-inner">
          <slot />
        </div>
      </a-layout-content>

      <a-layout-footer class="app-footer">
        <p class="app-footer-text">{{ t('app.footer', { version: '1.0.0' }) }}</p>
      </a-layout-footer>
    </a-layout>
  </a-layout>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
}

.app-sider {
  background: linear-gradient(180deg, #172554 0%, #1e293b 100%);
  box-shadow: 10px 0 35px -25px rgba(15, 23, 42, 0.8);
}

.app-brand {
  padding: 20px 16px;
  text-align: center;
  border-bottom: 1px solid rgba(148, 163, 184, 0.2);
}

.app-brand-title {
  color: #e2e8f0;
  margin: 0;
  font-size: 24px;
  font-weight: 800;
  letter-spacing: 0.04em;
}

.app-main-layout {
  min-width: 0;
}

.app-header {
  background: linear-gradient(90deg, #ffffff 0%, #f8faff 100%);
  padding: 0 20px;
  box-shadow: 0 8px 24px -20px rgba(15, 23, 42, 0.45);
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

.app-header-actions {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.app-header-title {
  margin: 0;
  font-size: 18px;
  color: #1e293b;
  font-weight: 700;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.app-logout-btn {
  background: #eef2ff;
  color: #3730a3;
  border: 1px solid #c7d2fe;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 10px;
  transition: all 0.2s;
  flex-shrink: 0;
}

.app-logout-btn:hover {
  background: #e0e7ff;
}

.app-content {
  background: transparent;
  padding: 20px;
}

.app-content-inner {
  width: 100%;
  max-width: 1440px;
  margin: 0 auto;
}

.app-footer {
  text-align: center;
  background: transparent;
  padding: 16px;
}

.app-footer-text {
  margin: 0;
  font-size: 12px;
  color: #64748b;
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
