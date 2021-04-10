using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WhiteBoard.EF.Abstractions;
using WhiteBoard.Entity;
using WhiteBoard.Model;

namespace WhiteBoard.Service
{
    /// <summary>
    /// 基础服务接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IBaseService<T> : IDisposable, IAsyncDisposable where T : BaseEntity
    {
        IDataBaseOperation DataBaseOperation { get; set; }

        #region 查询

        /// <summary>
        /// 查询第一个实体
        /// </summary>
        /// <returns><see cref="TData{T}"/></returns>
        Task<TData<T>> FindFirstAsync();

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="id">实体Id</param>
        /// <returns><see cref="TData{T}"/></returns>
        Task<TData<T>> FindAsync(int id);

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="predicate">表达式条件</param>
        /// <returns><see cref="TData{T}"/></returns>
        Task<TData<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 查询多个实体
        /// </summary>
        /// <param name="predicate">表达式条件</param>
        /// <returns><see cref="TData{Enumerable{int}}"/></returns>
        Task<TData<IEnumerable<T>>> FindListAsync(Expression<Func<T, bool>> predicate = null);

        /// <summary>
        /// 排序查询多个实体
        /// </summary>
        /// <param name="predicate">表达式条件</param>
        /// <param name="isAsc">是否为升序</param>
        /// <returns><see cref="TData{Enumerable{int}}"/></returns>
        Task<TData<IEnumerable<T>>> FindListOrderByAsync(Expression<Func<T, object>> predicate, bool isAsc);

        /// <summary>
        /// 排序+条件查询多个实体
        /// </summary>
        /// <param name="wherePredicate">表达式条件</param>
        /// <param name="orderPredicate">排序字段</param>
        /// <param name="isAsc">是否为升序</param>
        /// <returns></returns>
        Task<TData<IEnumerable<T>>> FindListWithOrderAsync(Expression<Func<T, bool>> wherePredicate, Expression<Func<T, object>> orderPredicate, bool isAsc);
        #endregion

        #region 分页查询

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pagination">分页参数</param>
        /// <returns><see cref="TData{Pagination{T}}"/></returns>
        Task<TData<Pagination<T>>> FindWithPaginationAsync(Pagination pagination);


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="predicate">查询表达式</param>
        /// <param name="pagination">分页参数</param>
        /// <returns><see cref="TData{Pagination{T}}"/></returns>
        Task<TData<Pagination<T>>> FindWithPaginationAsync(Expression<Func<T, bool>> predicate, Pagination pagination);

        #endregion

        #region 保存
        /// <summary>
        /// 保存多个实体
        /// </summary>
        /// <param name="ts">实体集合</param>
        /// <returns><see cref="TData{int}"/></returns>
        Task<TData<int>> SaveAsync(IEnumerable<T> ts);

        /// <summary>
        /// 保存多个实体(无事务)
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        Task<TData<int>> SaveAsyncWithNoTrans(IEnumerable<T> ts);

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="t">实例</param>
        /// <returns>保存结果<see cref="int"/></returns>
        Task<TData<int>> SaveAsync(T t);

        /// <summary>
        /// 保存实体(无事务)
        /// </summary>
        /// <param name="t">实例</param>
        /// <returns>保存结果<see cref="int"/></returns>
        Task<TData<int>> SaveAsyncWithNoTrans(T t);
        #endregion

        #region 删除
        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <param name="id">实体Id</param>
        /// <returns><see cref="TData{int}"/></returns>
        Task<TData<int>> DeleteAsync(int id);

        /// <summary>
        /// 删除一个实体(无事务)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TData<int>> DeleteAsyncWithNoTrans(int id);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="t">实体</param>
        /// <returns><see cref="TData{int}"/></returns>
        Task<TData<int>> DeleteAsync(T t);

        /// <summary>
        /// 删除实体(无事务)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<TData<int>> DeleteAsyncWithNoTrans(T t);

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <param name="ts">实体集合</param>
        /// <returns><see cref="TData{int}"/></returns>
        Task<TData<int>> DeleteAsync(IEnumerable<T> ts);

        /// <summary>
        /// 删除多个实体(无事务)
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        Task<TData<int>> DeleteAsyncWithNoTrans(IEnumerable<T> ts);

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <param name="ids">id集合</param>
        /// <returns><see cref="TData{int}"/></returns>
        Task<TData<int>> DeleteAsync(IEnumerable<int> ids);

        /// <summary>
        /// 删除多个实体(无事务)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<TData<int>> DeleteAsyncWithNoTrans(IEnumerable<int> ids);
        #endregion
    }
}
