import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from '../views/Dashboard.vue';
import DomainItems from '../views/DomainItems.vue';
import AddAsset from '../views/AddAsset.vue'; // Add the new admin page

const routes = [
    {
        path: '/',
        name: 'Dashboard',
        component: Dashboard,
    },
    {
        path: '/domain/:domainName',
        name: 'DomainItems',
        component: DomainItems,
        props: true,
    },
    {
        path: '/items/:id',
        name: 'ItemDetails',
        component: () => import('../views/ItemDetails.vue'),
        props: true
    },
    {
        path: '/admin/add-asset',
        name: 'AddAsset',
        component: AddAsset
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;