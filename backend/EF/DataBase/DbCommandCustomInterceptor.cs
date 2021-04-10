using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WhiteBoard.EF.DataBase
{
    /// <summary>
    /// 自定义数据库拦截器
    /// </summary>
    public class DbCommandCustomInterceptor : DbCommandInterceptor
    {
        private readonly int _dBSlowSqlLogTime = 3;
        public ILogger<DbCommandCustomInterceptor> Logger { get; set; }
        public DbCommandCustomInterceptor(IConfiguration configuration)
        {
            this._dBSlowSqlLogTime = configuration.GetSection("SystemConfig").GetSection("DBConfig:DBSlowSqlLogTime").Get<int>();
        }

        public override async Task<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (eventData.Duration.TotalMilliseconds >= this._dBSlowSqlLogTime * 1000)
            {
                string log = $"耗时:{eventData.Duration.TotalSeconds}秒\r\n" +
                             $"语句:{command.CommandText}";
                Logger.LogWarning(log);
            }
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override async Task<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (eventData.Duration.TotalMilliseconds >= this._dBSlowSqlLogTime * 1000)
            {
                string log = $"耗时:{eventData.Duration.TotalSeconds}秒\r\n" +
                             $"语句:{command.CommandText}";
                Logger.LogWarning(log);
            }
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override async Task<object> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object result, CancellationToken cancellationToken = default)
        {
            if (eventData.Duration.TotalMilliseconds >= this._dBSlowSqlLogTime * 1000)
            {
                string log = $"耗时:{eventData.Duration.TotalSeconds}秒\r\n" +
                             $"语句:{command.CommandText}";
                Logger.LogWarning(log);
            }
            return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override async Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            string log = $"异常:{eventData.Exception.Message}\r\n" +
                         $"语句:{command.CommandText}";
            Logger.LogError(log);
            await base.CommandFailedAsync(command, eventData, cancellationToken);
        }
    }
}
