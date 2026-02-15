import { ref, computed } from 'vue'
import type { LoginRequest, UserInfo } from '../types'
import { login as apiLogin, logout as apiLogout } from '../api'

const userInfo = ref<UserInfo | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

// Check if user is logged in on app initialization
function initAuth() {
    const token = localStorage.getItem('auth_token')
    const savedUserInfo = localStorage.getItem('user_info')

    if (token && savedUserInfo) {
        try {
            userInfo.value = JSON.parse(savedUserInfo)
        } catch {
            // Invalid saved data, clear it
            localStorage.removeItem('auth_token')
            localStorage.removeItem('user_info')
        }
    }
}

export function useAuth() {
    const isAuthenticated = computed(() => userInfo.value !== null)
    const currentUser = computed(() => userInfo.value)
    const roles = computed(() => userInfo.value?.roles ?? [])

    const login = async (credentials: LoginRequest) => {
        isLoading.value = true
        error.value = null

        try {
            const response = await apiLogin(credentials)

            // Store user info (without token)
            const user: UserInfo = {
                username: response.username,
                employeeId: response.employeeId,
                email: response.email,
                departmentCode: response.departmentCode,
                roles: response.roles,
            }

            userInfo.value = user
            localStorage.setItem('user_info', JSON.stringify(user))

            return response
        } catch (err) {
            error.value = err instanceof Error ? err.message : 'Login failed'
            throw err
        } finally {
            isLoading.value = false
        }
    }

    const logout = () => {
        apiLogout()
        userInfo.value = null
        localStorage.removeItem('user_info')
    }

    const hasRole = (role: string) => {
        return roles.value.includes(role)
    }

    const hasAnyRole = (...requiredRoles: string[]) => {
        return requiredRoles.some(role => roles.value.includes(role))
    }

    return {
        // State
        isAuthenticated,
        currentUser,
        roles,
        isLoading,
        error,

        // Methods
        login,
        logout,
        hasRole,
        hasAnyRole,
    }
}

// Initialize auth state on app startup
initAuth()
