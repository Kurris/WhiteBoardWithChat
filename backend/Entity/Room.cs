﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WhiteBoard.Entity
{
    /// <summary>
    /// 房间号
    /// </summary>
    [Table("Rooms")]
    public class Room : BaseEntity
    {
        /// <summary>
        /// 房间号
        /// </summary>
        public long RoomCode { get; set; }

        /// <summary>
        /// 房间名称
        /// </summary>
        [Required(ErrorMessage = "房间名称不能为空")]
        public string RoomName { get; set; }

        /// <summary>
        /// 拥有者
        /// </summary>
        [Required(ErrorMessage = "主持人不能为空")]
        public int Owner { get; set; }

        /// <summary>
        /// 是否需要密码
        /// </summary>
        public bool IsNeedPassword { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}
