import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7098',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  test: {
    environment: 'jsdom',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
    },
  },
})
