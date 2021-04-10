using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using WhiteBoard.EF.Abstractions;
using WhiteBoard.EF.DataBase;
using WhiteBoard.EF.DataBase.Extension;

namespace WhiteBoard.EF.DataBase
{
    /// <summary>
    /// 数据库操作实现抽象基类
    /// </summary>
    public abstract class BaseDatabaseImp : IDataBaseOperation
    {
        public DBExtension DBExtension { get; set; }
        public ILogger<BaseDatabaseImp> Logger { get; set; }

        public BaseDatabaseImp(MyDbContext myDbContext)
        {
            this.DbContext = myDbContext;
        }

        #region 数据库级别操作

        public async Task Build()
        {
            await this.DbContext.Database.EnsureDeletedAsync();
            await this.DbContext.Database.EnsureCreatedAsync();
        }

        public IDataBaseOperation GetIDataBaseOperation()
        {
            return this;
        }
        public DbContext DbContext { get; }
        public IDbContextTransaction DbContextTransaction { get; private set; }


        public virtual async Task<IDataBaseOperation> BeginTransAsync()
        {
            var connection = DbContext.Database.GetDbConnection();
            if (connection.State == ConnectionState.Closed) await connection.OpenAsync();
            DbContextTransaction = await DbContext.Database.BeginTransactionAsync();
            return this;
        }

        public virtual async Task<int> CommitTransAsync()
        {
            try
            {
                var lis = DbContext.ChangeTracker.Entries();

                if (lis.Count() > 0)
                {
                    foreach (var item in lis)
                    {
                        if (item.State == EntityState.Added)
                        {
                            item.Property("CreateTime").CurrentValue = DateTime.Now;
                        }
                        else if (item.State == EntityState.Modified)
                        {
                            //Update的情况,创建时间不能修改
                            item.Property("CreateTime").IsModified = false;
                            item.Property("ModifyTime").CurrentValue = DateTime.Now;
                        }
                    }

                    int commitResult = await DbContext.SaveChangesAsync();
                    if (DbContextTransaction != null) await DbContextTransaction.CommitAsync();
                    await this.CloseAsync();
                }

                return 0;
            }
            catch
            {
                //异常抛出,如果存在事务,那么此时DbContext尚未释放
                throw;
            }
            finally
            {
                if (DbContextTransaction == null)
                {
                    await this.CloseAsync();
                }
            }
        }
        public virtual async Task RollbackTransAsync()
        {
            if (DbContextTransaction != null)
            {
                await DbContextTransaction.RollbackAsync();
                await DbContextTransaction.DisposeAsync();
                await this.CloseAsync();
            }
        }

#pragma warning disable CS1998 
        public virtual async Task<IDataBaseOperation> CreateSavepointAsync(string name)
        {
            throw new NotImplementedException("dotnet core 3.1 没有实现该功能");

            //IDataBaseOperation operation = DbContextTransaction == null
            //                        ? await BeginTransAsync()
            //                        : this;

            //await DbContextTransaction.CreateSavepointAsync(name);

            //return operation;
        }

        public virtual async Task RollbackToSavepointAsync(string name)
        {
            throw new NotImplementedException("dotnet core 3.1 没有实现该功能");

            //if (DbContextTransaction != null)
            //{
            //    await DbContextTransaction.RollbackToSavepointAsync(name);
            //}
        }
#pragma warning restore CS1998 

        public virtual async Task CloseAsync()
        {
            await DbContext.DisposeAsync();
        }

        #endregion

        #region 转换AsNoTracking/AsQueryable
        public IDataBaseOperation AsNoTracking()
        {
            this.DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return this;
        }
        public IDataBaseOperation AsTracking()
        {
            this.DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            return this;
        }
        public virtual IQueryable<T> AsNoTracking<T>() where T : class, new()
        {
            return this.DbContext.Set<T>().AsNoTracking();
        }

        public virtual IQueryable<T> AsQueryable<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return predicate == null
                            ? this.AsQueryable<T>()
                            : this.AsQueryable<T>().Where(predicate);
        }

        public virtual IQueryable<T> AsQueryable<T>() where T : class, new()
        {
            return this.DbContext.Set<T>().AsQueryable();
        }
        #endregion

        #region 查找主键
        public virtual IDictionary<string, object> FindPrimaryKeyValue<T>(T t) where T : class, new()
        {
            var entityType = DbContext.Model.FindEntityType(typeof(T));
            IKey key = entityType.FindPrimaryKey();
            var dic = new Dictionary<string, object>(key.Properties.Count);
            foreach (var item in key.Properties)
            {
                var propInfo = item.PropertyInfo;
                dic.Add(propInfo.Name, propInfo.GetValue(t));
            }
            return dic;
        }

