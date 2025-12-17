import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from '../views/Dashboard.vue';
import AddAsset from '../views/AddAsset.vue';

// Use the Vite base URL so routing works when deployed under `/mp_ui`.
// In dev server this will be `/`, in builds it will be `/mp_ui/`.
const base = import.meta.env.BASE_URL || '/';

const routes = [
    {
        path: '/',
        name: 'Dashboard',
        component: Dashboard,
    },
    {
        path: '/admin/add-asset',
        name: 'AddAsset',
        component: AddAsset,
    },
];

export default createRouter({
    history: createWebHistory(base),
    routes,
});
