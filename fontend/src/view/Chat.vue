<template>
    <div class="contentWrap">
        <el-row class="h100">
            <el-col :xs="24" :sm="14" class="height_hb">
                <el-card class="inner draw h100" @mousemove="beginPath($event)">
                    <div slot="header" class="clearfix canvasHeader">
                        <span class="title">画板 房间号:{{ $route.query.roomName }} 登录用户:{{ realName }} 当前画图:{{
                currentPrint
              }}</span>
                    </div>
                    <div :class="{ canvasBox: true, disabled: !permission }" ref="canvasBox">
                        <div class="canvasWrap">
                            <canvas id="canvas" :class="isEraser ? 'eraser' : ''" :width="canvasWidth + 'px'" :height="canvasHeight + 'px'" @mousedown="canvasDown($event)" @mouseup="canvasUp($event)" @mousemove="canvasMove($event)" @touchstart="canvasDown($event)" @touchend="canvasUp($event)" @touchmove="canvasMove($event)">
                            </canvas>
                        </div>
                        <div id="control">
                            <!--画笔颜色-->
                            <!-- <div id="canvas-color">
                <ul>
                  <li
                    v-for="(item, index) in colors"
                    :class="{ active: config.lineColor === item }"
                    :style="{ background: item }"
                    @click="setColor(item)"
                    :key="'color-' + index"
                  ></li>
                </ul>
              </div> -->
                            <!--画笔-->
                            <div id="canvas-brush">
                                <span v-for="(pen, index) in brushs" :class="[pen.className, { active: config.lineWidth === pen.lineWidth }]" @click="setBrush(pen.lineWidth)" :key="'pen-' + index"></span>
                            </div>
                            <!--操作-->
                            <div id="canvas-control">
                                <span v-for="(control, index) in controls" :title="control.title" :class="control.className" @click="controlAction(control.action)" :key="'control-' + index"></span>
                                <!-- 橡皮 -->
                                <span class="fa fa-eraser" :class="isEraser ? 'active' : ''" @click="eraser"></span>
                                <span class="fa fa-image" @click="setBg"></span>
                                <input ref="filElem" type="file" class="upload-file" style="display:none" @change="getFile" />
                            </div>
                        </div>
                    </div>
                </el-card>
            </el-col>
            <el-col :xs="24" :sm="10" class="height_lt">
                <el-card class="chatCard h100">
                    <div slot="header" class="clearfix chatHeader">
                        <span class="title">聊天区</span>
                        <template v-if="owner">
                            <el-button @click="drawer = true" class="yhBtn">在线用户</el-button>
                        </template>
                    </div>
                    <div class="chatBody" id="chatBody">
                        <div class="chatObj" v-for="(item, i) in chatArr" :key="i">
                            <div class="bubble me" v-if="item.type == 'me'">
                                <div class="xq_c">
                                    <div class="rt_c">
                                        <span class="name">{{ item.userName }}</span>
                                        <span class="time">{{ item.currentTime }}</span>
                                    </div>
                                    <div class="rb_c">
                                        <div v-html="item.content"></div>
                                    </div>
                                </div>
                                <div class="h_c">
                                    <el-avatar :size="45" style="margin-top: 5px;">
                                        <img src="https://cube.elemecdn.com/e/fd/0fc7d20532fdaf769a25683617711png.png" />
                                    </el-avatar>
                                </div>
                            </div>
                            <div class="bubble your" v-else>
                                <div class="h_c">
                                    <el-avatar :size="45" style="margin-top: 5px;">
                                        <img src="https://cube.elemecdn.com/e/fd/0fc7d20532fdaf769a25683617711png.png" />
                                    </el-avatar>
                                </div>
                                <div class="xq_c">
                                    <div class="rt_c">
                                        <span class="name">{{ item.userName }}</span>
                                        <span class="time">{{ item.currentTime }}</span>
                                    </div>
                                    <div class="rb_c">
                                        <div v-html="item.content"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="chatBottom">
                        <el-button id="sendBtn" @click="sendMsg">发送</el-button>
                        <div id="editor">
                            <div ref="editor" class="text"></div>
                        </div>
                    </div>
                </el-card>
            </el-col>
        </el-row>

        <el-drawer title="在线用户" :visible.sync="drawer" direction="rtl" :size="drawerSize">
            <div class="zxyhBox">
                <template v-for="(item, index) in users">
                    <div :key="index" class="yhOBj">
                        <el-tag size="medium" :type="$route.query.userName == item.userName ? '' : 'success'">{{
              item.userName
            }}</el-tag>
                        <el-button :type="isActive.findIndex((v) => v == index) >= 0 ? 'success' : ''" v-show="$route.query.userName != item.userName" @click="setPermission(item.id, $route.query.roomName, index)">
                            <span v-show="isActive.findIndex((v) => v == index) < 0">画图权限</span>
                            <span v-show="isActive.findIndex((v) => v == index) >= 0">取消画图权限</span>
                        </el-button>
                    </div>
                </template>
            </div>
        </el-drawer>
    </div>
