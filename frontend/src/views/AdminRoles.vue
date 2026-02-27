<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'
import { getRoles } from '../api'
import type { RoleItem } from '../types'
import { getRoleDescriptionLabel, getRoleNameLabel } from '../utils/roles'

const roles = ref<RoleItem[]>([])
const loading = ref(false)
const { t } = useI18n()

const localizedRoles = computed(() =>
  roles.value.map(role => ({
    ...role,
    localizedName: getRoleNameLabel(role.name, t),
    localizedDescription: getRoleDescriptionLabel(role.name, role.description, t),
  })),
)

async function fetchRoles() {
  loading.value = true
  try {
    roles.value = await getRoles()
  } catch (e) {
    message.error(t('adminRoles.loadFailed'))
  } finally {
    loading.value = false
  }
}

onMounted(fetchRoles)
</script>

<template>
  <div class="page-stack">
    <h2 class="page-title">{{ t('adminRoles.pageTitle') }}</h2>
    <a-table :dataSource="localizedRoles" :loading="loading" rowKey="id">
      <a-table-column :title="t('adminRoles.columns.name')" dataIndex="localizedName" key="name" />
      <a-table-column :title="t('adminRoles.columns.description')" dataIndex="localizedDescription" key="description" />
    </a-table>
  </div>
</template>
