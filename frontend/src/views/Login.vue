<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import { message } from 'ant-design-vue'

const router = useRouter()
const { login, isLoading, error } = useAuth()

const formState = ref({
  username: '',
  password: '',
})

const handleLogin = async () => {
  if (!formState.value.username || !formState.value.password) {
    message.error('請輸入用戶名和密碼')
    return
  }

  try {
    await login({
      username: formState.value.username,
      password: formState.value.password,
    })

    message.success('登錄成功')
    router.push('/')
  } catch (err) {
    message.error(error.value || '登錄失敗，請檢查用戶名和密碼')
  }
}

// Demo users info
const demoUsers = [
  { username: 'employee1', role: '員工', password: 'Password123!' },
  { username: 'manager1', role: '主管', password: 'Password123!' },
  { username: 'admin', role: '管理員', password: 'Password123!' },
]

const fillDemoUser = (username: string, password: string) => {
  formState.value.username = username
  formState.value.password = password
}
</script>

<template>
  <div class="login-container">
    <a-card class="login-card" title="LifeSwap 登錄">
      <a-form :model="formState" layout="vertical" @submit.prevent="handleLogin">
        <a-form-item label="用戶名" required>
          <a-input :value="formState.username" placeholder="請輸入用戶名" size="large" :disabled="isLoading"
            @update:value="formState.username = $event" />
        </a-form-item>

        <a-form-item label="密碼" required>
          <a-input-password :value="formState.password" placeholder="請輸入密碼" size="large" :disabled="isLoading"
            @pressEnter="handleLogin" @update:value="formState.password = $event" />
        </a-form-item>

        <a-form-item>
          <a-button type="primary" html-type="submit" size="large" block :loading="isLoading">
            登錄
          </a-button>
        </a-form-item>
      </a-form>

      <a-divider>測試帳號</a-divider>

      <div class="demo-users">
        <a-space direction="vertical" style="width: 100%">
          <div v-for="user in demoUsers" :key="user.username" class="demo-user-item">
            <div class="demo-user-info">
              <strong>{{ user.role }}</strong>
              <span class="username">{{ user.username }}</span>
            </div>
            <a-button size="small" @click="fillDemoUser(user.username, user.password)">
              使用此帳號
            </a-button>
          </div>
        </a-space>
      </div>
    </a-card>
  </div>
</template>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background:
    radial-gradient(circle at 15% 20%, rgba(99, 102, 241, 0.45) 0%, rgba(99, 102, 241, 0) 35%),
    radial-gradient(circle at 85% 80%, rgba(59, 130, 246, 0.35) 0%, rgba(59, 130, 246, 0) 30%),
    linear-gradient(135deg, #0f172a 0%, #1e1b4b 50%, #312e81 100%);
  padding: 20px;
}

.login-card {
  width: 100%;
  max-width: 450px;
  border: 1px solid rgba(226, 232, 240, 0.75);
  border-radius: 18px;
  box-shadow: 0 30px 60px -34px rgba(15, 23, 42, 0.9);
  backdrop-filter: blur(2px);
}

.demo-users {
  margin-top: 16px;
}

.demo-user-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  margin-bottom: 8px;
}

.demo-user-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.username {
  font-size: 12px;
  color: #64748b;
  font-family: monospace;
}

:deep(.ant-card-head-title) {
  text-align: center;
  font-size: 20px;
}
</style>
