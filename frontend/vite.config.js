import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/Article': {
        target: 'http://localhost:5149',
        changeOrigin: true
      }
    }
  }
})
