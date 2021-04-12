import axios from 'axios'
import loading from '../utils/loading.js'
import router from '../router/index.js'
import ElementUI from 'element-ui'

export default function request(config) {
	const instance = axios.create()

	config.baseURL = 'http://localhost:5000/api'

	instance.interceptors.request.use(config => {
		loading.target = typeof config.target == 'undefined' ? '#main-container' : config.target
		//loading.needLoading = typeof config.needLoading == 'undefined' ? true : config.needLoading
		loading.open()

		return config
	})

	instance.interceptors.response.use(
		result => {
			loading.close()

			//登录成功或者请求成功
			if (result.data.status <= 1001) return result.data

			//token验证失败
			if (result.data.status == 4000) {
				if (router.currentRoute.path != '/login') {
					router.replace('/login')
					return
				}
			}
			//实体验证失败
			else if (result.data.status == 4002) {
				ElementUI.Notification.error(result.data.data.map(x => x.message).join(','))
			}
			//其他错误
			else {
				ElementUI.Notification.error(result.data.message)
			}
		},
		error => {
			loading.close()
			//跳转到404
			if (error.message == 'Network Error') {
				ElementUI.Notification.error('请求已丢失')
			}
		}
	)

	return instance(config)
}
