using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WhiteBoard.Service;
using WhiteBoard.Model;
using WhiteBoard.Entity;
using WhiteBoard.EF.Abstractions;

namespace WhiteBoard.Core.Hubs
{
    /// <summary>
    /// 白板系统即时通讯hub
    /// </summary>
    public class WhiteBoardHub : Hub
    {
        #region 依赖注入

        public IOnlineService OnlineService { get; set; }
        public IUserService UserService { get; set; }
        public IRoomService RoomService { get; set; }

        public IDataBaseOperation DataBaseOperation { get; set; }

        #endregion

        /// <summary>
        /// 断开链接
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            using (var op = DataBaseOperation)
            {
                //通过连接Id找到当前User
                var user = await op.AsNoTracking().FindAsync<User>(x => x.SignalRConnectionId == Context.ConnectionId);
                if (user != null)
                {
                    int userId = user.Id;
                    long roomCode = 0;

                    //判断是否为主持人
                    var tdRoom = await RoomService.FindAsync(x => x.Owner == userId);
                    if (tdRoom.IsValidationSuccessWithData())
                    {
                        roomCode = tdRoom.Data.RoomCode;

                        //移除当前在线的用户
                        //var onlines = await op.AsNoTracking().FindListAsync<Online>(x => x.RoomCode == roomCode);
                        await OnlineService.DeleteAsync(x => x.RoomCode == roomCode);
                        await RoomService.DeleteAsync(x => x.RoomCode == roomCode);

                    }
                    //普通用户
                    else
                    {
                        var online = await op.FindAsync<Online>(x => x.UserId == userId);
                        if (online != null)
                        {
                            roomCode = online.RoomCode;
                        }
                        await OnlineService.DeleteAsync(x => x.UserId == userId);
                    }

                    //清空Id
                    user = new User() { Id = user.Id, SignalRConnectionId = string.Empty };
                    await UserService.SaveAsync(user);

                    //signalR移除
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode.ToString());
                }
            }

        }

        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public async Task<TData<string>> CreateRoom(Room room)
        {
            var res = await RoomService.CreateRoom(room);
            if (res.Status == Status.Success)
            {
                //当前id加入到组里面
                await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomCode.ToString());
            }
            return res;
        }


        /// <summary>
        /// 用户加入房间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roomCode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<TData<string>> JoinRoom(int userId, long roomCode, string password)
        {
            var res = await RoomService.JoinRoom(userId, roomCode, password);
            if (res.Status == Status.Success)
            {
                //当前id加入到组里面
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode.ToString());
            }
            return res;
        }


        /// <summary>
        /// 聊天信息广播
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task OnChatBoard(string content)
        {
            var tdUser = await UserService.GetUserBySignalRConnectionId(Context.ConnectionId);
            if (tdUser.IsValidationSuccessWithData())
            {
                var tdOnline = await OnlineService.FindAsync(x => x.UserId == tdUser.Data.Id);
                if (tdOnline.IsValidationSuccessWithData())
                {
                    string roomCode = tdOnline.Data.RoomCode.ToString();

                    var group = Clients.Group(roomCode);
                    if (group != null)
                    {
                        await group.SendAsync("onChatBoard", new Result()
                        {
                            Status = true,
                            UserName = tdUser.Data.RealName,
                            ConnectionId = Context.ConnectionId,
                            Content = content
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 绘画广播
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task OnPrintBoard(string content)
        {
            var tdUser = await UserService.GetUserBySignalRConnectionId(Context.ConnectionId);
            if (tdUser.IsValidationSuccessWithData())
            {
                var tdOnline = await OnlineService.FindAsync(x => x.UserId == tdUser.Data.Id);
                if (tdOnline.IsValidationSuccessWithData())
                {
                    if (tdOnline.Data.Role == Role.Moderator || tdOnline.Data.Role == Role.TemporaryUser)
                    {
                        string roomCode = tdOnline.Data.RoomCode.ToString();

                        var group = Clients.Group(roomCode);
                        if (group != null)
                        {
                            await group.SendAsync("onPrintBoard", new Result()
                            {
                                Status = true,
                                UserName = tdUser.Data.RealName,
                                ConnectionId = Context.ConnectionId,
                                Content = content
                            });
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 画板的特定操作
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task OnAction(string action)
        {
            var tdUser = await UserService.GetUserBySignalRConnectionId(Context.ConnectionId);
            if (tdUser.IsValidationSuccessWithData())
            {
                var tdOnline = await OnlineService.FindAsync(x => x.UserId == tdUser.Data.Id);
                if (tdOnline.IsValidationSuccessWithData())
                {
                    if (tdOnline.Data.Role == Role.Moderator || tdOnline.Data.Role == Role.TemporaryUser)
                    {
                        string roomCode = tdOnline.Data.RoomCode.ToString();

                        var group = Clients.Group(roomCode);
                        if (group != null)
                        {
                            await group.SendAsync("onAction", new
                            {
                                action = action,
                                id = Context.ConnectionId
                            });
                        }
                    }
                }
            }
        }

        public async Task SetPermission(int userId, long roomCode)
        {
            var tdOnline = await OnlineService.FindAsync(x => x.UserId == userId && x.RoomCode == roomCode);
            if (tdOnline.IsValidationSuccessWithData())
            {
                var tdUser = await UserService.FindAsync(x => x.Id == userId);
                if (tdUser.IsValidationSuccessWithData())
                {
                    var connectionId = tdUser.Data.SignalRConnectionId;
                    var online = tdOnline.Data;

                    if (online.Role == Role.TemporaryUser)
                    {
                        online.Role = Role.Member;
                        await Clients.Client(connectionId).SendAsync("setPermission", false);
                    }
                    else
                    {
                        online.Role = Role.TemporaryUser;
                        await Clients.Client(connectionId).SendAsync("setPermission", true);
                    }

                    await OnlineService.SaveAsync(online);
                }
            }
        }

        public async Task<bool> GetPermission()
        {
            var tdUser = await UserService.FindAsync(x => x.SignalRConnectionId == Context.ConnectionId);
            if (tdUser.IsValidationSuccessWithData())
            {
                var tdOnline = await OnlineService.FindAsync(x => x.UserId == tdUser.Data.Id);
                if (tdOnline.IsValidationSuccessWithData())
                {
                    return tdOnline.Data.Role == Entity.Role.Moderator || tdOnline.Data.Role == Entity.Role.TemporaryUser;
                }
            }
            return false;
        }

        public async Task<bool> GetOwner()
        {
            var tdUser = await UserService.FindAsync(x => x.SignalRConnectionId == Context.ConnectionId);
            if (tdUser.IsValidationSuccessWithData())
            {
                var tdOnline = await OnlineService.FindAsync(x => x.UserId == tdUser.Data.Id);
                if (tdOnline.IsValidationSuccessWithData())
                {
                    return tdOnline.Data.Role == Entity.Role.Moderator;
                }
            }
            return false;
        }

        public async Task<IEnumerable<object>> GetUsers()
        {
            List<object> list = new List<object>();

            var tdUsers = await UserService.FindListAsync();
            if (tdUsers.IsValidationSuccessWithData())
            {
                var tdUser = await UserService.FindAsync(x => x.SignalRConnectionId == Context.ConnectionId);
                if (tdUser.IsValidationSuccessWithData())
                {
                    var tdOnline = await OnlineService.FindAsync(x => x.UserId == tdUser.Data.Id && (x.Role == Entity.Role.Moderator || x.Role == Entity.Role.Admin));
                    if (tdOnline.IsValidationSuccessWithData())
                    {
                        var tdOnlines = await OnlineService.FindListAsync(x => x.RoomCode == tdOnline.Data.RoomCode);
                        if (tdOnlines.IsValidationSuccessWithData())
                        {
                            foreach (var item in tdOnlines.Data)
                            {
                                list.Add(new
                                {
                                    id = item.UserId,
                                    userName = tdUsers.Data.FirstOrDefault(x => x.Id == item.UserId)?.RealName
                                });
                            }
                        }
                    }
                }
            }

            return list;
        }
    }

    public class Result
    {
        public bool Status { get; set; }
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public string CurrentTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string Content { get; set; }
    }
}
