<script setup lang="ts">
import { ref } from 'vue'
import { LogoutOutlined } from '@ant-design/icons-vue'
import AppSidebar from './AppSidebar.vue'

const collapsed = ref(false)

function handleLogout() {
  // TODO: Implement logout logic
  alert('Logout not implemented yet')
}
</script>

<template>
  <a-layout style="min-height: 100vh">
    <a-layout-sider
      v-model:collapsed="collapsed"
      :trigger="null"
      collapsible
      :width="250"
      :collapsed-width="60"
      style="background: #001529"
    >
      <div style="padding: 16px; text-align: center">
        <h2 style="color: white; margin: 0; font-size: 18px">LifeSwap</h2>
      </div>
      <AppSidebar />
    </a-layout-sider>

    <a-layout>
      <a-layout-header style="background: white; padding: 0 16px; box-shadow: 0 2px 8px rgba(0,0,0,0.06)">
        <div style="display: flex; justify-content: space-between; align-items: center; height: 100%">
          <h3 style="margin: 0">員工加班/補休管理系統</h3>
          <button
            type="button"
            style="background: none; border: none; cursor: pointer; display: flex; align-items: center; gap: 8px; padding: 8px 12px; border-radius: 4px; transition: background 0.3s"
            @click="handleLogout"
          >
            <LogoutOutlined />
            登出
          </button>
        </div>
      </a-layout-header>

      <a-layout-content style="padding: 24px; background: #f5f5f5">
        <router-view />
      </a-layout-content>

      <a-layout-footer style="text-align: center; background: white; padding: 16px">
        <p style="margin: 0; font-size: 12px; color: #888">
          LifeSwap © 2026 All Rights Reserved. Version 1.0.0
        </p>
      </a-layout-footer>
    </a-layout>
  </a-layout>
</template>
