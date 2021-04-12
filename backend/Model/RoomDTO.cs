using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhiteBoard.Entity;

namespace WhiteBoard.Model
{
    public class RoomDTO : Room
    {
        /// <summary>
        /// 主持人
        /// </summary>
        public string Moderator { get; set; }
    }
}
