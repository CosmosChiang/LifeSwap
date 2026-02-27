<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'

const router = useRouter()
const { login, isLoading, error } = useAuth()
const { t } = useI18n()

const formState = ref({
  username: '',
  password: '',
})

const handleLogin = async () => {
  if (!formState.value.username || !formState.value.password) {
    message.error(t('login.requiredCredentials'))
    return
  }

  try {
    await login({
      username: formState.value.username,
      password: formState.value.password,
    })

    message.success(t('login.loginSuccess'))
    router.push('/')
  } catch (err) {
    message.error(error.value || t('login.loginFailed'))
  }
}

// Demo users info
const demoUsers = [
  { username: 'employee1', roleKey: 'login.roleEmployee', password: 'Password123!' },
  { username: 'manager1', roleKey: 'login.roleManager', password: 'Password123!' },
  { username: 'admin', roleKey: 'login.roleAdmin', password: 'Password123!' },
]

const fillDemoUser = (username: string, password: string) => {
  formState.value.username = username
  formState.value.password = password
}
</script>

<template>
  <div class="login-container">
    <a-card class="login-card" :title="t('login.title')">
      <template #extra>
        <LocaleSwitcher />
      </template>
      <a-form :model="formState" layout="vertical" @submit.prevent="handleLogin">
        <a-form-item :label="t('login.username')" required>
          <a-input :value="formState.username" :placeholder="t('login.usernamePlaceholder')" size="large" :disabled="isLoading"
            @update:value="formState.username = $event" />
        </a-form-item>

        <a-form-item :label="t('login.password')" required>
          <a-input-password :value="formState.password" :placeholder="t('login.passwordPlaceholder')" size="large" :disabled="isLoading"
            @pressEnter="handleLogin" @update:value="formState.password = $event" />
        </a-form-item>

        <a-form-item>
          <a-button type="primary" html-type="submit" size="large" block :loading="isLoading">
            {{ t('login.submit') }}
          </a-button>
        </a-form-item>
      </a-form>

      <a-divider>{{ t('login.demoAccounts') }}</a-divider>

      <div class="demo-users">
        <a-space direction="vertical" style="width: 100%">
          <div v-for="user in demoUsers" :key="user.username" class="demo-user-item">
            <div class="demo-user-info">
              <strong>{{ t(user.roleKey) }}</strong>
              <span class="username">{{ user.username }}</span>
            </div>
            <a-button size="small" @click="fillDemoUser(user.username, user.password)">
              {{ t('login.useAccount') }}
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
