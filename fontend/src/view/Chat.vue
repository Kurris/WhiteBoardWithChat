<template>
  <div class="contentWrap">
    <el-row class="h100">
      <el-col :xs="24" :sm="14" class="height_hb">
        <el-card class="inner draw h100" @mousemove="beginPath($event)">
          <div slot="header" class="clearfix canvasHeader">
            <span class="title">画板</span>
          </div>
          <div class="canvasBox">
            <div class="canvasWrap" ref="canvasWrap">
              <canvas
                id="canvas"
                :width="canvasWidth + 'px'"
                :height="canvasHeight + 'px'"
                @mousedown="canvasDown($event)"
                @mouseup="canvasUp($event)"
                @mousemove="canvasMove($event)"
                @touchstart="canvasDown($event)"
                @touchend="canvasUp($event)"
                @touchmove="canvasMove($event)"
              >
              </canvas>
            </div>
            <div id="control">
              <!--画笔颜色-->
              <div id="canvas-color">
                <ul>
                  <li
                    v-for="(item, index) in colors"
                    :class="{ active: config.lineColor === item }"
                    :style="{ background: item }"
                    @click="setColor(item)"
                    :key="'color-' + index"
                  ></li>
                </ul>
              </div>
              <!--画笔-->
              <div id="canvas-brush">
                <span
                  v-for="(pen, index) in brushs"
                  :class="[pen.className, { active: config.lineWidth === pen.lineWidth }]"
                  @click="setBrush(pen.lineWidth)"
                  :key="'pen-' + index"
                ></span>
              </div>
              <!--操作-->
              <div id="canvas-control">
                <span
                  v-for="(control, index) in controls"
                  :title="control.title"
                  :class="control.className"
                  @click="controlCanvas(control.action)"
                  :key="'control-' + index"
                ></span>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="10" class="height_lt">
        <el-card class="chatCard h100">
          <div slot="header" class="clearfix chatHeader">
            <span class="title">聊天区</span>
          </div>
          <div class="chatBody">
            <div class="chatObj" v-for="(item, i) in chatArr" :key="i">
              <div class="bubble me" v-if="item.type == 'me'">
                <div class="xq_c">
                  <!-- <div class="rt_c">
                    <span class="name">user</span>
                    <span class="time">2020/7/22 17:39:28</span>
                  </div> -->
                  <div class="rb_c">
                    <div>{{ item.content }}</div>
                  </div>
                </div>
                <div class="h_c">
                  <img src="../assets/images/t1.jpg" />
                </div>
              </div>
              <div class="bubble your" v-else>
                <div class="h_c">
                  <img src="../assets/images/t2.jpg" />
                </div>
                <div class="xq_c">
                  <div class="rb_c">
                    <div>{{ item.content }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="chatBottom">
            <el-button id="sendBtn">发送</el-button>
            <div id="editor">
              <div ref="editor" class="text"></div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script>
import E from "wangeditor";
export default {
  name: "chat",
  data() {
    return {
      canvasWidth: 0,
      canvasHeight: 0,
      editorContent: "",
      editor: null,
      chatArr: [
        {
          type: "me",
          content:
            "1988年，习近平从福建省最发达的厦门调到当时全省经济最落后的宁德地区任地委书记。在宁德工作近两年时间里，习近平跑完了124个乡镇中的123个。4个不通公路的特困乡，他跑了3个，下党乡就是其中之一。",
        },
        {
          type: "your",
          content: "收到",
        },
      ],
      colors: ["#fef4ac", "#0018ba", "#ffc200", "#f32f15", "#cccccc", "#5ab639"],
      brushs: [
        {
          className: "small fa fa-paint-brush",
          lineWidth: 3,
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
      canvasMoveUse: true,
      // 存储当前表面状态数组-上一步
      preDrawAry: [],
      // 存储当前表面状态数组-下一步
      nextDrawAry: [],
      // 中间数组
      middleAry: [],
      // 配置参数
      config: {
        lineWidth: 1,
        shadowBlur: 2,
      },
    };
  },
  methods: {
    createEditor() {
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
      ];
      this.editor.create();
    },
    initCanvas() {
      const canvas = document.querySelector("#canvas");
      this.context = canvas.getContext("2d");
      this.initDraw();
      this.setCanvasStyle();
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
      const preData = this.context.getImageData(0, 0, 600, 400);
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
        this.context.lineTo(canvasX, canvasY);
        this.context.stroke();
      }
    },
    beginPath(e) {
      const canvas = document.querySelector("#canvas");
      if (e.target !== canvas) {
        console.log("beginPath");
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
      console.log("moveTo", canvasX, canvasY);
      // 当前绘图表面状态
      const preData = this.context.getImageData(0, 0, 600, 400);
      // 当前绘图表面进栈
      this.preDrawAry.push(preData);
    },
    // 设置颜色
    setColor(color) {
      this.config.lineColor = color;
    },
    // 设置笔刷大小
    setBrush(type) {
      this.config.lineWidth = type;
    },
    // 操作
    controlCanvas(action) {
      switch (action) {
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
    },
    // 生成图片
    getImage() {
      const canvas = document.querySelector("#canvas");
      const src = canvas.toDataURL("image/png");
      this.imgUrl.push(src);
      if (!this.isPc()) {
        // window.open(`data:text/plain,${src}`)
        window.location.href = src;
      }
    },
    // 设置绘画配置
    setCanvasStyle() {
      this.context.lineWidth = this.config.lineWidth;
      this.context.shadowBlur = this.config.shadowBlur;
      this.context.shadowColor = this.config.lineColor;
      this.context.strokeStyle = this.config.lineColor;
    },
  },
  mounted() {
    this.canvasWidth = this.$refs.canvasWrap.offsetWidth;
    this.canvasHeight = this.$refs.canvasWrap.offsetHeight;
    this.initCanvas();
    this.createEditor();
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
  created() {},
};
</script>

<style lang="scss" scoped>
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
    & #sendBtn {
      position: absolute;
      z-index: 99999;
      bottom: 5px;
      right: 8px;
    }
  }
}

.height_hb {
  height: 100%;
}
.height_lt {
  height: 100%;
}
@media screen and (max-width: 768px) {
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
  height: 100%;
  & .canvasWrap {
    height: calc(100% - 80px);
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
      padding-top: 10px !important;
      & span {
        display: inline-block;
        width: 20px;
        height: 15px;
        margin-left: 10px;
        cursor: pointer;
      }
      & .small {
        font-size: 12px;
      }
      & .middle {
        font-size: 14px;
      }
      & .big {
        font-size: 16px;
      }
    }
    #canvas-control {
      padding-top: 12px !important;
      & span {
        display: inline-block;
        font-size: 14px;
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
