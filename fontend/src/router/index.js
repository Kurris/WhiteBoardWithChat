import Vue from 'vue'
import Router from 'vue-router'
Vue.use(Router)

const Chat = () => import('../view/Chat.vue')
const login = () => import('../view/login.vue')
const roomInfo = () => import('../view/RoomInfo.vue')

const routes = [
	{
		path: '/',
		component: login,
	},
	{
		path: '/login',
		component: login,
		meta: {
			title: '用户登录',
		},
	},
	{
		path: '/whiteboard',
		component: Chat,
		meta: {
			title: '白板',
		},
	},
	{
		path: '/roomInfo',
		component: roomInfo,
		meta: {
			title: '房间信息',
		},
	},
]

const router = new Router({
	routes,
	mode: 'history',
})

// 路由守卫
router.beforeEach((to, from, next) => {
	if (to.matched.length > 0) document.title = to.matched[to.matched.length - 1].meta.title
	next()
})

export default router
