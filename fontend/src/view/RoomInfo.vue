<template>
    <div>
        <div id="ca">
            <el-card style="margin-bottom:20px">
                <el-button type="primary" @click="createRoomVisible">创建房间</el-button>
                <el-button @click="refresh()">刷新</el-button>
                <span v-if="currentRow!=null">
                    选择房间号:{{currentRow.roomCode}}
                </span>
            </el-card>
            <el-card>

                <el-table :data="pagination.pageDatas" style="width: 100%" highlight-current-row row-click="rowClick">
                    <el-table-column prop="roomCode" label="房间号" width="180">
                    </el-table-column>
                    <el-table-column prop="roomName" label="房间名称" width="180">
                    </el-table-column>
                    <el-table-column prop="moderator" label="主持人">
                    </el-table-column>
                    <el-table-column label="操作" width="100">
                        <template slot-scope="scope">
                            <el-button type="warning" @click="joinRoom(scope.row)">加入房间</el-button>
                        </template>
                    </el-table-column>
                </el-table>

                <el-pagination @size-change="handleSizeChange" @current-change="handleCurrentChange" :current-page.sync="pagination.pageIndex" :page-size="pagination.pageSize" layout="sizes, prev, pager, next" :total="pagination.total">
                </el-pagination>
            </el-card>
        </div>

        <!-- 创建房间界面 -->
        <el-dialog title="创建房间" :visible.sync="createVisible" :show-close="false" width="300px" center :close-on-click-modal="false">
            <el-form :model="createForm" :rules="createRules" ref="createForm" label-width="100px">

                <el-form-item prop="isNeedPassword">
                    <el-checkbox v-model="createForm.isNeedPassword" label="需要密码?"></el-checkbox>
                </el-form-item>
                <el-form-item prop="roomName" label="房间名称">
                    <el-input v-model="createForm.roomName"></el-input>
                </el-form-item>

                <el-form-item v-if="createForm.isNeedPassword" :prop="chkPwd" label="用户密码">
                    <el-input type="password" v-model="createForm.password"></el-input>
                </el-form-item>
            </el-form>

            <div>
                <el-button type="primary" @click="createRoom('createForm')" style="float:right;width:100px;margin-bottom:20px">确定</el-button>
            </div>
        </el-dialog>
        <el-dialog title="请输入房间密码" :visible.sync="passwordVisible" :show-close="false" width="300px" center :close-on-click-modal="false">
            <el-input type="password" v-model="password"></el-input>
            <el-button type="primary" @click="currentJoin">确定</el-button>
        </el-dialog>
    </div>
</template>

<script>
import * as signalR from "@microsoft/signalr";
import Vue from "vue";

import request from '../api/index'

export default {
    name: 'room',
    data() {
        return {
            passwordVisible: false,
            createVisible: false,
            password: "",
            createForm: {
                roomName: "",
                isNeedPassword: false,
                password: "",
                owner: Number.parseInt(Vue.userId)
            },
            createRules: {
                roomName: [{ required: true, message: "用户名不能为空", trigger: "blur" }],
                password: [{ required: true, message: "密码不能为空", trigger: "blur" }],
            },
            currentRow: null,
            pagination: {
                //分页配置
                isASC: true,
                pageCount: 0,
                pageDatas: [],
                pageIndex: 1,
                pageSize: 20,
                sortColumn: null,
                total: 0,
            },
        }
    },
    methods: {
        createRoomVisible() {
            this.createVisible = true
        },
        currentJoin() {
            Vue.$signalR.invoke("JoinRoom", Vue.userId, this.currentRow.roomCode, this.password).then((res) => {
                if (res.status != 1001) {
                    this.$notify.warning(res.message);
                } else {
                    this.$router.push({
                        path: "/whiteboard",
                    });
                }
            }).catch(err => {
                this.$notify.error(err)
            });
        },
        joinRoom(row) {
            console.log(row);
            if (row.isNeedPassword) {
                this.currentRow = row
                this.passwordVisible = true
            } else {
                Vue.$signalR.invoke("JoinRoom", Vue.userId, row.roomCode, '').then((res) => {
                    if (res.status != 1001) {
                        this.$notify.warning(res.message);
                    } else {
                        this.$router.push({
                            path: "/whiteboard",
                        });
                    }
                }).catch(err => {
                    this.$notify.error(err)
                });
            }
        },
        createRoom(form) {
            this.$refs[form].validate(check => {
                if (check) {

                    Vue.$signalR.invoke("CreateRoom", this.createForm).then((res) => {
                        if (res.status != 1001) {
                            this.$notify.warning(res.message);
                        } else {
                            this.$router.push({
                                path: "/whiteboard",
                            });
                        }
                    }).catch(err => {
                        this.$notify.error(err)
                    });
                }
            })
        },
        rowClick(row) {
            this.currentRow = row
        },
        handleSizeChange(pageSize) {
            this.pagination.pageSize = pageSize;
            this.refresh();
        },
        handleCurrentChange(pageIndex) {
            this.pagination.pageIndex = pageIndex;
            this.refresh();
        },
        refresh() {
            request({
                method: 'get',
                url: 'Room/GetRoomsWithPagination',
                params: {
                    pageSize: this.pagination.pageSize,
                    pageIndex: this.pagination.pageIndex
                }
            }).then(res => {
                if (res && res.data != null) {
                    this.pagination = res.data
                } else {
                    this.pagination.pageDatas
                }
            })
        },
    },
    computed: {
        chkPwd() {
            if (this.createForm.isNeedPassword) {
                return 'password'
            }
            return ''
        }
    },
    mounted() {
        request({
            method: 'get',
            url: 'Room/GetRoomsWithPagination',
            params: {
                pageIndex: this.pagination.pageIndex,
                pageSize: this.pagination.pageSize
            }
        }).then(res => {
            if (res && res.data != null) {
                this.pagination.pageDatas = res.data
            } else {
                this.pagination.pageDatas = []
            }
        })
    },
    mounted() {
        let userId = Vue.userId
        if (userId == 'undefined' || userId == '' || userId == null) {
            this.$router.push({
                path: '/Login'
            })
        } else {
            let hubUrl = "http://localhost:5000/whiteboard";
            const connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();
            //连接开始
            connection.start().then(res => {

                request({
                    method: 'post',
                    url: 'User/SetConnectionId',
                    data: {
                        userId: Number.parseInt(userId),
                        id: connection.connectionId
                    }
                })

                Vue.isLogin = true;
                Vue.$signalR = connection;
            });

            this.refresh()
        }
    }

}
</script>

<style scoped>
#ca {
    position: absolute;
    left: 50%;
    top: 50%;
    transform: translate(-50%, -50%);
}
</style>