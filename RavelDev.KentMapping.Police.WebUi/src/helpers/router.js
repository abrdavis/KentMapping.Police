import { createRouter, createWebHistory } from 'vue-router';


import { HomeView } from '@/views';

export const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    linkActiveClass: 'active',
    routes: [
        { path: '/', component: HomeView }
    ]
});