import Vue from 'vue'
import VueRouter from 'vue-router'

Vue.use(VueRouter)

const routes = [
    {
        path: '/',
        name: 'home',
        meta: {requiresAuth: true},
        component: () => import('./views/Home')
    },
    {
        path: '/login',
        name: 'login',
        component: () => import('./views/Login')
    },
    {
        path: '*',
        component: () => import('./views/NotFound')
    },
]

const router = new VueRouter({
    base: process.env.BASE_URL,
    routes
})

router.beforeEach((to, from, next) => {
    if (to.matched.some(record => record.meta.requiresAuth)) {
        if (!Vue.cookie.get('BREEZETOKEN')) {
            router.push('/login')
        } else {
            next();
        }
    } else {
        next();
    }
})

export default router
