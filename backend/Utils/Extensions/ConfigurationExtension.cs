using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WhiteBoard.Utils.Extensions
{
    public static class ConfigurationExtension
    {
        public static T GetSystemConfig<T>(this IConfiguration configuration, string key)
        {
            return configuration.GetSection($"SystemConfig:{key}").Get<T>();
        }

        public static T GetDBConfig<T>(this IConfiguration configuration, string key)
        {
            return configuration.GetSection("SystemConfig").GetSection($"DBConfig:{key}").Get<T>();
        }
    }

    public class DBConfigConst
    {
        public const string dbSlowSqlLogTime = "DBSlowSqlLogTime";
        public const string sqlConnectionString = "SqlConnectionString";
        public const string timeout = "Timeout";
    }
}
