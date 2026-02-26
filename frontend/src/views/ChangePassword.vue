<script setup lang="ts">
import { reactive, ref } from 'vue'
import { message } from 'ant-design-vue'
import { changeMyPassword } from '../api'

const submitting = ref(false)
const form = reactive({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
})

function resetForm() {
    form.currentPassword = ''
    form.newPassword = ''
    form.confirmPassword = ''
}

async function handleSubmit() {
    if (!form.currentPassword || !form.newPassword || !form.confirmPassword) {
        message.error('請完整輸入密碼欄位')
        return
    }

    if (form.newPassword.length < 8) {
        message.error('新密碼至少 8 碼')
        return
    }

    if (form.newPassword !== form.confirmPassword) {
        message.error('新密碼與確認密碼不一致')
        return
    }

    submitting.value = true
    try {
        await changeMyPassword(form.currentPassword, form.newPassword)
        message.success('密碼已更新，請用新密碼重新登入')
        resetForm()
    } catch (error) {
        message.error((error as Error).message || '密碼修改失敗')
    } finally {
        submitting.value = false
    }
}
</script>

<template>
    <a-card title="修改密碼" style="max-width: 520px">
        <a-form layout="vertical" @submit.prevent="handleSubmit">
            <a-form-item label="目前密碼">
                <a-input-password :value="form.currentPassword" @update:value="form.currentPassword = $event" />
            </a-form-item>

            <a-form-item label="新密碼">
                <a-input-password :value="form.newPassword" @update:value="form.newPassword = $event" />
            </a-form-item>

            <a-form-item label="確認新密碼">
                <a-input-password :value="form.confirmPassword" @update:value="form.confirmPassword = $event" />
            </a-form-item>

            <a-space>
                <a-button type="primary" :loading="submitting" @click="handleSubmit">儲存密碼</a-button>
                <a-button @click="resetForm">清除</a-button>
            </a-space>
        </a-form>
    </a-card>
</template>
