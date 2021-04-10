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

        public Task<TData<string>> SignIn(User user)
        {
            throw new NotImplementedException();
        }
    }
}