        public virtual (string key, int value) FindFirstPrimaryKeyValue<T>(T t) where T : class, new()
        {
            var entityType = DbContext.Model.FindEntityType(typeof(T));
            IKey key = entityType.FindPrimaryKey();
            var propInfo = key.Properties.FirstOrDefault().PropertyInfo;
            var res = (propInfo.Name, Convert.ToInt32(propInfo.GetValue(t)));
            return res;
        }

        public virtual (string table, IEnumerable<string> keys) FindPrimaryKeyWithTable<T>() where T : class, new()
        {
            var entityType = this.DbContext.Model.FindEntityType(typeof(T));
            string tableName = entityType.GetTableName();
            IKey key = entityType.FindPrimaryKey();
            return (tableName, key.Properties.Select(x => x.PropertyInfo.Name));
        }

        public virtual IEnumerable<string> FindPrimaryKey<T>() where T : class, new()
        {
            var entityType = this.DbContext.Model.FindEntityType(typeof(T));
            IKey key = entityType.FindPrimaryKey();
            return key.Properties.Select(x => x.PropertyInfo.Name);
        }
        #endregion

        #region Sql操作
        public virtual async Task<int> RunSqlAsync(string strSql, IDictionary<string, object> keyValues = null)
        {
            if (keyValues != null)
                await DbContext.Database.ExecuteSqlRawAsync(strSql, new DbParameterBuilder(this.DbContext).AddParams(keyValues).GetParams());
            else
                await DbContext.Database.ExecuteSqlRawAsync(strSql);

            return await GetOperationReuslt();
        }
        public virtual async Task<int> RunSqlInterAsync(FormattableString strSql)
        {
            await DbContext.Database.ExecuteSqlInterpolatedAsync(strSql);
            return await GetOperationReuslt();
        }

#pragma warning disable CS1998
        public virtual async Task<int> ExecProcAsync(string procName, IDictionary<string, object> keyValues = null)
        {
            throw new NotImplementedException("请在派生类中实现");
        }
#pragma warning restore CS1998


        public virtual async Task<DataTable> GetTableAsync(string strSql, IDictionary<string, object> keyValues = null)
        {
            DataTable dt = await new DBHelper(this.DbContext).GetDataTable(strSql, keyValues);
            return dt;
        }
        public virtual async Task<IDataReader> GetReaderAsync(string strSql, IDictionary<string, object> keyValues = null)
        {
            IDataReader reader = await new DBHelper(this.DbContext).GetDataReader(strSql, keyValues);
            return reader;
        }
        public virtual async Task<object> GetScalarAsync(string strSql, IDictionary<string, object> keyValues = null)
        {
            object obj = await new DBHelper(this.DbContext).GetScalar(strSql, keyValues);
            return obj;
        }

        #endregion

        #region 删除

