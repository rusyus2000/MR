
import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from '../views/Dashboard.vue';
import DomainItems from '../views/DomainItems.vue';

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
        props: true, // Pass route params as props to the component
    },
    {
        path: '/items/:id',
        name: 'ItemDetails',
        component: () => import('../views/ItemDetails.vue'),
        props: true
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;


