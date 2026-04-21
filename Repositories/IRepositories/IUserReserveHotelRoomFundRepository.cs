using Entities.Models;
using Entities.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IUserReserveHotelRoomFundRepository
    {
        Task<int> InsertUserReserveHotelRoomFund(UserReserveHotelRoomFund model);
        Task<int> UpdateUserReserveHotelRoomFund(UserReserveHotelRoomFund model);
        Task<List<UserReserveHotelRoomFundViewModel>> GetListUserReserveHotelRoomFund(UserReserveHotelRoomFundSearchModel searchModel);
        Task<UserReserveHotelRoomFundViewModel> GetById(int id);
        Task<List<UserReserveHotelRoomFundViewModel>> GetListByIds(List<int> ids);
        Task<List<HotelRoomFundDetailModel>> GetHotelRoomFundDetailWithAvailability(int hotelId, int supplierId);
    }
}