</template>

<script>
import E from "wangeditor";
import Vue from "vue";
import request from '../api/index'

export default {
    name: "chat",
    data() {
        return {
            realName: '',
            isActive: [],
            owner: false,
            users: [],
            drawer: false,
            currentPrint: "",
            permission: true,
            canvasWidth: 0,
            canvasHeight: 0,
            editorContent: "",
            editor: null,
            chatArr: [],
            drawerSize: "35%",
            colors: ["#fef4ac", "#0018ba", "#ffc200", "#f32f15", "#cccccc", "#5ab639"],
            brushs: [
                {
                    className: "small fa fa-paint-brush",
                    lineWidth: 1,
                },
                {
                    className: "middle fa fa-paint-brush",
                    lineWidth: 6,
                },
                {
                    className: "big fa fa-paint-brush",
                    lineWidth: 12,
                },
            ],
            context: {},
            imgUrl: [],
            canvasMoveUse: false,
            // 存储当前表面状态数组-上一步
            preDrawAry: [],
            // 存储当前表面状态数组-下一步
            nextDrawAry: [],
            // 中间数组
            middleAry: [],
            //在使用橡皮
            isEraser: false,
            // 配置参数
            config: {
                lineWidth: 1,
                shadowBlur: 2,
            },
        };
    },
    methods: {
        //设置背景
        setBg() {
            this.$refs.filElem.dispatchEvent(new MouseEvent("click"));
        },
        getFile() {
            var that = this;
            const inputFile = this.$refs.filElem.files[0];
            if (inputFile) {
                if (inputFile.type !== "image/jpeg" && inputFile.type !== "image/png" && inputFile.type !== "image/gif") {
                    alert("不是有效的图片文件！");
                    return;
                }
                var reader = new FileReader();
                reader.readAsDataURL(inputFile);
                reader.onload = function (e) {
                    that.canvasBase64(this.result);
                };
            } else {
                return;
            }
        },
        //橡皮
        eraser() {
            this.isEraser = !this.isEraser;
            if (this.isEraser) {
                this.config.lineColor = "#fff";
            } else {
                this.config.lineColor = "#000";
            }
        },
        sendMsg() {
            //需要清空内容
            let nobq = this.editorContent;
            //nobq = nobq.replace(/<[^>]+>/g, "");
            nobq = true;
            if (this.editorContent != "" && nobq) {
                Vue.$signalR.invoke("OnChatBoard", this.editorContent);
                this.editor.txt.html("");
                var div = document.getElementById("chatBody");
                setTimeout(function () {
                    div.scrollTop = div.scrollHeight;
                }, 10);
            } else {
                this.$message.error("发送内容不能为空！");
            }
        },
        setPermission(id, room, index) {
            var isActive = this.isActive;
            if (isActive.length == 0) {
                isActive.push(index);
            } else {
                var aIndex = isActive.findIndex((v) => v == index);
                if (aIndex >= 0) {
                    isActive.splice(aIndex, 1);
                } else {
                    isActive.push(index);
                }
            }
            Vue.$signalR.invoke("SetPermission", Number.parseInt(Vue.userId), this.$route.roomCode);
            this.drawer = !this.drawer;
        },
        createEditor() {
            var that = this;
            this.editor = new E(this.$refs.editor);
            this.editor.config.onchange = (html) => {
                this.editorContent = html;
            };
            this.editor.config.height = 150;
            this.editor.config.showFullScreen = false;
            this.editor.config.placeholder = "开始聊天";
            this.editor.config.menus = [
                // 菜单配置
                "bold", // 粗体
                "emoticon", // 表情
                "foreColor", // 文字颜色
                "image",
            ];
            this.editor.config.uploadImgServer = "/upload-img";
            this.editor.config.showLinkImg = false;
            this.editor.config.uploadImgMaxLength = 1; // 一次最多上传 1 个图片
            this.editor.config.customUploadImg = function (resultFiles, insertImgFn) {
                var file = resultFiles[0];
                var reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = function (e) {
                    insertImgFn(this.result);
                };
                // resultFiles 是 input 中选中的文件列表
                // insertImgFn 是获取图片 url 后，插入到编辑器的方法
                // 上传图片，返回结果，将图片插入到编辑器中
                //insertImgFn(imgUrl);
            };
            this.editor.config.uploadImgShowBase64 = true;
            this.editor.create();
        },
        initCanvas() {
            const canvas = document.querySelector("#canvas");
            this.context = canvas.getContext("2d");
            this.initDraw();
            this.setCanvasStyle();
        },
        //图片加载到画布
        canvasBase64(base64) {
            var that = this;
            var image = new Image();
            image.src = base64;
            image.onload = function () {
                that.context.drawImage(image, 0, 0);
            };
        },
        isPc() {
            const userAgentInfo = navigator.userAgent;
            const Agents = ["Android", "iPhone", "SymbianOS", "Windows Phone", "iPad", "iPod"];
            let flag = true;
            for (let v = 0; v < Agents.length; v++) {
                if (userAgentInfo.indexOf(Agents[v]) > 0) {
                    flag = false;
                    break;
                }
            }
            return flag;
        },
        removeImg(src) {
            this.imgUrl = this.imgUrl.filter((item) => item !== src);
        },
        initDraw() {
            const preData = this.context.getImageData(0, 0, this.canvasWidth, this.canvasHeight);
            // 空绘图表面进栈
            this.middleAry.push(preData);
        },
        canvasMove(e) {
            if (this.canvasMoveUse) {
                const t = e.target;
                let canvasX;
                let canvasY;
                if (this.isPc()) {
                    canvasX = e.clientX - t.parentNode.offsetLeft;
                    canvasY = e.clientY - t.parentNode.offsetTop;
                } else {
                    canvasX = e.changedTouches[0].clientX - t.parentNode.offsetLeft;
                    canvasY = e.changedTouches[0].clientY - t.parentNode.offsetTop;
                }
                if (this.isEraser) {
                    //橡皮状态
                    this.context.arc(canvasX, canvasY, 30, 0, 5 * Math.PI);
                    this.context.closePath();
                    this.context.fillStyle = "white";
                    this.context.fill();
                } else {
                    this.context.lineTo(canvasX, canvasY);
                    this.context.stroke();
                }

                this.SyncImage();
            }
        },
        SyncImage(clear) {
            if (clear) {
                this.context.clearRect(0, 0, this.context.canvas.width, this.context.canvas.height);
            }
            //画画立刻生成图像
            let canvas = document.querySelector("#canvas");
            let canvasImg = canvas.toDataURL("image/png");
            if (canvasImg && canvasImg != "") {
                Vue.$signalR.invoke("OnPrintBoard", canvasImg);
            }
        },
        beginPath(e) {
            const canvas = document.querySelector("#canvas");
            if (e.target !== canvas) {
                this.context.beginPath();
            }
        },
        // mouseup
        canvasUp(e) {
            const preData = this.context.getImageData(0, 0, 600, 400);
            if (!this.nextDrawAry.length) {
                // 当前绘图表面进栈
                this.middleAry.push(preData);
            } else {
                this.middleAry = [];
                this.middleAry = this.middleAry.concat(this.preDrawAry);
                this.middleAry.push(preData);
                this.nextDrawAry = [];
            }
            this.canvasMoveUse = false;
        },
        // mousedown
        canvasDown(e) {
            this.canvasMoveUse = true;
            // client是基于整个页面的坐标
            // offset是cavas距离顶部以及左边的距离
            const canvasX = e.clientX - e.target.parentNode.offsetLeft;
            const canvasY = e.clientY - e.target.parentNode.offsetTop;
            this.setCanvasStyle();
            // 清除子路径
            this.context.beginPath();
            this.context.moveTo(canvasX, canvasY);
            // 当前绘图表面状态
            const preData = this.context.getImageData(0, 0, 600, 400);
            // 当前绘图表面进栈
            this.preDrawAry.push(preData);
        },
        // 设置颜色
        setColor(color) {
            this.isEraser = false;
            this.config.lineColor = color;
        },
        // 设置笔刷大小
        setBrush(type) {
            if (this.isEraser) {
                this.isEraser = false;
                this.config.lineColor = "#000";
            }
            this.config.lineWidth = type;
        },
        controlAction(action) {
            Vue.$signalR.invoke("OnAction", action);
        },
        // 设置绘画配置
        setCanvasStyle() {
            this.context.lineWidth = this.config.lineWidth;
            this.context.shadowBlur = this.config.shadowBlur;
            this.context.shadowColor = this.config.lineColor;
            this.context.strokeStyle = this.config.lineColor;
        },
        getUser() {
            request({
                method: 'get',
                url: 'User/GetUser',
                params: {
                    connId: Vue.$signalR.connectionId
                }
            }).then(res => {
                if (res && res.data != null) {
                    this.realName = res.data.realName
                }
            })
        },
    },
    mounted() {
        this.canvasWidth = this.$refs.canvasBox.offsetWidth;
        this.canvasHeight = this.$refs.canvasBox.offsetHeight;
        this.initCanvas();
        this.createEditor();
        this.getUser();

        var _this = this;
        document.getElementById("editor").onkeydown = function (e) {
            let key = window.event.keyCode;
            if (key == 13) {
                _this.sendMsg();
            }
        };

        window.onresize = () => {
            if (window.innerWidth <= 768) {
                this.drawerSize = "80%";
            } else {
                this.drawerSize = "35%";
            }
        };
        //同步聊天室内容
        Vue.$signalR.on("onChatBoard", (res) => {
            if (res.status) {
                this.chatArr.push({
                    type: Vue.$signalR.connectionId == res.connectionId ? "me" : "your",
                    content: res.content,
                    userName: res.userName,
                    currentTime: res.currentTime,
                });
            }
        });

        //同步canvas数据
        Vue.$signalR.on("onPrintBoard", (res) => {
            if (res.status) {
                this.currentPrint = res.userName;
                this.canvasBase64(res.content);
            }
        });

        Vue.$signalR.on("onAction", (res) => {
            switch (res.action) {
                case "prev":
                    if (this.preDrawAry.length) {
                        const popData = this.preDrawAry.pop();
                        const midData = this.middleAry[this.preDrawAry.length + 1];
                        this.nextDrawAry.push(midData);
                        this.context.putImageData(popData, 0, 0);
                    }
                    break;
                case "next":
                    if (this.nextDrawAry.length) {
                        const popData = this.nextDrawAry.pop();
                        const midData = this.middleAry[this.middleAry.length - this.nextDrawAry.length - 2];
                        this.preDrawAry.push(midData);
                        this.context.putImageData(popData, 0, 0);
                    }
                    break;
                case "clear":
                    this.context.clearRect(0, 0, this.context.canvas.width, this.context.canvas.height);
                    this.preDrawAry = [];
                    this.nextDrawAry = [];
                    this.middleAry = [this.middleAry[0]];

                    break;
            }

            this.SyncImage(Vue.$signalR.connectionId != res.id);
        });

        Vue.$signalR.on("setPermission", (res) => {
            this.permission = res;
        });
        Vue.$signalR.invoke("GetPermission").then((per) => {
            this.permission = per;
        });

        Vue.$signalR.invoke("GetOwner").then((owner) => {
            this.owner = owner;
        });



        setInterval(() => {
            if (this.owner) {
                Vue.$signalR.invoke("GetUsers").then((users) => {
                    this.users = users;
                });
            }
        }, 2000);
    },
    created() { },
    beforeCreate() {
        // 浏览器刷新之后回到登录页面
        if (!Vue.isLogin && this.$router.path !== "/") {
            this.$router.replace("/");
        }
    },
    computed: {
        controls() {
            return [
                {
                    title: "上一步",
                    action: "prev",
                    className: this.preDrawAry.length ? "active fa fa-reply" : "fa fa-reply",
                },
                {
                    title: "下一步",
                    action: "next",
                    className: this.nextDrawAry.length ? "active fa fa-share" : "fa fa-share",
                },
                {
                    title: "清除",
                    action: "clear",
                    className: this.preDrawAry.length || this.nextDrawAry.length ? "active fa fa-trash" : "fa fa-trash",
                },
            ];
        },
    },
};
</script>

