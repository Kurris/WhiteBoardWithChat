﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using WhiteBoard.Model;
using WhiteBoard.Utils;

namespace WhiteBoard.Filter
{
    /// <summary>
    /// 自定义Action和Result过滤器
    /// </summary>
    public class CustomActionAndResultFilterAttribute : ActionFilterAttribute
    {
        public ILogger<CustomActionAndResultFilterAttribute> Logger { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sLog = $"【Controller】:{context.RouteData.Values["controller"]}\r\n" +
                       $"【Action】:{context.RouteData.Values["action"]}\r\n" +
                       $"【Paras】：{(context.ActionArguments.Count == 0 ? "None" : JsonHelper.ToJson(context.ActionArguments))}";
            Logger.LogInformation(sLog);

            await base.OnActionExecutionAsync(context, next);
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errorResult = context.ModelState.Keys.SelectMany(key =>
                context.ModelState[key].Errors.Where(x => !string.IsNullOrEmpty(key)).Select(x =>
                {
                    return new
                    {
                        Field = key,
                        Message = x.ErrorMessage
                    };
                }));

                context.Result = new ObjectResult(new TData<IEnumerable<object>>(
                    message: "参数不合法",
                    data: errorResult,
                    status: Status.ValidateEntityError));
            }
            else
            {
                object successResult = (context.Result as ObjectResult)?.Value;

                if (successResult != null)
                {
                    if (successResult.GetType().Name.StartsWith("TData"))
                        context.Result = new ObjectResult(successResult);
                    else
                        context.Result = new ObjectResult(new TData<object>("请求成功", successResult, Status.Success));
                }
            }

            await base.OnResultExecutionAsync(context, next);
        }
    }
}
