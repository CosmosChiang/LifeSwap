<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { getRoles } from '../api'
import type { RoleItem } from '../types'

const roles = ref<RoleItem[]>([])
const loading = ref(false)

async function fetchRoles() {
  loading.value = true
  try {
    roles.value = await getRoles()
  } catch (e) {
    message.error('無法取得角色列表')
  } finally {
    loading.value = false
  }
}

onMounted(fetchRoles)
</script>

<template>
  <div>
    <h2>角色管理</h2>
    <a-table :dataSource="roles" :loading="loading" rowKey="id">
      <a-table-column title="角色名稱" dataIndex="name" key="name" />
      <a-table-column title="描述" dataIndex="description" key="description" />
    </a-table>
  </div>
</template>
