using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;


namespace WhiteBoard.EF.DataBase.Extension
{
    /// <summary>
    /// DataBase操作扩展帮助
    /// </summary>
    internal class DBExtension
    {
        #region 分页排序
        /// <summary>
        /// 分页帮助
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tmpData"></param>
        /// <param name="sort"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        internal static IQueryable<T> PaginationSort<T>(IQueryable<T> tmpData, string sort, bool isAsc) where T : class
        {
            string[] sortArr = sort.Split(',');

            MethodCallExpression resultExpression = null;

            for (int index = 0; index < sortArr.Length; index++)
            {
                string[] sortColAndRuleArr = sortArr[index].Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string sortField = sortColAndRuleArr.First();
                bool sortAsc = isAsc;

                //排序列带上规则   "Id Asc"
                if (sortColAndRuleArr.Length == 2)
                {
                    sortAsc = string.Equals(sortColAndRuleArr[1], "asc", StringComparison.OrdinalIgnoreCase);
                }

                var parameter = Expression.Parameter(typeof(T), "type");
                var property = typeof(T).GetProperties().First(p => p.Name.Equals(sortField));
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);

                if (index == 0)
                {
                    resultExpression = Expression.Call(
                        typeof(Queryable), //调用的类型
                        sortAsc ? "OrderBy" : "OrderByDescending", //方法名称
                        new[] { typeof(T), property.PropertyType }, tmpData.Expression, Expression.Quote(orderByExpression));
                }
                else
                {
                    resultExpression = Expression.Call(
                        typeof(Queryable),
                        sortAsc ? "ThenBy" : "ThenByDescending",
                        new[] { typeof(T), property.PropertyType }, tmpData.Expression, Expression.Quote(orderByExpression));
                }

                tmpData = tmpData.Provider.CreateQuery<T>(resultExpression);
            }
            return tmpData;
        }
        #endregion

        #region 递归附加实体

        /// <summary>
        /// 递归附加
        /// </summary>
        /// <param name="dbContext">当前上下文</param>
        /// <param name="entity">实例</param>
        internal static void RecursionAttach(DbContext dbContext, object entity)
        {

            var entityType = FindTrackingEntity(dbContext, entity);

            if (entityType == null)
                dbContext.Attach(entity);
            else if (entityType.State == EntityState.Modified || entityType.State == EntityState.Added)
                return;

            foreach (var prop in entity.GetType().GetProperties().Where(x => !x.IsDefined(typeof(NotMappedAttribute), false)))
            {
                if (prop.Name.Equals(entity.GetType().Name + "Id", StringComparison.OrdinalIgnoreCase)) continue;

                object obj = prop.GetValue(entity);
                if (obj == null) continue;

                var subEntityType = FindTrackingEntity(dbContext, obj);

                //List<Entity>
                if (prop.PropertyType.IsGenericType && prop.PropertyType.IsClass)
                {
                    IEnumerable<object> objs = (IEnumerable<object>)obj;
                    foreach (var item in objs)
                    {
                        RecursionAttach(dbContext, item);
                    }
                }
                //string/int
                else if (subEntityType == null)
                {
                    dbContext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                //Entity
                else if (subEntityType != null && subEntityType.State == EntityState.Unchanged)
                {
                    RecursionAttach(dbContext, obj);
                }
            }
        }

        /// <summary>
        /// 根据ID匹配是否存在
        /// </summary>
        /// <param name="dbContext">当前上下文</param>
        /// <param name="entity">实例</param>
        /// <returns></returns>
        private static EntityEntry FindTrackingEntity(DbContext dbContext, object entity)
        {
            foreach (var item in dbContext.ChangeTracker.Entries())
            {
                if (item.State == EntityState.Added)
                {
                    if (item.Entity == entity)
                    {
                        return item;
                    }
                }
                string key = entity.GetType().Name + "Id";

                var tryObj = item.Properties.FirstOrDefault(x => x.Metadata.PropertyInfo.Name.Equals(key))?.CurrentValue;
                if (tryObj != null)
                {
                    int tracking = (int)tryObj;
                    int now = (int)entity.GetType().GetProperty(key)?.GetValue(entity);

                    if (tracking == now)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        #endregion


        internal static string GetSql<TEntity>(IQueryable<TEntity> query)
        {
            var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var relationalCommandCache = DBExtension.Private(enumerator, "_relationalCommandCache");
            var selectExpression = DBExtension.Private<SelectExpression>(relationalCommandCache, "_selectExpression");
            var factory = DBExtension.Private<IQuerySqlGeneratorFactory>(relationalCommandCache, "_querySqlGeneratorFactory");

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);

            string sql = command.CommandText;
            return sql;
        }

        /// <summary>
        /// 将IDataReader转换为集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<T> IDataReaderToList<T>(IDataReader reader) where T : new()
        {
            using (reader)
            {
                List<string> field = new List<string>(reader.FieldCount);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    field.Add(reader.GetName(i));
                }
                List<T> list = new List<T>();
                var props = GetProperties(typeof(T));

                while (reader.Read())
                {
                    T t = new T();

                    foreach (PropertyInfo property in props)
                    {
                        if (field.Any(x => x.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (!IsNullOrDBNull(reader[property.Name]))
                            {
                                property.SetValue(t, HackType(reader[property.Name], property.PropertyType), null);
                            }
                        }
                    }
                    list.Add(t);
                }
                return list;
            }
        }

        private static readonly ConcurrentDictionary<string, List<PropertyInfo>> dictCache = new ConcurrentDictionary<string, List<PropertyInfo>>();

        #region 得到类里面的属性集合

        /// <summary>
        /// 得到类里面的属性集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetProperties(Type type)
        {
            List<PropertyInfo> properties = null;
            if (dictCache.ContainsKey(type.FullName))
            {
                properties = dictCache[type.FullName];
            }
            else
            {
                properties = type.GetProperties().ToList();
                dictCache.TryAdd(type.FullName, properties);
            }
            return properties;
        }
        #endregion

        public static bool IsNullOrDBNull(object obj)
        {
            return ((obj is DBNull) || string.IsNullOrEmpty(obj.ToString()));
        }


        //这个类对可空类型进行判断转换，要不然会报错
        public static object HackType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            if (conversionType.IsEnum)
            {
                return Enum.Parse(conversionType, value.ToString());
            }
            return Convert.ChangeType(value, conversionType);
        }


        #region 私有方法
        private static object Private(object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        private static T Private<T>(object obj, string privateField) => (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        #endregion
    }
}
