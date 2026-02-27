<script setup lang="ts">
import { reactive, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useI18n } from 'vue-i18n'
import { changeMyPassword } from '../api'

const submitting = ref(false)
const { t } = useI18n()
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
        message.error(t('changePassword.validation.required'))
        return
    }

    if (form.newPassword.length < 8) {
        message.error(t('changePassword.validation.minLength'))
        return
    }

    if (form.newPassword !== form.confirmPassword) {
        message.error(t('changePassword.validation.mismatch'))
        return
    }

    submitting.value = true
    try {
        await changeMyPassword(form.currentPassword, form.newPassword)
        message.success(t('changePassword.success'))
        resetForm()
    } catch (error) {
        message.error((error as Error).message || t('changePassword.error'))
    } finally {
        submitting.value = false
    }
}
</script>

<template>
    <div class="single-card-wrap">
        <a-card :title="t('changePassword.pageTitle')">
            <a-form layout="vertical" @submit.prevent="handleSubmit">
            <a-form-item :label="t('changePassword.currentPassword')">
                <a-input-password :value="form.currentPassword" @update:value="form.currentPassword = $event" />
            </a-form-item>

            <a-form-item :label="t('changePassword.newPassword')">
                <a-input-password :value="form.newPassword" @update:value="form.newPassword = $event" />
            </a-form-item>

            <a-form-item :label="t('changePassword.confirmPassword')">
                <a-input-password :value="form.confirmPassword" @update:value="form.confirmPassword = $event" />
            </a-form-item>

            <a-space>
                <a-button type="primary" :loading="submitting" @click="handleSubmit">{{ t('changePassword.save') }}</a-button>
                <a-button @click="resetForm">{{ t('common.clear') }}</a-button>
            </a-space>
            </a-form>
        </a-card>
    </div>
</template>
