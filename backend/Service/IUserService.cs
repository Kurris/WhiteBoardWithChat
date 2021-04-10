using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhiteBoard.Entity;
using WhiteBoard.Model;

namespace WhiteBoard.Service
{
    public interface IUserService : IBaseService<User>
    {
        Task<TData<string>> Login(string userName, string password);
        Task<TData<string>> SignIn(User user);
    }
}
