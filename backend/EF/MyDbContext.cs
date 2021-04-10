using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WhiteBoard.EF.DataBase;
using WhiteBoard.Entity;
using WhiteBoard.Utils.Extensions;

namespace WhiteBoard.EF
{
    public sealed class MyDbContext : DbContext
    {
        private readonly DbCommandCustomInterceptor dbCommandInterceptor;

        public MyDbContext(DbCommandCustomInterceptor dbCommandInterceptor, IConfiguration configuration)
        {
            this.dbCommandInterceptor = dbCommandInterceptor;
            _connStr = configuration.GetDBConfig<string>(DBConfigConst.sqlConnectionString);
            _timeout = configuration.GetDBConfig<int>(DBConfigConst.timeout);
        }

        readonly string _connStr = string.Empty;
        readonly int _timeout = 10;

        public MyDbContext()
        {
            _connStr = "data source=47.116.143.51;database=WhiteBoard; uid=root;pwd=Sa123456!;";
            _timeout = 10;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_connStr, x => x.CommandTimeout(_timeout));
            if (dbCommandInterceptor != null) optionsBuilder.AddInterceptors(dbCommandInterceptor);
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder =>
            {
                builder.AddFilter((string category, LogLevel level) =>
                category == DbLoggerCategory.Database.Command.Name
                && level == LogLevel.Information).AddConsole();
            }));
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseEntity)) && x.IsDefined(typeof(TableAttribute)));
            foreach (var entityType in entityTypes)
            {
                if (modelBuilder.Model.GetEntityTypes().Any(x => x.Name.Equals(entityType.FullName)))
                {
                    continue;
                }
                modelBuilder.Entity(entityType);
            }

            var DateTimeConverter = new ValueConverter<DateTime, DateTime>(
               v => DateTime.Parse(v.ToString("yyyy-MM-dd HH:mm:ss")),
               v => DateTime.Parse(v.ToString("yyyy-MM-dd HH:mm:ss"))
               );

            foreach (var item in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var prop in item.GetProperties().Where(x => x.ClrType == typeof(DateTime) || x.ClrType == typeof(DateTime?)))
                {
                    prop.SetValueConverter(DateTimeConverter);
                    prop.SetMaxLength(14);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
