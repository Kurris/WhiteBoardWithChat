using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace WhiteBoard.Utils
{
    /// <summary>
    /// json帮助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// Json序列化
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(object obj)
        {
            if (obj == null) return string.Empty;
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Json序列化
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="jsonSetting">序列化配置</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(object obj, JsonSetting jsonSetting)
        {
            if (obj == null) return string.Empty;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = jsonSetting.ContractResolver switch
                {
                    ContractResolver.CamelCase => new CamelCasePropertyNamesContractResolver(),
                    _ => null,
                },
                ReferenceLoopHandling = (ReferenceLoopHandling)jsonSetting.LoopHandling
            };

            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">字符串</param>
        /// <returns>泛型的具体类型<see cref="{T}"/></returns>
        public static T ToObejct<T>(string jsonStr) => string.IsNullOrEmpty(jsonStr) ? default : JsonConvert.DeserializeObject<T>(jsonStr);

        /// <summary>
        /// Json转JObject
        /// </summary>
        /// <param name="jsonStr">json字符串</param>
        /// <returns>JObject对象<see cref="JObject"/></returns>
        public static JObject ToJObject(string jsonStr) => string.IsNullOrEmpty(jsonStr) ? JObject.Parse("{}") : JObject.Parse(jsonStr);

        /// <summary>
        /// Json转JToken
        /// </summary>
        /// <param name="jsonArrayStr">json数组字符串</param>
        /// <returns>JToken数组<see cref="JToken"/></returns>
        public static JToken ToJToken(string jsonArrayStr) => string.IsNullOrEmpty(jsonArrayStr) ? JToken.Parse("[]") : JToken.Parse(jsonArrayStr);
    }

    /// <summary>
    /// Json序列化配置
    /// </summary>
    public class JsonSetting
    {
        /// <summary>
        /// 循环处理
        /// </summary>
        public LoopHandling LoopHandling { get; set; }

        /// <summary>
        /// 序列化约定
        /// </summary>
        public ContractResolver? ContractResolver { get; set; }
    }

    /// <summary>
    /// 序列化约定
    /// </summary>
    public enum ContractResolver
    {
        CamelCase = 0
    }

    /// <summary>
    /// 循环处理
    /// </summary>
    public enum LoopHandling
    {
        /// <summary>
        /// 错误(默认)
        /// </summary>
        Error = 0,

        /// <summary>
        /// 忽略
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// 序列化
        /// </summary>
        Serialize = 2
    }

}
