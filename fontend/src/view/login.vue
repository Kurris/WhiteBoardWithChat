<template>
  <div>
    <div class="login-wrap">
      <div id="login-container">
        <div class="login-title">
          <b>聊天室</b>
        </div>

        <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="1px">
          <el-form-item prop="username">
            <el-input v-model="ruleForm.username" placeholder="用户名" class="userIpt" clearable>
              <el-button slot="prepend" icon="fa fa-user-circle"></el-button>
            </el-input>
          </el-form-item>

          <div class="login-btn">
            <el-button type="primary" style="float:right;width:100px" @click="inLogin">确定用户名称</el-button>
          </div>
        </el-form>
      </div>
    </div>

    <el-dialog
      title="加入房间方式"
      :visible.sync="chatVisible"
      :show-close="false"
      width="300px"
      class="homeList"
      center
    >
      <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="1px">
        <el-form-item prop="username">
          <el-input v-model="ruleForm.roomName" placeholder="房间号" clearable></el-input>
        </el-form-item>
      </el-form>

      <ul>
        <li>
          <el-button type="primary" @click="goHome(0)">创建房间</el-button>
        </li>
        <li>
          <el-button type="success" @click="goHome(1)">加入房间</el-button>
        </li>
      </ul>
    </el-dialog>
  </div>
</template>

<script>
import * as signalR from "@microsoft/signalr";
import Vue from "vue";

export default {
  data() {
    return {
      chatVisible: false,
      ruleForm: {
        username: "",
        roomName: "",
      },
      rules: {
        username: [{ required: true, message: "用户名不能为空", trigger: "blur" }],
      },
    };
  },
  methods: {
    async inLogin() {
      var validate = await this.$refs["ruleForm"].validate().catch((err) => {});
      if (!validate) return;
      this.chatVisible = true;
    },
    async goHome(type) {
      let hubUrl = "http://localhost:5000/whiteboard";
      const connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();

      //连接开始
      await connection.start();

      if (type == 0) {
        connection.invoke("CreateRoom", this.ruleForm.username, this.ruleForm.roomName).then((res) => {
          if (res.status) {
            this.chatVisible = false;
            this.$router.push({
              path: "/whiteboard",
              query: {
                roomName: this.ruleForm.roomName,
                userName: this.ruleForm.username,
              },
            });
          }
        });
      } else {
        connection.invoke("JoinRoom", this.ruleForm.username, this.ruleForm.roomName).then((res) => {
          if (!res.status) {
            this.$notify.warning(res.content);
          } else {
            this.chatVisible = false;
            this.$router.push({
              path: "/whiteboard",
              query: {
                roomName: this.ruleForm.roomName,
                userName: this.ruleForm.username,
              },
            });
          }
        });
      }
      Vue.isLogin = true;
      Vue.$signalR = connection;
    },
  },
  created() {},
  mounted() {},
};
</script>

<style lang="scss" scoped>
.userIpt {
  height: 40px;
  & ::v-deep.el-input__inner {
    height: 40px;
    line-height: 40px;
  }
}
.login-wrap {
  position: absolute;
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
  border: 1px solid #dcdfe6;
  width: 450px;
  padding: 35px 35px 15px 35px;
  border-radius: 5px;
  box-shadow: 0 0 25px #909399;
  background-color: white;
}

.login-title {
  text-align: center;
  font-size: 18px;
  margin: 0 auto 20px auto;
  color: #303133;
}
.homeList {
  text-align: center;
  ul,
  li {
    list-style: none;
  }
  li {
    margin-bottom: 5px;
  }
  .el-button--mini {
    width: 100%;
  }
}
</style>
