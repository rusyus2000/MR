import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
    server: {
        port: 5174,
        strictPort: true // optional: fail if port is taken
    },
  plugins: [vue()],
});