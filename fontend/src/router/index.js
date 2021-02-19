import Vue from 'vue'
import Router from 'vue-router'
Vue.use(Router)

const Chat = () => import('../view/Chat.vue')
const test = () => import('../view/test.vue')
const login = () => import('../view/login.vue')

const routes = [
	{
		path: '/',
		component: login,
	},
	{
		path: '/login',
		component: login,
	},
	{
		path: '/whiteboard',
		component: Chat,
	},
	{
		path: '/test',
		component: test,
	},
]

const router = new Router({
	routes,
	mode: 'history',
})

export default router
