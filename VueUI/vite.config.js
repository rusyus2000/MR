import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig(({ mode }) => ({
    base: mode === 'production' ? '/mp_ui/' : '/',
    server: {
        port: 5174,
        strictPort: true // optional: fail if port is taken
    },
    plugins: [vue()],
}));