<style lang="scss" scoped>
.disabled {
    pointer-events: none;

    cursor: default;
}
.eraser {
    cursor: url("../assets/images/eraser.png"), default !important;
}
.zxyhBox {
    padding: 5px 5%;
    & .yhOBj {
        margin-bottom: 10px;
        & button {
            margin-left: 8px;
        }
    }
}
.chatCard {
    position: relative;
    & ::v-deep.el-card__header {
        height: 45px;
        padding: 13px 20px;
    }
    ::v-deep.el-card__body {
        padding: 0;
        height: 100%;
    }
    & .chatHeader {
        position: relative;
        & .yhBtn {
            position: absolute;
            right: -5px;
            top: -7px;
        }
        & .title {
            font-weight: 600;
            font-size: 14px;
        }
    }
    & .chatBody {
        height: calc(100% - 245px);
        overflow: auto;
        padding: 8px 20px;
        & .chatObj {
            position: relative;
            width: 100%;
            min-height: 50px;
            display: inline-block;
            margin: 20px 0;
            & .bubble {
                width: 320px;
                display: -webkit-inline-box;
                & .xq_c {
                    width: 255px;
                    margin-top: 10px;
                    margin-left: 10px;
                    margin-right: 10px;
                    & .rt_c {
                        margin-bottom: 5px;
                        font-size: 12px;
                        color: #92948f;
                        & .name {
                            margin-right: 5px;
                        }
                    }
                }
                & .rb_c {
                    text-align: justify;
                    text-justify: newspaper;
                    word-break: break-all;
                    max-width: 100%;
                    padding: 8px 20px;
                    display: inline-block;
                    border-radius: 4px;
                    margin: 0 0 0 5px;
                    position: relative;
                    &:before {
                        content: "";
                        position: absolute;
                        border: 6px solid #ffffff00;
                        top: 1px;
                    }
                }
            }
            & .h_c {
                & img {
                    border: 1px solid #d4cdcd;
                    border-radius: 50px;
                    width: 35px;
                    height: 35px;
                    margin-top: 16px;
                }
            }
            & .me {
                float: right;
                & .xq_c {
                    text-align: right;
                }
                & .rb_c {
                    float: right;
                    background-color: #007ddc;
                    color: #fff;
                    &:before {
                        border-left: 6px solid #007ddc;
                        right: -11px;
                    }
                }
            }
            & .your {
                float: left;
                & .xq_c {
                    text-align: left;
                }
                & .rb_c {
                    float: left;
                    background-color: #67c23a;
                    color: #fff;
                    &:before {
                        border-right: 6px solid#67c23a;
                        left: -11px;
                    }
                }
            }
        }
    }
    & .chatBottom {
        position: absolute;
        width: 100%;
        bottom: 0;
        z-index: 0;
        & #sendBtn {
            position: absolute;
            z-index: 99999;
            bottom: 5px;
            right: 8px;
        }
    }
}

