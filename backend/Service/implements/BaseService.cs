using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WhiteBoard.EF.Abstractions;
using WhiteBoard.Entity;
using WhiteBoard.Model;

namespace WhiteBoard.Service.implements
{

    /// <summary>
    /// 基础服务
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class BaseService<T> : IBaseService<T> where T : BaseEntity, new()
    {
        public IDataBaseOperation DataBaseOperation { get; set; }

        protected BaseService()
        {

        }

        #region 查询

        public async Task<TData<T>> FindFirstAsync()
        {
            var tdResult = new TData<T>();
            try
            {
                var t = await DataBaseOperation.FindFirstAsync<T>();
                tdResult.Success("查询成功", t);
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }

        public virtual async Task<TData<IEnumerable<T>>> FindListOrderByAsync(Expression<Func<T, object>> predicate, bool isAsc)
        {
            var tdResult = new TData<IEnumerable<T>>();

            try
            {
                var ts = await DataBaseOperation.FindListByOrderAsync(predicate, isAsc);
                tdResult.Success("查询成功", ts);
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }


        public virtual async Task<TData<T>> FindAsync(int id)
        {
            var tdResult = new TData<T>();

            try
            {
                var t = await DataBaseOperation.FindAsync<T>(id);
                tdResult.Success("查询成功", t);
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }
        public virtual async Task<TData<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var tdResult = new TData<T>();

            try
            {
                var t = await DataBaseOperation.FindAsync(predicate);
                if (t == null)
                {
                    tdResult.Fail("查询失败");
                }
                else
                {
                    tdResult.Success("查询成功", t);
                }
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }

        public virtual async Task<TData<IEnumerable<T>>> FindListAsync(Expression<Func<T, bool>> predicate = null)
        {
            var tdResult = new TData<IEnumerable<T>>();
            try
            {
                var ts = await DataBaseOperation.FindListAsync(predicate);
                if (ts == null)
                {
                    tdResult.Fail("查询失败");
                }
                else
                {
                    tdResult.Success("查询成功", ts);
                }
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }

        public virtual async Task<TData<IEnumerable<T>>> FindListWithOrderAsync(Expression<Func<T, bool>> wherePredicate, Expression<Func<T, object>> orderPredicate, bool isAsc)
        {
            var tdResult = new TData<IEnumerable<T>>();

            try
            {
                var queryable = isAsc ? DataBaseOperation.AsQueryable<T>().Where(wherePredicate).OrderBy(orderPredicate)
                                  : DataBaseOperation.AsQueryable<T>().Where(wherePredicate).OrderByDescending(orderPredicate);
                var ts = await queryable.ToListAsync();

                if (ts == null)
                {
                    tdResult.Fail("查询失败");
                }
                else
                {
                    tdResult.Success("查询成功", ts);
                }
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }
        #endregion

        #region 分页查询

        public virtual async Task<TData<Pagination<T>>> FindWithPaginationAsync(Pagination pagination)
        {
            var tdResult = new TData<Pagination<T>>();
            try
            {
                (int total, IEnumerable<T> ts) = await DataBaseOperation.FindListAsync<T>(
                                                      sortColumn: pagination.SortColumn,
                                                      isAsc: pagination.IsASC,
                                                      pageSize: pagination.PageSize,
                                                      pageIndex: pagination.PageIndex
                                                      );

                pagination.Total = total;
                var pages = this.PaginationData(pagination, ts);
                tdResult.Success("查询成功", pages);
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }


        public virtual async Task<TData<Pagination<T>>> FindWithPaginationAsync(Expression<Func<T, bool>> predicate, Pagination pagination)
        {
            var tdResult = new TData<Pagination<T>>();

            try
            {
                (int total, IEnumerable<T> ts) = await DataBaseOperation.FindListAsync(
                                             predicate: predicate,
                                             sortColumn: pagination.SortColumn,
                                             isAsc: pagination.IsASC,
                                             pageSize: pagination.PageSize,
                                             pageIndex: pagination.PageIndex
                                             );

                pagination.Total = total;
                var pages = this.PaginationData(pagination, ts);
                tdResult.Success("查询成功", pages);
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }

        #endregion

        #region 删除
        public virtual async Task<TData<int>> DeleteAsync(int id)
        {
            var tdResult = new TData<int>();
            using (var op = await DataBaseOperation.BeginTransAsync())
            {
                try
                {
                    var t = await op.FindAsync<T>(id);
                    if (t == null)
                    {
                        tdResult.Fail("数据不存在,删除失败");
                    }
                    else
                    {
                        await op.DeleteAsync(t);
                        await op.CommitTransAsync();
                        tdResult.Success("删除成功");
                    }
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }
        public virtual async Task<TData<int>> DeleteAsync(T t)
        {
            var tdResult = new TData<int>();
            using (var op = await DataBaseOperation.BeginTransAsync())
            {
                try
                {
                    (string key, int value) = op.FindFirstPrimaryKeyValue(t);
                    var tdelete = await op.FindAsync<T>(value);
                    if (tdelete == null)
                    {
                        tdResult.Fail("数据不存在,删除失败");
                    }
                    else
                    {
                        await op.DeleteAsync(t);
                        await op.CommitTransAsync();
                        tdResult.Success("删除成功");
                    }
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }
        public virtual async Task<TData<int>> DeleteAsync(IEnumerable<T> ts)
        {
            var tdResult = new TData<int>();
            using (var op = await DataBaseOperation.BeginTransAsync())
            {
                try
                {
                    int count = 0;
                    foreach (var t in ts)
                    {
                        (string key, int value) = op.FindFirstPrimaryKeyValue(t);

                        var tdelete = await op.FindAsync<T>(value);
                        if (tdelete == null)
                        {
                            await op.RollbackTransAsync();
                            return tdResult.Fail("数据不存在,删除失败");
                        }
                        else
                        {
                            await op.DeleteAsync(t);
                            count++;
                        }
                    }
                    await op.CommitTransAsync();
                    tdResult.Success("删除成功", count);
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }
        public virtual async Task<TData<int>> DeleteAsync(IEnumerable<int> ids)
        {
            var tdResult = new TData<int>();
            using (var op = await DataBaseOperation.BeginTransAsync())
            {
                try
                {
                    int count = 0;
                    foreach (var id in ids)
                    {
                        var t = await op.FindAsync<T>(id);
                        if (t == null)
                        {
                            await op.RollbackTransAsync();
                            return tdResult.Fail($"{id} 数据不存在,删除失败");
                        }
                        else
                        {
                            await op.DeleteAsync(t);
                            count++;
                        }
                    }
                    await op.CommitTransAsync();
                    tdResult.Success("删除成功", count);
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }
        public virtual async Task<TData<int>> DeleteAsyncWithNoTrans(int id)
        {
            var tdResult = new TData<int>();
            try
            {
                var t = await DataBaseOperation.FindAsync<T>(id);
                if (t == null)
                {
                    tdResult.Fail("数据不存在,删除失败");
                }
                else
                {
                    await DataBaseOperation.DeleteAsync(t);
                    tdResult.Success("删除成功");
                }
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }
        public virtual async Task<TData<int>> DeleteAsyncWithNoTrans(T t)
        {
            var tdResult = new TData<int>();
            try
            {
                (string key, int value) = DataBaseOperation.FindFirstPrimaryKeyValue(t);
                var tdelete = await DataBaseOperation.FindAsync<T>(value);
                if (tdelete == null)
                {
                    tdResult.Fail("数据不存在,删除失败");
                }
                else
                {
                    await DataBaseOperation.DeleteAsync(t);
                    tdResult.Success("删除成功");
                }
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }

            return tdResult;
        }
        public virtual async Task<TData<int>> DeleteAsyncWithNoTrans(IEnumerable<T> ts)
        {
            var tdResult = new TData<int>();
            try
            {
                foreach (var t in ts)
                {
                    (string key, int value) = DataBaseOperation.FindFirstPrimaryKeyValue(t);
                    var tdelete = await DataBaseOperation.FindAsync<T>(value);
                    if (tdelete == null)
                    {
                        await DataBaseOperation.RollbackTransAsync();
                        return tdResult.Fail("数据不存在,删除失败");
                    }
                    else
                    {
                        await DataBaseOperation.DeleteAsync(t);
                    }
                }
                tdResult.Success("删除成功");
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }

            return tdResult;
        }
        public virtual async Task<TData<int>> DeleteAsyncWithNoTrans(IEnumerable<int> ids)
        {
            var tdResult = new TData<int>();

            try
            {
                int count = 0;
                foreach (var id in ids)
                {
                    var t = await DataBaseOperation.FindAsync<T>(id);
                    if (t == null)
                    {
                        await DataBaseOperation.RollbackTransAsync();
                        return tdResult.Fail($"{id} 数据不存在,删除失败");
                    }
                    else
                    {
                        await DataBaseOperation.DeleteAsync(t);
                        count++;
                    }
                }
                tdResult.Success("删除成功", count);
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }


        #endregion

        #region 保存
        public virtual async Task<TData<int>> SaveAsync(IEnumerable<T> ts)
        {
            var tdResult = new TData<int>();
            using (var op = await DataBaseOperation.BeginTransAsync())
            {
                try
                {
                    if (ts == null || ts.Count() == 0)
                    {
                        tdResult.Fail("保存数据不能为空");
                    }
                    else
                    {
                        foreach (var t in ts)
                        {
                            (string key, int value) = op.FindFirstPrimaryKeyValue(t);
                            if (value == 0)
                            {
                                await op.AddAsync(t);
                            }
                            else
                            {
                                await op.UpdateAsync(t);
                            }
                        }
                        tdResult.Success("保存成功", ts.Count());
                        await op.CommitTransAsync();
                    }
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }
        public virtual async Task<TData<int>> SaveAsync(T t)
        {
            var tdResult = new TData<int>();
            using (var op = await DataBaseOperation.BeginTransAsync())
            {
                try
                {
                    (string key, int value) = op.FindFirstPrimaryKeyValue(t);
                    if (value == 0)
                    {
                        await op.AddAsync(t);
                    }
                    else
                    {
                        await op.UpdateAsync(t);
                    }

                    await op.CommitTransAsync();
                    tdResult.Success("保存成功", 1);
                }
                catch (Exception ex)
                {
                    await op.RollbackTransAsync();
                    tdResult.Error(ex);
                }
            }

            return tdResult;
        }

        public virtual async Task<TData<int>> SaveAsyncWithNoTrans(IEnumerable<T> ts)
        {
            var tdResult = new TData<int>();
            try
            {
                if (ts == null || ts.Count() == 0)
                {
                    tdResult.Fail("保存数据不能为空");
                }
                else
                {
                    foreach (var t in ts)
                    {
                        (string key, int value) = DataBaseOperation.FindFirstPrimaryKeyValue(t);
                        if (value == 0)
                        {
                            await DataBaseOperation.AddAsync(t);
                        }
                        else
                        {
                            await DataBaseOperation.UpdateAsync(t);
                        }
                    }

                    tdResult.Success("保存成功", ts.Count());
                }
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }

            return tdResult;
        }

        public virtual async Task<TData<int>> SaveAsyncWithNoTrans(T t)
        {
            var tdResult = new TData<int>();

            try
            {
                (string key, int value) = DataBaseOperation.FindFirstPrimaryKeyValue(t);
                if (value == 0)
                {
                    await DataBaseOperation.AddAsync(t);
                }
                else
                {
                    await DataBaseOperation.UpdateAsync(t);
                }

                tdResult.Success("保存成功");
            }
            catch (Exception ex)
            {
                tdResult.Error(ex);
            }
            return tdResult;
        }

        #endregion

        #region Dispose
        private bool _isDispose = false;

        public void Dispose()
        {
            if (_isDispose)
            {
                return;
            }
            _isDispose = true;
            this.DataBaseOperation?.CloseAsync();
        }


        public async ValueTask DisposeAsync()
        {
            if (_isDispose)
            {
                return;
            }
            _isDispose = true;
            await this.DataBaseOperation?.CloseAsync();
        }
        #endregion


        private Pagination<T> PaginationData(Pagination pagination, IEnumerable<T> ts)
        {
            Pagination<T> currentPagination = new Pagination<T>();
            if (pagination == null) throw new NullReferenceException(nameof(Pagination));

            currentPagination.IsASC = pagination.IsASC;
            currentPagination.PageIndex = pagination.PageIndex;
            currentPagination.PageSize = pagination.PageSize;
            currentPagination.SortColumn = pagination.SortColumn;
            currentPagination.Total = pagination.Total;
            currentPagination.PageDatas = ts;

            return currentPagination;
        }
    }
}
