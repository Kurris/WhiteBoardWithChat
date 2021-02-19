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
        static ConcurrentDictionary<string, string> _dicGrps = new ConcurrentDictionary<string, string>();
        static ConcurrentDictionary<string, string> _dicUsers = new ConcurrentDictionary<string, string>();
        static ConcurrentDictionary<string, string> _dicGrpMaster = new ConcurrentDictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            if (_dicGrps.ContainsKey(Context.ConnectionId))
            {
                await Clients.Caller.SendAsync("onConnected", "重复连接");
            }
            else
            {
                await Clients.Caller.SendAsync("onConnected", "连接成功");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_dicGrpMaster.TryGetValue(Context.ConnectionId, out string roomNameSMaster))
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
                        }
                    }
                }
                _dicGrpMaster.TryRemove(Context.ConnectionId, out _);
            }

            _dicGrps.TryRemove(Context.ConnectionId, out string roomName);
            _dicUsers.TryRemove(Context.ConnectionId, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task OnChatBoard(string content)
        {
            if (_dicGrps.TryGetValue(Context.ConnectionId, out string roomName))
            {
                await Clients.Group(roomName).SendAsync("onChatBoard", new Result()
                {
                    Status = true,
                    UserName = _dicUsers[Context.ConnectionId],
                    ConnectionId = Context.ConnectionId,
                    RoomName = roomName,
                    Content = content
                });
            }
            await Clients.Caller.SendAsync("onChatBoard", new Result()
            {
                Status = false
            });
        }

        public async Task OnPrintBoard(string content)
        {

        }

        public async Task<Result> CreateOrJoinRoom(string userName, string roomName)
        {

            if (!_dicGrpMaster.ContainsKey(Context.ConnectionId))
            {
                _dicGrpMaster.TryAdd(Context.ConnectionId, roomName);
            }

            if (!_dicGrps.ContainsKey(Context.ConnectionId))
            {
                _dicGrps.TryAdd(Context.ConnectionId, roomName);
            }

            if (!_dicUsers.ContainsKey(Context.ConnectionId))
            {
                _dicUsers.TryAdd(Context.ConnectionId, userName);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            return new Result()
            {
                Status = true,
                UserName = userName,
                ConnectionId = Context.ConnectionId,
                RoomName = roomName
            };
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