.zxkh_w {
    width: 40%;
}
.height_hb {
    height: 100%;
}
.height_lt {
    height: 100%;
}
@media screen and (max-width: 768px) {
    .zxkh_w {
        width: 80%;
    }
    .height_hb {
        height: 230px;
    }
    .height_lt {
        height: calc(100% - 230px);
    }
}

.inner.draw {
    ::v-deep.el-card__body {
        padding: 0;
        height: 100%;
    }
}
.draw {
    & ::v-deep.el-card__header {
        height: 45px;
        padding: 13px 20px;
    }
    & .canvasHeader {
        & .title {
            font-weight: 600;
            font-size: 14px;
        }
    }
    h5 {
        margin-bottom: 10px;
    }
}
.canvasBox {
    width: 100%;
    height: calc(100% - 80px);

    & .canvasWrap {
        height: 100%;
        & #canvas {
            cursor: crosshair;
        }
    }

    #control {
        border-top: 1px solid #e2e0e0;
        display: flex;
        height: 40px;
        & div {
            padding: 5px;
        }
        #canvas-color {
            & ul {
                overflow: hidden;
                margin: 0;
                padding: 0;
            }
            & ul li {
                float: left;
                display: inherit;
                width: 13px;
                height: 13px;
                border: 3px #fff solid;
                margin: 8px;
                cursor: pointer;
            }
            & .active {
                border: 1px solid #f2849e;
            }
        }

        #canvas-brush {
            padding-top: 8px !important;
            & span {
                display: inline-block;
                width: 20px;
                height: 15px;
                margin-left: 10px;
                cursor: pointer;
            }
            & .small {
                font-size: 14px;
            }
            & .middle {
                font-size: 16px;
            }
            & .big {
                font-size: 18px;
            }
        }
        #canvas-control {
            padding-top: 10px !important;
            & span {
                display: inline-block;
                font-size: 16px;
                width: 20px;
                height: 15px;
                margin-left: 10px;
                cursor: pointer;
            }
        }

        #canvas-control .active,
        #canvas-brush .active {
            color: #f2849e;
        }
    }
}
</style>
