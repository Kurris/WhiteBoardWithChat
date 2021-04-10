using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WhiteBoard.Entity;
using WhiteBoard.Model;
using WhiteBoard.Service;

namespace WhiteBoard.Controllers
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
        public async Task<TData<string>> Login(string userName, string password)
        {
            var res = await UserService.Login(userName, password);
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TData<string>> SignIn(User user)
        {
            var res = await UserService.SignIn(user);
            return res;
        }
    }
}
