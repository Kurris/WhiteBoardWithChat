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
        static ConcurrentBag<string> _conlis = new ConcurrentBag<string>();

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("onConnected", "连接成功");
            _conlis.Add(Context.UserIdentifier);
        }

        public async Task OnBoard(string content)
        {
            await Clients.All.SendAsync("onBoard", content);
        }
    }
}
