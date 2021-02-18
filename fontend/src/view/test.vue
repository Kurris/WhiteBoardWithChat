<template>
    <div class="hello">
        <el-input v-model="user" type="text" />
        <div id="message" v-html="remsg"></div>
        <div id="el-input">
            <el-input id="chatbox" @keyup.native.enter="handle" type="textarea" :rows="1" placeholder="请输入内容" v-model="msg"></el-input>
        </div>
        <el-button size="small" style="display:inline-block;" icon="el-icon-s-promotion" type="suceess" @click="handle" plain></el-button>
    </div>
</template>

<script>
import * as signalR from "@microsoft/signalr";


export default {
    name: "Im",

    mounted() {
        var _this = this;
        //实现Show方法
        connection.on("Show", function (username, message) {
            _this.remsg = _this.remsg + "<br>" + username + ":" + message;
        });
    },
    data() {
        return {
            user: "",
            msg: "",
            remsg: "",
        };
    },

    methods: {
        handle: function () {
            if (this.msg.trim() == "") {
                alert("不能发送空白消息");
                return;
            }
            //调用后端方法 SendMsg 传入参数
            connection.invoke("SendMsg", this.user, this.msg);
            this.msg = "";
        },
    },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h1,
h2 {
    font-weight: normal;
}
ul {
    list-style-type: none;
    padding: 0;
}
li {
    display: inline-block;
    margin: 0 10px;
}
a {
    color: #42b983;
}
#el-input {
    display: inline-block;
    width: 96%;
    float: left;
}
#message {
    overflow-y: auto;
    text-align: left;
    border: #42b983 solid 1px;
    height: 500px;
}
</style>
