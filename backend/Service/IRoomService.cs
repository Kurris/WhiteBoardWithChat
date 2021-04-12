using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhiteBoard.Entity;
using WhiteBoard.Model;

namespace WhiteBoard.Service
{
    public interface IRoomService : IBaseService<Room>
    {
        Task<TData<string>> JoinRoom(int userId, long roomCode,string password);

        Task<TData<string>> CreateRoom(Room room);

        Task<TData<Pagination<RoomDTO>>> GetRoomsWithPagination(Pagination pagination);
    }
}
