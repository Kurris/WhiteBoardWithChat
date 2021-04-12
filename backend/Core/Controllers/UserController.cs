using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WhiteBoard.Entity;
using WhiteBoard.Model;
using WhiteBoard.Service;

namespace WhiteBoard.Core.Controllers
{
    /// <summary>
    /// 用户控制器
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public IUserService UserService { get; set; }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns><see cref="TData{string}"/></returns>
        [HttpGet]
        public async Task<TData<User>> Login(string userName, string password)
        {
            var res = await UserService.Login(userName, password);
            return res;
        }

        /// <summary>
        /// 通过connId找用户
        /// </summary>
        /// <param name="connId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TData<User>> GetUser(string connId)
        {
            var res = await UserService.FindAsync(x => x.SignalRConnectionId == connId);
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TData<string>> SignUp(User user)
        {
            var res = await UserService.SignUp(user);
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<TData<int>> SetConnectionId(SetId setId)
        {
            var res = await UserService.SetSignalRConnectionId(setId.UserId, setId.Id);
            return res;
        }

        public class SetId
        {
            public int UserId { get; set; }
            public string Id { get; set; }
        }
    }
}
