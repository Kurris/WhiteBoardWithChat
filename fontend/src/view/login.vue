<template>
    <div>
        <div class="login-wrap">
            <div id="login-container">
                <div class="login-title">
                    <b>白板系统</b>
                </div>

                <!-- 登录界面 -->
                <el-form :model="loginForm" :rules="loginRules" ref="loginForm" label-width="1">
                    <el-form-item prop="userName">
                        <el-input v-model="loginForm.userName" placeholder="用户名" tabindex="1" clearable>
                            <el-button slot="prepend" class="w45" icon="fa fa-user-circle"></el-button>
                        </el-input>
                    </el-form-item>
                    <el-form-item prop="password">
                        <el-input tabindex="2" type="password" placeholder="密码" v-model="loginForm.password" @keyup.enter.native="submitForm('loginForm')" show-password clearable>
                            <el-button slot="prepend" class="w45" icon="fa fa-lock"></el-button>
                        </el-input>
                    </el-form-item>

                    <div class="login-btn">
                        <el-button @click="signUpVisible=true" style="width:100px">前往注册</el-button>
                        <el-button type="primary" @click="submitForm('loginForm')" style="float:right;width:100px">登录</el-button>
                    </div>
                </el-form>
            </div>
        </div>
        <!-- 注册界面 -->
        <el-dialog title="用户注册" :visible.sync="signUpVisible" :show-close="false" width="300px" center :close-on-click-modal="false">
            <el-form :model="signUpForm" :rules="signUpRules" ref="signUpForm" label-width="100px">
                <el-form-item prop="userName" label="登录账号">
                    <el-input v-model="signUpForm.userName" clearable auto-complete="new"></el-input>
                </el-form-item>
                <el-form-item prop="realName" label="真实姓名">
                    <el-input v-model="signUpForm.realName" clearable auto-complete="new"></el-input>
                </el-form-item>
                <el-form-item prop="password" label="用户密码">
                    <el-input type="password" v-model="signUpForm.password" clearable auto-complete="new"></el-input>
                </el-form-item>
            </el-form>

            <div class="login-btn">
                <el-button type="primary" @click="signUp('signUpForm')" style="float:right;width:100px;margin-bottom:20px">注册</el-button>
            </div>
        </el-dialog>
    </div>
    <!-- <div class="login-wrap">
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

        <el-dialog title="加入房间方式" :visible.sync="chatVisible" :show-close="false" width="300px" class="homeList" center>
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
    </div> -->
</template>

<script>
import request from '../api/index'
import Vue from "vue";

export default {
    data() {
        return {
            chatVisible: false,
            signUpVisible: false,
            loginForm: {
                userName: "",
                password: "",
            },
            loginRules: {
                userName: [{ required: true, message: "用户名不能为空", trigger: "blur" }],
                password: [{ required: true, message: "密码不能为空", trigger: "blur" }],
            },
            signUpForm: {
                userName: '',
                realName: '',
                password: '',
            },
            signUpRules: {
                userName: [{ required: true, message: "用户名不能为空", trigger: "blur" }],
                realName: [{ required: true, message: "真实姓名不能为空", trigger: "blur" }],
                password: [{ required: true, message: "密码不能为空", trigger: "blur" }],
            }
        };
    },
    methods: {
        signUp(form) {
            console.log(form);
            this.$refs[form].validate(check => {

                if (check) {
                    request({
                        method: 'post',
                        url: 'user/signup',
                        data: {
                            userName: this.signUpForm.userName,
                            realName: this.signUpForm.realName,
                            password: this.signUpForm.password,
                        }
                    }).then(res => {
                        this.$notify.success(res.message)
                        this.signUpVisible = false

                    }).finally(() => {
                        this.$refs[form].resetFields()
                    })
                }
            });
        },
        async inLogin() {
            var validate = await this.$refs["ruleForm"].validate().catch((err) => { });
            if (!validate) return;
            this.chatVisible = true;
        },
        async goHome(type) {


            //on 在前端注册方法, invoke 调用后端的方法
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

        },
        submitForm(form) {

            this.$refs[form].validate(check => {
                if (check) {
                    request({
                        method: 'get',
                        url: 'user/login',
                        params: {
                            userName: this.loginForm.userName,
                            password: this.loginForm.password,
                        }
                    }).then(res => {

                        if (res.status == 1000) {
                            Vue.userId = res.data.id
                            this.$notify.success(`登录成功  欢迎用户${res.data.realName}`)
                            this.$router.push("/roomInfo")
                        }
                    })
                }
            })
        },
    },
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

.yzmBtn {
    float: right;
    width: 100px;
    height: 35px;
}
#login-container {
    height: 35px;
    & ::v-deep.el-input__inner {
        height: 35px;
        line-height: 35px;
    }
}
.w45 {
    width: 45px;
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
    font-size: 20px;
    text-align: center;
    margin: 0 auto 20px auto;
    color: #303133;
}
</style>
