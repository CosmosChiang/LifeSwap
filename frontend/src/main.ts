import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import 'ant-design-vue/dist/reset.css'
import './style.css'
import App from './App.vue'

const app = createApp(App)

// Import routes dynamically
const routes = [
  {
    path: '/login',
    component: () => import('./views/Login.vue'),
    name: 'Login',
    meta: { requiresAuth: false },
  },
  {
    path: '/',
    component: () => import('./views/Home.vue'),
    name: 'Home',
    meta: { requiresAuth: true },
  },
  {
    path: '/requests',
    component: () => import('./views/MyRequests.vue'),
    name: 'MyRequests',
    meta: { requiresAuth: true },
  },
  {
    path: '/review',
    component: () => import('./views/ToReview.vue'),
    name: 'ToReview',
    meta: { requiresAuth: true, roles: ['Manager', 'HR', 'Administrator'] },
  },
  {
    path: '/reports',
    component: () => import('./views/Reports.vue'),
    name: 'Reports',
    meta: { requiresAuth: true, roles: ['HR', 'Administrator'] },
  },
  {
    path: '/notifications',
    component: () => import('./views/Notifications.vue'),
    name: 'Notifications',
    meta: { requiresAuth: true },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// Route guard for authentication and authorization
router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('auth_token')
  const userInfoStr = localStorage.getItem('user_info')
  const isAuthenticated = !!(token && userInfoStr)

  // Check if route requires authentication
  if (to.meta.requiresAuth && !isAuthenticated) {
    // Redirect to login if not authenticated
    next({ name: 'Login' })
    return
  }

  // If already authenticated and trying to access login, redirect to home
  if (to.name === 'Login' && isAuthenticated) {
    next({ name: 'Home' })
    return
  }

  // Check role-based access
  if (to.meta.roles && isAuthenticated) {
    try {
      const userInfo = JSON.parse(userInfoStr!)
      const userRoles = userInfo.roles || []
      const requiredRoles = to.meta.roles as string[]

      // Check if user has any of the required roles
      const hasRequiredRole = requiredRoles.some(role => userRoles.includes(role))

      if (!hasRequiredRole) {
        // User doesn't have required role, redirect to home
        next({ name: 'Home' })
        return
      }
    } catch {
      // Invalid user info, redirect to login
      localStorage.removeItem('auth_token')
      localStorage.removeItem('user_info')
      next({ name: 'Login' })
      return
    }
  }

  next()
})

app.use(router)
app.mount('#app')

