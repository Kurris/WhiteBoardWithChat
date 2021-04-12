using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using WhiteBoard.Cfg;
using WhiteBoard.Core.Hubs;
using WhiteBoard.Filter;

namespace WhiteBoard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        private readonly string _corsPolicy = "AllowCors";
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddControllersAsServices();
            services.AddSignalR(option =>
            {
                option.MaximumReceiveMessageSize = 102400000;
            });

            services.AddCors(op =>
            {
                op.AddPolicy(_corsPolicy, builder =>
                {
                    builder.SetIsOriginAllowed(x => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });

            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "Ver 1",
                    Title = "WebApi",
                });

                string xml = Path.Combine(AppContext.BaseDirectory, "WhiteBoard.xml");
                option.IncludeXmlComments(xml);

            });

            services.AddMvc(option =>
            {
                option.Filters.AddService<CustomExceptionFilterAttribute>();
                option.Filters.AddService<CustomActionAndResultFilterAttribute>();
            });
        }

        /// <summary>
        /// 依赖注入Autofac
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder) => builder.RegisterModule<AutofacModule>();


        /// <summary>
        /// 请求管道
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            CultureInfo cultureInfo = new CultureInfo("zh-CN");
            cultureInfo.DateTimeFormat.AMDesignator = string.Empty;
            cultureInfo.DateTimeFormat.PMDesignator = string.Empty;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi");
            });

            app.UseRouting();
            app.UseCors(_corsPolicy);
            app.UseMiddleware<GlobalMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<WhiteBoardHub>("/WhiteBoard");
            });
        }
    }
}
