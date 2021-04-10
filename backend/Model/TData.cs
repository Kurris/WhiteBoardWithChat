using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhiteBoard.Model
{
    /// <summary>
    /// 数据结果返回模型
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class TData<T>
    {
        /// <summary>
        /// 数据结果返回模型
        /// <code>
        /// Message = null;
        /// Status = Status.Error;
        /// Data = Default(T)
        /// </code>
        /// </summary>
        public TData()
        {
            this.Status = Status.Error;
        }

        /// <summary>
        /// 数据结果返回模型
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="data">数据</param>
        /// <param name="status">状态</param>
        public TData(string message, T data, Status status)
        {
            this.Message = message;
            this.Data = data;
            this.Status = status;
        }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 结果内容
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 状态(默认:Error 5000)
        /// </summary>
        public Status Status { get; set; }
    }

    /// <summary>
    /// 返回状态
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// 登录成功
        /// </summary>
        LoginSuccess = 1000,

        /// <summary>
        /// 操作成功
        /// </summary>
        Success = 1001,


        /// <summary>
        /// 操作失败
        /// </summary>
        Fail = 1002,

        /// <summary>
        /// 操作被取消
        /// </summary>
        Cancel = 1003,


        /// <summary>
        /// 鉴权失败
        /// </summary>
        AuthorizationFail = 4000,

        /// <summary>
        /// 无权限
        /// </summary>
        NoPermission = 4001,


        /// <summary>
        /// 实体验证失败
        /// </summary>
        ValidateEntityError = 4002,

        /// <summary>
        /// 执行异常
        /// </summary>
        Error = 5000,
    }


    /// <summary>
    /// 状态处理
    /// </summary>
    public static class MapperStatus
    {
        public static TData<Pagination<TOut>> TempPagination<TIn, TOut>(this TData<Pagination<TIn>> tdata)
        {
            var tdResult = new TData<Pagination<TOut>>()
            {
                Message = tdata.Message,
                Status = tdata.Status,
            };

            return tdResult;
        }

        public static TData<IEnumerable<TOut>> TempEnumerable<TIn, TOut>(this TData<IEnumerable<TIn>> tdata)
        {
            var tdResult = new TData<IEnumerable<TOut>>()
            {
                Message = tdata.Message,
                Status = tdata.Status,
            };

            return tdResult;
        }

        public static TData<TOut> Temp<TIn, TOut>(this TData<TIn> tdata)
        {
            var tdResult = new TData<TOut>()
            {
                Message = tdata.Message,
                Status = tdata.Status,
            };

            return tdResult;
        }

        public static TData<T> Error<T>(this TData<T> tdata, Exception ex)
        {
            tdata.Message = ex.GetBaseException().Message;
            tdata.Status = Status.Error;
            return tdata;
        }

        public static TData<T> Error<T>(this TData<T> tdata, string ex)
        {
            tdata.Message = ex;
            tdata.Status = Status.Error;
            return tdata;
        }


        public static TData<T> Fail<T>(this TData<T> tdata, string failMsg)
        {
            tdata.Message = failMsg;
            tdata.Status = Status.Fail;
            return tdata;
        }

        public static TData<T> Cancel<T>(this TData<T> tdata, string failMsg)
        {
            tdata.Message = failMsg;
            tdata.Status = Status.Cancel;
            return tdata;
        }


        public static TData<T> Success<T>(this TData<T> tdata, string successMsg)
        {
            tdata.Message = successMsg;
            tdata.Status = Status.Success;
            return tdata;
        }

        public static TData<T> Success<T>(this TData<T> tdata, string successMsg, T data, Status status = Status.Success)
        {
            tdata.Message = successMsg;
            tdata.Status = status;
            tdata.Data = data;

            return tdata;
        }

        public static bool IsValidationSuccessWithData<T>(this TData<Pagination<T>> tdDatas)
        {
            if (tdDatas.Status == Status.Success)
            {
                if (tdDatas.Data != null && tdDatas.Data.PageDatas != null && tdDatas.Data.PageDatas.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsValidationSuccessWithData<T>(this TData<IEnumerable<T>> tdDatas)
        {
            if (tdDatas.Status == Status.Success)
            {
                if (tdDatas.Data != null && tdDatas.Data.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidationSuccessWithData<T>(this TData<T> tdDatas)
        {
            if (tdDatas.Status == Status.Success)
            {
                if (tdDatas.Data != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidationSuccessWithData<T>(this IEnumerable<T> datas)
        {
            return datas != null && datas.Count() > 0;
        }
    }
}
