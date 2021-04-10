using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WhiteBoard.Model;
using WhiteBoard.Utils;

namespace WhiteBoard.Filter
{
    public class GlobalMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public GlobalMiddleware(RequestDelegate next, ILogger<GlobalMiddleware> logger, IConfiguration configuration)
        {
            this._next = next;
            this._logger = logger;
            this._configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

            }
            catch (Exception ex)
            {
                string msg = ex.GetBaseException().Message;
                _logger.LogError(msg);

                context.Response.StatusCode = 200;
                msg = $"内部发生异常{string.Concat(Environment.NewLine, msg)}";
                string result = JsonHelper.ToJson(new TData<string>()
                {
                    Data = null,
                    Message = msg,
                    Status = Status.Error
                }, new JsonSetting() { ContractResolver = ContractResolver.CamelCase });
                byte[] content = Encoding.UTF8.GetBytes(result);

                context.Response.ContentType = "application/json";
                context.Response.ContentLength = content.Length;
                await context.Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(content));
            }
        }
    }

}
