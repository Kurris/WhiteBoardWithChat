using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace WhiteBoard
{
    public class WhiteBoard : Hub
    {
        //id room
        static ConcurrentDictionary<string, string> _dicGrps = new ConcurrentDictionary<string, string>();

        //id userName
        static ConcurrentDictionary<string, string> _dicUsers = new ConcurrentDictionary<string, string>();

        //id room
        static ConcurrentDictionary<string, string> _dicGrpOwner = new ConcurrentDictionary<string, string>();

        //id room
        static ConcurrentDictionary<string, string> _dicGrpPermission = new ConcurrentDictionary<string, string>();



        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_dicGrpOwner.TryGetValue(Context.ConnectionId, out string roomNameSMaster))
            {
                if (!string.IsNullOrEmpty(roomNameSMaster))
                {
                    foreach (var item in _dicGrps)
                    {
                        string key = item.Key;
                        string value = item.Value;

                        if (value == roomNameSMaster)
                        {
                            _dicGrps.TryRemove(key, out _);
                            _dicUsers.TryRemove(key, out _);
                            _dicGrpPermission.TryRemove(key, out _);
                            await Groups.RemoveFromGroupAsync(key, roomNameSMaster);
                        }
                    }
                }
                _dicGrpOwner.TryRemove(Context.ConnectionId, out _);
            }

            _dicGrpPermission.TryRemove(Context.ConnectionId, out _);
            _dicGrps.TryRemove(Context.ConnectionId, out string roomName);
            _dicUsers.TryRemove(Context.ConnectionId, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task<Result> CreateRoom(string userName, string roomName)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(roomName))
            {
                return new Result()
                {
                    Status = false,
                    Content = "用户名称和房间号不能为空"
                };
            }


            if (!_dicGrpOwner.ContainsKey(Context.ConnectionId))
            {
                foreach (var item in _dicGrpOwner)
                {
                    if (item.Value == roomName)
                    {
                        return new Result()
                        {
                            Status = false,
                            Content = $"房间号 {roomName} 已存在"
                        };
                    }
                }
                _dicGrpOwner.TryAdd(Context.ConnectionId, roomName);
            }

            var res = await JoinRoom(userName, roomName);
            return res;
        }
        public async Task<Result> JoinRoom(string userName, string roomName)
        {

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(roomName))
            {
                return new Result()
                {
                    Status = false,
                    Content = "用户名称和房间号不能为空"
                };
            }

            bool existRoom = false;
            foreach (var item in _dicGrpOwner)
            {
                if (item.Value == roomName) existRoom = true;
            }

            if (!existRoom)
            {
                return new Result()
                {
                    Status = false,
                    Content = $"不存在房间号 {roomName}"
                };
            }

            //加入房间
            if (!_dicGrps.ContainsKey(Context.ConnectionId))
            {
                _dicGrps.TryAdd(Context.ConnectionId, roomName);
            }
            //当前id加入到组里面
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            //记录用户名称
            if (!_dicUsers.ContainsKey(Context.ConnectionId))
            {
                _dicUsers.TryAdd(Context.ConnectionId, userName);
            }

            return new Result()
            {
                Status = true,
                ConnectionId = Context.ConnectionId,
                UserName = userName,
                RoomName = roomName,
            };
        }

        public async Task OnChatBoard(string content)
        {
            if (_dicGrps.TryGetValue(Context.ConnectionId, out string roomName))
            {
                var group = Clients.Group(roomName);
                if (group != null)
                {
                    await group.SendAsync("onChatBoard", new Result()
                    {
                        Status = true,
                        UserName = _dicUsers[Context.ConnectionId],
                        ConnectionId = Context.ConnectionId,
                        RoomName = roomName,
                        Content = content
                    });
                }
            }
        }
        public async Task OnPrintBoard(string content)
        {
            string roomName = string.Empty;
            if (_dicGrpOwner.TryGetValue(Context.ConnectionId, out roomName) || _dicGrpPermission.TryGetValue(Context.ConnectionId, out roomName))
            {
                var group = Clients.Group(roomName);
                if (group != null)
                {
                    await group.SendAsync("onPrintBoard", new Result()
                    {
                        Status = true,
                        UserName = _dicUsers[Context.ConnectionId],
                        ConnectionId = Context.ConnectionId,
                        RoomName = roomName,
                        Content = content
                    });
                }
            }
        }

        public async Task OnAction(string action)
        {
            string roomName = string.Empty;
            if (_dicGrpOwner.TryGetValue(Context.ConnectionId, out roomName) || _dicGrpPermission.TryGetValue(Context.ConnectionId, out roomName))
            {
                var group = Clients.Group(roomName);
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

        public async Task SetPermission(string id, string roomName)
        {
            if (_dicGrpPermission.ContainsKey(id))
            {
                _dicGrpPermission.TryRemove(id, out _);
                await Clients.Client(id).SendAsync("setPermission", false);
            }
            else
            {
                _dicGrpPermission.TryAdd(id, roomName);
                await Clients.Client(id).SendAsync("setPermission", true);
            }

        }
        public bool GetPermission()
        {
            return _dicGrpOwner.ContainsKey(Context.ConnectionId) || _dicGrpPermission.ContainsKey(Context.ConnectionId);
        }

        public bool GetOwner()
        {
            return _dicGrpOwner.ContainsKey(Context.ConnectionId);
        }

        public IEnumerable<object> GetUsers()
        {
            List<object> list = new List<object>();
            if (_dicGrpOwner.TryGetValue(Context.ConnectionId, out string roomName))
            {
                foreach (var item in _dicGrps)
                {
                    if (item.Value == roomName)
                    {
                        list.Add(new
                        {
                            id = item.Key,
                            userName = _dicUsers[item.Key],
                        });
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
        public string RoomName { get; set; }
        public string Content { get; set; }
    }
}
