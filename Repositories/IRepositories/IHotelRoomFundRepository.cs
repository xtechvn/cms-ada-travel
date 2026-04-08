using Entities.Models;
using Entities.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IHotelRoomFundRepository
    {
        Task<List<HotelRoomFundModel>> GetListHotelRoomFund(HotelRoomFundSearchMode searchModel);
        Task<int> InsertHotelRoomFund(HotelRoomFund model);
        Task<int> UpdateHotelRoomFund(HotelRoomFund model);
        Task<int> InsertHotelRoomFundDetail(HotelRoomFundDetail model);
        Task<List<HotelRoomFundDetail>> GetListHotelRoomFundDetail(int hotelRoomFundId);
        Task<int> DeleteHotelRoomFundDetailByFundId(int hotelRoomFundId, int HotelRoomId);
        Task<HotelRoomFundModel> GetById(int id);
        Task<HotelRoomFund> GetByHotelAndSupplier(int hotelId, int supplierId);
    }
}
