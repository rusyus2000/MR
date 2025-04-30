import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from '../views/Dashboard.vue';
import AddAsset from '../views/AddAsset.vue';
import ItemDetails from '../views/ItemDetails.vue';

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
    // catch-all could go here if desired
];

export default createRouter({
    history: createWebHistory(),
    routes,
});
