import { Loading } from 'element-ui'

const loading = {
	requestCount: 0,
	instance: null,
	requestDone: false,
	target: 'body',
	needLoading: true,
	open() {
		this.requestCount++
		let that = this
		setTimeout(function() {
			if (that.instance === null && that.requestCount == 1) {
				that.instance = Loading.service({
					lock: true,
					text: '数据加载中...',
					background: '#fff',
					target: that.target,
				})
			}
		}, 200)
	},
	close() {
		this.requestCount--
		if (this.requestCount == 0) {
			if (this.instance != null && this.requestCount == 0) {
				this.instance.close()
				this.instance = null
			}
		}
	},
}

export default loading
