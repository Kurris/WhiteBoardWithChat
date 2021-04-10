using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhiteBoard.Model
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// 每页行数(默认五行)
        /// </summary>
        public int PageSize { get; set; } = 5;

        /// <summary>
        /// 当前页(默认第一页)
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 排序列
        /// <code>
        /// "Id Asc,Name Desc" 或者 "Id,Name Desc"
        /// </code>
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// 排序类型,如果 <see cref="SortColumn"/> 没有指定排序方式,则以此参数为准
        /// </summary>
        public bool IsASC { get; set; } = false;

        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount
        {
            get
            {
                if (Total > 0)
                {
                    return (Total % PageSize) == 0
                                            ? Total / PageSize
                                            : Total / PageSize + 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }

    /// <summary>
    /// 分页参数
    /// </summary>
    /// <typeparam name="T">具体数据类型</typeparam>
    public class Pagination<T> : Pagination
    {
        /// <summary>
        /// 返回的数据
        /// </summary>
        public IEnumerable<T> PageDatas { get; set; }
    }
}
