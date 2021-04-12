using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WhiteBoard.Entity;
using WhiteBoard.Model;
using WhiteBoard.Service;

namespace WhiteBoard.Core.Controllers
{
    /// <summary>
    /// 房间信息
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        public IRoomService RoomService { get; set; }

        //[HttpPost]
        //public async Task<TData<string>> JoinRoom(JointRoomModel jointRoomModel)
        //{
        //    var res = await RoomService.JoinRoom(jointRoomModel);
        //    return res;
        //}

        //[HttpPost]
        //public async Task<TData<string>> CreateRoom(Room room)
        //{
        //    var res = await RoomService.CreateRoom(room);
        //    return res;
        //}

        [HttpGet]
        public async Task<TData<Pagination<RoomDTO>>> GetRoomsWithPagination([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            await using (RoomService.DataBaseOperation.AsNoTracking())
            {
                Pagination pagination = new Pagination()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    IsASC = false,
                };

                var res = await RoomService.GetRoomsWithPagination(pagination);
                return res;
            }
        }
    }
}
