using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhiteBoard.EF.Abstractions;

namespace WhiteBoard.EF.DataBase
{
    /// <summary>
    /// MySql数据库
    /// </summary>
    public class MySqlDB : BaseDatabaseImp
    {
        public MySqlDB(MyDbContext myDbContext) : base(myDbContext)
        {
        }

        /*----------------------------------------------重写基类默认的sql行为---------------------------------------------------*/

        public override async Task<int> ExecProcAsync(string procName, IDictionary<string, object> keyValues = null)
        {
            await RunSqlAsync($"CALL {procName};", keyValues);
            return await GetOperationReuslt();
        }
    }
}
