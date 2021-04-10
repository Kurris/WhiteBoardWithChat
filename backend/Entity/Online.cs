using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WhiteBoard.Entity
{
    /// <summary>
    /// 在线表
    /// </summary>
    [Table("Onlines")]
    public class Online : BaseEntity
    {
        /// <summary>
        /// 房间号
        /// </summary>
        public long RoomCode { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public Role Role { get; set; }
    }

    /// <summary>
    /// 角色
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// 主持人
        /// </summary>
        [Description("主持人")]
        Moderator = 0,

        /// <summary>
        /// 管理员
        /// </summary>
        [Description("管理员")]
        Admin = 1,

        /// <summary>
        /// 临时使用
        /// </summary>
        [Description("临时使用")]
        TemporaryUser = 2,

        /// <summary>
        /// 成员
        /// </summary>
        [Description("成员")]
        Member = 3
    }
}
