import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from '../views/Dashboard.vue';
import AddAsset from '../views/AddAsset.vue';
import ItemDetails from '../views/ItemDetails.vue';

// This uses Vite's environment variables
const base =
    import.meta.env.MODE === 'production'
        ? '/mp_ui/'
        : '/';

const routes = [
    {
        path: '/',
        name: 'Dashboard',
        component: Dashboard,
    },
    {
        path: '/items/:id',
        name: 'ItemDetails',
        component: ItemDetails,
        props: true,
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
