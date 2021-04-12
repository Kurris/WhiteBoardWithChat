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
        Task<TData<User>> Login(string userName, string password);
        Task<TData<string>> SignUp(User user);
        Task<TData<User>> GetUserBySignalRConnectionId(string id);
        Task<TData<int>> SetSignalRConnectionId(int userId, string id);
    }
}
