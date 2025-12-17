import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig(({ mode }) => ({
    // Deployed under IIS virtual directory `/mp_ui`, so build assets must be rooted there.
    // Only the dev server should use `/` as base.
    base: mode === 'development' ? '/' : '/mp_ui/',
    server: {
        port: 5174,
        strictPort: true // optional: fail if port is taken
    },
    plugins: [vue()],
}));