        public virtual async Task<int> DeleteAsync<T>(T entity) where T : class, new()
        {
            DbContext.Set<T>().Remove(entity);
            return await GetOperationReuslt();
        }
        public virtual async Task<int> DeleteAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            DbContext.Set<T>().RemoveRange(entities);
            return await GetOperationReuslt();
        }
        public virtual async Task<int> DeleteAsync<T>(int keyValue) where T : class, new()
        {
            T t = new T();
            (string key, _) = FindFirstPrimaryKeyValue(t);
            t.GetType().GetProperty(key).SetValue(t, keyValue);

            return await this.DeleteAsync(t);
        }
        public virtual async Task<int> DeleteAsync<T>(IEnumerable<int> keyValues) where T : class, new()
        {
            foreach (var item in keyValues)
            {
                T t = new T();
                (string key, _) = FindFirstPrimaryKeyValue(t);
                t.GetType().GetProperty(key).SetValue(t, item);

                await this.DeleteAsync(t);
            }
            return await GetOperationReuslt();
        }


        public virtual async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            IEnumerable<T> ts = await this.FindListAsync(predicate);
            if (ts != null && ts.Count() > 0)
            {
                await DeleteAsync(ts);
            }
            return await this.GetOperationReuslt();
        }

        #endregion

        #region 分页查询
        public virtual async Task<(int total, IEnumerable<T> list)> FindListAsync<T>(string sortColumn, bool isAsc, int pageSize, int pageIndex) where T : class, new()
        {
            var tempData = this.AsQueryable<T>();
            var res = await this.FindListAsync(tempData, string.Empty, sortColumn, isAsc, pageSize, pageIndex);
            return res;
        }
        public virtual async Task<(int total, IEnumerable<T> list)> FindListAsync<T>(Expression<Func<T, bool>> condition, string sortColumn,
                                                                                     bool isAsc, int pageSize, int pageIndex) where T : class, new()
        {
            string whereString = string.Empty;
            if (condition != null)
            {
                ConditionBuilderVisitor whereCondition = new ConditionBuilderVisitor("MySql");
                whereCondition.Visit(condition);
                whereString = whereCondition.CombineWithWhere();
            }

            var tempData = this.AsQueryable<T>();
            var res = await this.FindListAsync(tempData, whereString, sortColumn, isAsc, pageSize, pageIndex);
            return res;
        }
        private async Task<(int total, IEnumerable<T> list)> FindListAsync<T>(IQueryable<T> tmpdata, string where, string sortColumn, bool isAsc,
                                                                              int pageSize, int pageIndex) where T : class, new()
        {
            if (string.IsNullOrEmpty(sortColumn))
            {
                var keys = FindPrimaryKey<T>();
                sortColumn = keys.FirstOrDefault();
            }

            string sql = DBExtension.GetSql(tmpdata);
            if (!string.IsNullOrEmpty(where))
            {
                sql += "  " + where;
            }
            var (total, reader) = await FindReaderAsync(sql, null, sortColumn, isAsc, pageSize, pageIndex);
            var lis = DBExtension.IDataReaderToList<T>(reader);
            //tmpdata = DBExtension.PaginationSort(tmpdata, sortColumn, isAsc);
            //var list = await tmpdata.ToListAsync();

            if (lis?.Count > 0)
            {
                // var currentData = list.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                return (total, lis);
            }
            else
            {
                return (0, new List<T>());
            }
        }

        public virtual async Task<(int total, IDataReader)> FindReaderAsync(string strSql, IDictionary<string, object> dbParameters, string sort, bool isAsc, int pageSize, int pageIndex)
        {
            #region sqlserver
            //            if (pageIndex == 0) pageIndex = 1;
            //            int numLeft = (pageIndex - 1) * pageSize + 1;
            //            int numRight = (pageIndex) * pageSize;

            //            string OrderBy;

            //            if (!string.IsNullOrEmpty(sort))
            //            {
            //                if (sort.ToUpper().IndexOf("ASC") + sort.ToUpper().IndexOf("DESC") > 0)
            //                    OrderBy = " ORDER BY " + sort;
            //                else
            //                    OrderBy = " ORDER BY " + sort + " " + (isAsc ? "ASC" : "DESC");
            //            }
            //            else
            //            {
            //                OrderBy = "ORDERE BY (SELECT 0)";
            //            }

            //            string sql = $@"
            //SELECT * FROM 
            //(SELECT  ROW_NUMBER () OVER({OrderBy}) AS ROWNUM, * FROM ({strSql})t1 ) T2
            //WHERE T2.ROWNUM BETWEEN  {numLeft} AND {numRight};";
            #endregion

            StringBuilder sb = new StringBuilder();
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            int num = (pageIndex - 1) * pageSize;
            string OrderBy = "";

            if (!string.IsNullOrEmpty(sort))
            {
                if (sort.ToUpper().IndexOf("ASC") + sort.ToUpper().IndexOf("DESC") > 0)
                {
                    OrderBy = " ORDER BY " + sort;
                }
                else
                {
                    OrderBy = " ORDER BY " + sort + " " + (isAsc ? "ASC" : "DESC");
                }
            }
            sb.Append(strSql + OrderBy);
            sb.Append(" LIMIT " + num + "," + pageSize + "");


            object res = await this.GetScalarAsync($"SELECT COUNT(1) FROM ({strSql}) t", dbParameters);
            int total = Convert.ToInt32(res);
            IDataReader reader = await this.GetReaderAsync(sb.ToString(), dbParameters);
            return (total, reader);
        }

        #endregion

        #region 更新
        public virtual async Task<int> UpdateAsync<T>(T entity, bool updateAll = false) where T : class, new()
        {
            if (updateAll)
            {
                this.DbContext.Update(entity);
            }
            else
            {
                DBExtension.RecursionAttach(this.DbContext, entity);
            }

            return await GetOperationReuslt();
        }
        public virtual async Task<int> UpdateAsync<T>(IEnumerable<T> entities, bool updateAll = false) where T : class, new()
        {
            foreach (var entity in entities)
            {
                await UpdateAsync(entity, updateAll);
            }

            return await GetOperationReuslt();
        }
        public virtual async Task<int> UpdateAsync<T>(IEnumerable<Expression<Func<T, bool>>> setPredicates, Expression<Func<T, bool>> wherePredicate) where T : class, new()
        {
            var (table, keys) = FindPrimaryKeyWithTable<T>();
            string tableName = table;

            ConditionBuilderVisitor whereCondition = new ConditionBuilderVisitor("MySql");
            whereCondition.Visit(wherePredicate);
            string whereString = whereCondition.CombineWithWhere();

            string setStrings = string.Join(",", setPredicates.Select(x =>
            {
                ConditionBuilderVisitor setCondition = new ConditionBuilderVisitor("MySql");
                setCondition.Visit(x);
                return setCondition.Combine();

            })).TrimStart('(', ' ').TrimEnd(')', ' ');


            string sql = $@"UPDATE {tableName} SET {setStrings} {whereString}";

            return await this.RunSqlAsync(sql);
        }
        public virtual async Task<int> UpdateAsync<T>(IEnumerable<Expression<Func<T, bool>>> setPredicates, Expression<Func<T, bool>> wherePredicate, IDictionary<string, object> keyValues) where T : class, new()
        {

            var (table, keys) = FindPrimaryKeyWithTable<T>();
            string tableName = table;

            ConditionBuilderVisitor whereCondition = new ConditionBuilderVisitor("MySql");
            whereCondition.Visit(wherePredicate);
            string whereString = whereCondition.CombineWithWhere();

            string setStrings = string.Join(",", setPredicates.Select(x =>
            {
                ConditionBuilderVisitor setCondition = new ConditionBuilderVisitor("MySql");
                setCondition.Visit(x);
                return setCondition.Combine();

            })).TrimStart('(', ' ').TrimEnd(')', ' ');


            string sql = $@"UPDATE {tableName} SET {setStrings} {whereString}";

            return await this.RunSqlAsync(sql, keyValues);
        }


        #endregion

        #region 添加
        public virtual async Task<int> AddAsync<T>(T entity) where T : class, new()
        {
            this.DbContext.Set<T>().Add(entity);
            return await GetOperationReuslt();
        }
        public virtual async Task<int> AddAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            await this.DbContext.Set<T>().AddRangeAsync(entities);
            return await GetOperationReuslt();
        }
        #endregion

        #region 查询

        public virtual async Task<T> FindFirstAsync<T>() where T : class, new()
        {
            var t = await DbContext.Set<T>().FirstOrDefaultAsync();
            return t;
        }

        public virtual async Task<T> FindAsync<T>(params object[] keyValues) where T : class, new()
        {
            var t = await DbContext.Set<T>().FindAsync(keyValues);
            return t;
        }
        public virtual async Task<T> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            var t = await DbContext.Set<T>().FirstOrDefaultAsync(predicate);
            return t;
        }
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, bool>> predicate = null) where T : class, new()
        {

            IEnumerable<T> et = predicate == null ? await DbContext.Set<T>().ToListAsync()
                                                  : await DbContext.Set<T>().Where(predicate).ToListAsync();

            return et;
        }
        public virtual async Task<IEnumerable<T>> FindListByOrderAsync<T>(Expression<Func<T, object>> predicate, bool isAsc) where T : class, new()
        {
            IEnumerable<T> et = isAsc ? await DbContext.Set<T>().OrderBy(predicate).ToListAsync()
                                      : await DbContext.Set<T>().OrderByDescending(predicate).ToListAsync();

            return et;
        }

        #endregion


        /// <summary>
        /// 获取操作结果
        /// </summary>
        /// <returns>返回受影响数 <see cref="int"/></returns>
        protected async Task<int> GetOperationReuslt()
        {
            return DbContextTransaction == null //如果没有事务
                ? await this.CommitTransAsync() //那么立即提交
                : 0;                            //否则返回0;
        }


        private bool _isDispose = false;

        public virtual async ValueTask DisposeAsync()
        {
            if (_isDispose)
            {
                return;
            }
            _isDispose = true;
            await this.CloseAsync();
        }
        public virtual async void Dispose()
        {
            if (_isDispose)
            {
                return;
            }
            _isDispose = true;
            await this.CloseAsync();
        }

        public void LogId()
        {
            var id = this.DbContext.ContextId.InstanceId.ToString();
            Logger.LogInformation(id);
        }
    }
}
