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
  { username: 'hr_admin', role: 'HR', password: 'Password123!' },
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
      <a-form
        :model="formState"
        layout="vertical"
        @submit.prevent="handleLogin"
      >
        <a-form-item label="用戶名" required>
          <a-input
            v-model:value="formState.username"
            placeholder="請輸入用戶名"
            size="large"
            :disabled="isLoading"
          />
        </a-form-item>

        <a-form-item label="密碼" required>
          <a-input-password
            v-model:value="formState.password"
            placeholder="請輸入密碼"
            size="large"
            :disabled="isLoading"
            @pressEnter="handleLogin"
          />
        </a-form-item>

        <a-form-item>
          <a-button
            type="primary"
            html-type="submit"
            size="large"
            block
            :loading="isLoading"
          >
            登錄
          </a-button>
        </a-form-item>
      </a-form>

      <a-divider>測試帳號</a-divider>

      <div class="demo-users">
        <a-space direction="vertical" style="width: 100%">
          <div
            v-for="user in demoUsers"
            :key="user.username"
            class="demo-user-item"
          >
            <div class="demo-user-info">
              <strong>{{ user.role }}</strong>
              <span class="username">{{ user.username }}</span>
            </div>
            <a-button
              size="small"
              @click="fillDemoUser(user.username, user.password)"
            >
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
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.login-card {
  width: 100%;
  max-width: 450px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
}

.demo-users {
  margin-top: 16px;
}

.demo-user-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: #f5f5f5;
  border-radius: 6px;
  margin-bottom: 8px;
}

.demo-user-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.username {
  font-size: 12px;
  color: #666;
  font-family: monospace;
}
</style>
