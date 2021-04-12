using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WhiteBoard.Entity;
using WhiteBoard.Model;
using WhiteBoard.Utils;

namespace WhiteBoard.Service.implements
{
    public class UserService : BaseService<User>, IUserService
    {
        public async Task<TData<User>> GetUserBySignalRConnectionId(string id)
        {
            await using (DataBaseOperation.AsNoTracking())
            {
                var tdUser = await this.FindAsync(x => x.SignalRConnectionId == id);
                return tdUser;
            }
        }

        public async Task<TData<User>> Login(string userName, string password)
        {
            await using (var op = await DataBaseOperation.BeginTransAsync())
            {
                TData<User> tdResult = new TData<User>();
                try
                {
                    User user = await op.AsNoTracking<User>().Where(x => x.UserName == userName).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        tdResult.Fail("用户不存在");
                    }
                    else
                    {
                        string encrypt = SecurityHelper.MD5Encrypt(password);
                        if (user.Password != encrypt)
                        {
                            tdResult.Fail("密码错误");
                        }
                        else
                        {
                            user.Password = string.Empty;
                            tdResult.Success("登陆成功", user, Status.LoginSuccess);
                        }
                    }
                    await op.CommitTransAsync();
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
                return tdResult;
            }
        }

        public async Task<TData<int>> SetSignalRConnectionId(int userId, string id)
        {
            await using (var op = await DataBaseOperation.BeginTransAsync())
            {
                TData<int> tdResult = new TData<int>();
                tdResult.Fail("查询失败");
                var user = await op.AsNoTracking().FindAsync<User>(x => x.Id == userId);
                if (user != null)
                {
                    User userSave = new User()
                    {
                        Id = userId,
                        SignalRConnectionId = id
                    };
                    await this.SaveAsyncWithNoTrans(userSave);

                    tdResult.Success("设置成功");
                }
               

                await op.CommitTransAsync();

                return tdResult;
            }
        }

        public async Task<TData<string>> SignUp(User user)
        {
            TData<string> tdResult = new TData<string>();
            await using (var op = await DataBaseOperation.BeginTransAsync())
            {
                try
                {
                    User existUser = await op.FindAsync<User>(x => x.UserName == user.UserName);
                    if (existUser != null)
                    {
                        tdResult.Fail($"注册失败,已经存在用户{user.UserName}");
                    }
                    else
                    {
                        user.Password = SecurityHelper.MD5Encrypt(user.Password);
                        await op.AddAsync(user);
                        await op.CommitTransAsync();
                        tdResult.Success("注册成功");
                    }
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }
    }
}
