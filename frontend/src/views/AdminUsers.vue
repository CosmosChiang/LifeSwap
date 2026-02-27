<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { message, Modal } from 'ant-design-vue'
import { assignUserRoles, createUser, deleteUser, getRoles, getUsers, resetUserPassword, updateUser } from '../api'
import type { CreateUserPayload, RoleItem, UpdateUserPayload, UserItem } from '../types'

const users = ref<UserItem[]>([])
const roles = ref<RoleItem[]>([])
const loading = ref(false)
const savingUserId = ref<string | null>(null)
const createModalVisible = ref(false)
const editModalVisible = ref(false)
const resetPasswordModalVisible = ref(false)
const submittingCreate = ref(false)
const submittingEdit = ref(false)
const submittingResetPassword = ref(false)
const editingUser = ref<UserItem | null>(null)
const passwordTargetUser = ref<UserItem | null>(null)

const createForm = reactive({
  username: '',
  email: '',
  employeeId: '',
  password: '',
  roleId: '',
})

const editForm = reactive({
  email: '',
  isActive: true,
  roleId: '',
})

const resetPasswordForm = reactive({
  newPassword: '',
  confirmPassword: '',
})

const roleOptions = computed(() => roles.value.map(role => ({ value: role.id, label: role.name })))

function firstRoleIdByName(roleName: string): string {
  const role = roles.value.find(item => item.name === roleName)
  return role ? role.id : ''
}

function roleIdOf(user: UserItem): string | undefined {
  const firstRole = user.roles[0]
  return firstRole ? firstRole.id : undefined
}

function roleNameOf(user: UserItem): string {
  const firstRole = user.roles[0]
  return firstRole ? firstRole.name : '未設定'
}

function resetCreateForm() {
  createForm.username = ''
  createForm.email = ''
  createForm.employeeId = ''
  createForm.password = ''
  createForm.roleId = firstRoleIdByName('Employee')
}

function openCreateModal() {
  resetCreateForm()
  createModalVisible.value = true
}

function openEditModal(user: UserItem) {
  editingUser.value = user
  editForm.email = user.email
  editForm.isActive = user.isActive
  editForm.roleId = roleIdOf(user) ?? ''
  editModalVisible.value = true
}

function openResetPasswordModal(user: UserItem) {
  passwordTargetUser.value = user
  resetPasswordForm.newPassword = ''
  resetPasswordForm.confirmPassword = ''
  resetPasswordModalVisible.value = true
}

async function loadData() {
  loading.value = true
  try {
    const [userList, roleList] = await Promise.all([getUsers(), getRoles()])
    users.value = userList
    roles.value = roleList
  } catch {
    message.error('無法取得使用者或角色資料')
  } finally {
    loading.value = false
  }
}

function validateCreateForm(): boolean {
  if (!createForm.username.trim()) {
    message.error('請輸入帳號')
    return false
  }

  if (!createForm.email.trim()) {
    message.error('請輸入 Email')
    return false
  }

  if (!createForm.employeeId.trim()) {
    message.error('請輸入員工編號')
    return false
  }

  if (!createForm.password.trim()) {
    message.error('請輸入密碼')
    return false
  }

  if (createForm.password.length < 8) {
    message.error('密碼至少 8 碼')
    return false
  }

  if (!createForm.roleId) {
    message.error('請選擇角色')
    return false
  }

  return true
}

async function handleCreateUser() {
  if (!validateCreateForm()) {
    return
  }

  submittingCreate.value = true
  try {
    const payload: CreateUserPayload = {
      username: createForm.username.trim(),
      email: createForm.email.trim(),
      employeeId: createForm.employeeId.trim(),
      password: createForm.password,
      roleIds: [createForm.roleId],
    }

    await createUser(payload)
    createModalVisible.value = false
    message.success('帳號新增成功')
    await loadData()
  } catch {
    message.error('帳號新增失敗')
  } finally {
    submittingCreate.value = false
  }
}

async function handleUpdateUser() {
  if (!editingUser.value) {
    return
  }

  if (!editForm.email.trim()) {
    message.error('請輸入 Email')
    return
  }

  if (!editForm.roleId) {
    message.error('請選擇角色')
    return
  }

  submittingEdit.value = true
  try {
    const payload: UpdateUserPayload = {
      email: editForm.email.trim(),
      isActive: editForm.isActive,
      roleIds: [editForm.roleId],
    }

    await updateUser(editingUser.value.id, payload)
    editModalVisible.value = false
    message.success('帳號更新成功')
    await loadData()
  } catch {
    message.error('帳號更新失敗')
  } finally {
    submittingEdit.value = false
  }
}

async function handleResetPassword() {
  if (!passwordTargetUser.value) {
    return
  }

  if (!resetPasswordForm.newPassword || !resetPasswordForm.confirmPassword) {
    message.error('請完整輸入新密碼欄位')
    return
  }

  if (resetPasswordForm.newPassword.length < 8) {
    message.error('新密碼至少 8 碼')
    return
  }

  if (resetPasswordForm.newPassword !== resetPasswordForm.confirmPassword) {
    message.error('新密碼與確認密碼不一致')
    return
  }

  submittingResetPassword.value = true
  try {
    await resetUserPassword(passwordTargetUser.value.id, resetPasswordForm.newPassword)
    resetPasswordModalVisible.value = false
    message.success(`已重設 ${passwordTargetUser.value.username} 的密碼`)
  } catch {
    message.error('密碼重設失敗')
  } finally {
    submittingResetPassword.value = false
  }
}

