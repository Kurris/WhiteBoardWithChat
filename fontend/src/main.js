import Vue from 'vue'
import App from './App.vue'
import router from './router/index'
//第三方组件导入
import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
import 'lib-flexible'
import 'font-awesome/css/font-awesome.min.css'
//自定义
import './assets/css/base.css'
import './assets/css/scss/common.min.css'

Vue.config.productionTip = false
Vue.use(ElementUI, { size: 'mini' })

new Vue({
	router,
	render: h => h(App),
}).$mount('#app')
