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
        public async Task<TData<string>> Login(string userName, string password)
        {
            TData<string> tdResult = new TData<string>();
            await using (var op = await DataBaseOperation.BeginTransAsync())
            {
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
                            tdResult.Success("登陆成功");
                        }
                    }
                    await op.CommitTransAsync();
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }

        public async Task<TData<string>> SignIn(User user)
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
