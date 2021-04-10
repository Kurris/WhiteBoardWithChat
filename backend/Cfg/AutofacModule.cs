using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using WhiteBoard.EF.Abstractions;

namespace WhiteBoard.Cfg
{
    public class AutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            string local = AppContext.BaseDirectory;

            var types = Assembly.GetExecutingAssembly().GetTypes();

            //控制器和过滤器注入,可使用属性
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(x => x.IsSubclassOf(typeof(ControllerBase))
                         || x.IsSubclassOf(typeof(Attribute))).PropertiesAutowired();


            //注入数据库访问
            var efTypes = types.Where(x => x.FullName.StartsWith("WhiteBoard.EF", StringComparison.OrdinalIgnoreCase));
            foreach (var type in efTypes)
            {
                if (type.GetConstructors().Count() > 0)
                {
                    builder.RegisterType(type).InstancePerDependency().PropertiesAutowired();
                }
            }
            builder.RegisterType(efTypes.First(x => x.Name == "MySqlDB")).As<IDataBaseOperation>().InstancePerDependency().PropertiesAutowired();


            //业务注入
            var businessTypes = types.Where(x => x.FullName.StartsWith("WhiteBoard.Service", StringComparison.OrdinalIgnoreCase));
            builder.RegisterTypes(businessTypes.ToArray()).InstancePerLifetimeScope().AsImplementedInterfaces().PropertiesAutowired();
        }
    }
}