async function handleDelete(userId: string) {
  Modal.confirm({
    title: '確定要刪除此帳號？',
    onOk: async () => {
      try {
        await deleteUser(userId)
        message.success('刪除成功')
        await loadData()
      } catch {
        message.error('刪除失敗')
      }
    },
  })
}

async function handleRoleChange(user: UserItem, roleId: string) {
  savingUserId.value = user.id
  try {
    await assignUserRoles(user.id, [roleId])
    message.success(`已更新 ${user.username} 的角色`)
    await loadData()
  } catch {
    message.error('角色更新失敗')
  } finally {
    savingUserId.value = null
  }
}

onMounted(loadData)
</script>

<template>
  <div class="page-stack">
    <div class="page-header">
      <h2 class="page-title">帳號管理</h2>
      <a-button type="primary" @click="openCreateModal">新增帳號</a-button>
    </div>

    <a-table :data-source="users" :loading="loading" row-key="id" size="middle" :scroll="{ x: 1040 }">
      <a-table-column title="帳號" data-index="username" key="username" :width="160" />
      <a-table-column title="Email" data-index="email" key="email" :width="260" />
      <a-table-column title="員工編號" data-index="employeeId" key="employeeId" :width="130" />
      <a-table-column title="狀態" key="isActive" :width="100">
        <template #default="{ record }">
          <a-tag :color="record.isActive ? 'green' : 'red'">
            {{ record.isActive ? '啟用' : '停用' }}
          </a-tag>
        </template>
      </a-table-column>
      <a-table-column title="角色權限" key="roles" :width="220">
        <template #default="{ record }">
          <a-select :value="roleIdOf(record)" :options="roleOptions" :loading="savingUserId === record.id"
            style="width: 180px" @change="handleRoleChange(record, $event)" />
          <div class="role-note">
            目前：{{ roleNameOf(record) }}
          </div>
        </template>
      </a-table-column>
      <a-table-column title="操作" key="actions" :width="220">
        <template #default="{ record }">
          <a-space>
            <a-button type="link" @click="openEditModal(record)">編輯</a-button>
            <a-button type="link" @click="openResetPasswordModal(record)">重設密碼</a-button>
            <a-button type="link" danger @click="handleDelete(record.id)">刪除</a-button>
          </a-space>
        </template>
      </a-table-column>
    </a-table>

    <a-modal :open="createModalVisible" title="新增帳號" ok-text="建立" cancel-text="取消" :confirm-loading="submittingCreate"
      @ok="handleCreateUser" @update:open="createModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item label="帳號">
          <a-input :value="createForm.username" @update:value="createForm.username = $event" />
        </a-form-item>
        <a-form-item label="Email">
          <a-input :value="createForm.email" @update:value="createForm.email = $event" />
        </a-form-item>
        <a-form-item label="員工編號">
          <a-input :value="createForm.employeeId" @update:value="createForm.employeeId = $event" />
        </a-form-item>
        <a-form-item label="密碼">
          <a-input-password :value="createForm.password" @update:value="createForm.password = $event" />
        </a-form-item>
        <a-form-item label="角色">
          <a-select :value="createForm.roleId" :options="roleOptions" @update:value="createForm.roleId = $event" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal :open="editModalVisible" title="編輯帳號" ok-text="儲存" cancel-text="取消" :confirm-loading="submittingEdit"
      @ok="handleUpdateUser" @update:open="editModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item label="帳號">
          <a-input :value="editingUser?.username" disabled />
        </a-form-item>
        <a-form-item label="員工編號">
          <a-input :value="editingUser?.employeeId" disabled />
        </a-form-item>
        <a-form-item label="Email">
          <a-input :value="editForm.email" @update:value="editForm.email = $event" />
        </a-form-item>
        <a-form-item label="帳號狀態">
          <a-select :value="editForm.isActive" @update:value="editForm.isActive = $event">
            <a-select-option :value="true">啟用</a-select-option>
            <a-select-option :value="false">停用</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="角色">
          <a-select :value="editForm.roleId" :options="roleOptions" @update:value="editForm.roleId = $event" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal :open="resetPasswordModalVisible" title="重設密碼" ok-text="更新密碼" cancel-text="取消"
      :confirm-loading="submittingResetPassword" @ok="handleResetPassword"
      @update:open="resetPasswordModalVisible = $event">
      <a-form layout="vertical">
        <a-form-item label="目標帳號">
          <a-input :value="passwordTargetUser?.username" disabled />
        </a-form-item>
        <a-form-item label="新密碼">
          <a-input-password :value="resetPasswordForm.newPassword"
            @update:value="resetPasswordForm.newPassword = $event" />
        </a-form-item>
        <a-form-item label="確認新密碼">
          <a-input-password :value="resetPasswordForm.confirmPassword"
            @update:value="resetPasswordForm.confirmPassword = $event" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<style scoped>
:deep(.ant-table-thead > tr > th),
:deep(.ant-table-tbody > tr > td) {
  white-space: nowrap;
}

.role-note {
  font-size: 12px;
  color: #64748b;
  margin-top: 4px;
}
</style>
