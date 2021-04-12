using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhiteBoard.Entity;
using WhiteBoard.Model;
using WhiteBoard.Utils;

namespace WhiteBoard.Service.implements
{
    public class RoomService : BaseService<Room>, IRoomService
    {
        public IUserService UserService { get; set; }
        public IOnlineService OnlineService { get; set; }

        public async Task<TData<string>> CreateRoom(Room room)
        {
            await using (var op = await DataBaseOperation.BeginTransAsync())
            {
                TData<string> tdResult = new TData<string>();
                try
                {
                    var tdOnline = await OnlineService.FindAsync(x => x.UserId == room.Owner);
                    if (tdOnline.IsValidationSuccessWithData())
                    {
                        tdResult.Fail("创建失败,当前在房间中");
                    }
                    else
                    {
                        room.RoomCode = IdHelper.Instance.GetLongId();
                        await this.SaveAsyncWithNoTrans(room);
                        await OnlineService.SaveAsyncWithNoTrans(new Online()
                        {
                            Role = Role.Moderator,
                            RoomCode = room.RoomCode,
                            UserId = room.Owner
                        });

                        await op.CommitTransAsync();
                        tdResult.Success("创建成功");
                    }
                }
                catch (Exception ex)
                {
                    tdResult.Error(ex);
                    await op.RollbackTransAsync();
                }
                return tdResult;
            }
        }

        public async Task<TData<Pagination<RoomDTO>>> GetRoomsWithPagination(Pagination pagination)
        {
            await using (DataBaseOperation.AsNoTracking())
            {
                var tdDatas = await this.FindWithPaginationAsync(pagination);
                var tdResult = new TData<Pagination<RoomDTO>>()
                {
                    Message = tdDatas.Message,
                    Status = tdDatas.Status
                };

                if (tdDatas.IsValidationSuccessWithData())
                {
                    var rooms = tdDatas.Data.PageDatas;
                    var users = await UserService.FindListAsync();

                    List<RoomDTO> roomDTOs = new List<RoomDTO>(rooms.Count());
                    foreach (var room in rooms)
                    {
                        roomDTOs.Add(new RoomDTO()
                        {
                            CreateTime = room.CreateTime,
                            Id = room.Id,
                            IsNeedPassword = room.IsNeedPassword,
                            Moderator = users.Data.FirstOrDefault(x => x.Id == room.Owner)?.RealName,
                            Owner = room.Owner,
                            ModifyTime = room.ModifyTime,
                            Password = room.Password,
                            RoomCode = room.RoomCode,
                            RoomName = room.RoomName
                        });
                    }
                    tdResult.Data = new Pagination<RoomDTO>()
                    {
                        PageDatas = roomDTOs,
                        IsASC = pagination.IsASC,
                        PageIndex = pagination.PageIndex,
                        PageSize = pagination.PageSize,
                        SortColumn = pagination.SortColumn,
                        Total = pagination.Total
                    };
                }

                return tdResult;
            }
        }

        public async Task<TData<string>> JoinRoom(int userId, long roomCode, string password)
        {
            await using (var op = await DataBaseOperation.BeginTransAsync())
            {
                TData<string> tdResult = new TData<string>();
                try
                {
                    var tdOnline = await OnlineService.FindAsync(x => x.UserId == userId);
                    if (tdOnline.IsValidationSuccessWithData())
                    {
                        await OnlineService.DeleteAsyncWithNoTrans(tdOnline.Data);
                    }
                    var tdRoom = await this.FindAsync(x => x.RoomCode == roomCode);
                    if (tdRoom.IsValidationSuccessWithData())
                    {
                        if (tdRoom.Data.IsNeedPassword)
                        {
                            if (tdRoom.Data.Password != password)
                            {
                                return tdResult.Fail("密码错误");
                            }
                        }

                        await OnlineService.SaveAsyncWithNoTrans(new Online()
                        {
                            Role = Role.Member,
                            RoomCode = roomCode,
                            UserId = userId
                        });

                        await op.CommitTransAsync();

                    }
                    tdResult.Success("加入成功");
                }
                catch (Exception ex)
                {
                    tdResult.Error(ex);
                    await op.RollbackTransAsync();
                }
                return tdResult;
            }
        }
    }
}
