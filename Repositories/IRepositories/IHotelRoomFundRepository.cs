using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.WebPages;

namespace Repositories.IRepositories
{
    public interface IHotelRoomFundRepository
    {
        Task<List<HotelRoomFundModel>> GetListHotelRoomFund(HotelRoomFundSearchMode searchModel);
        Task<int> InsertHotelRoomFund(HotelRoomFund model);
        Task<int> UpdateHotelRoomFund(HotelRoomFund model);
        Task<int> InsertHotelRoomFundDetail(HotelRoomFundDetail model);
        Task<List<HotelRoomFundDetailModel>> GetListHotelRoomFundDetail(int hotelRoomFundId,DateTime? StartDate = null, DateTime? EndDate = null);
        Task<List<HotelRoomFundDetail>> GetListHotelRoomFundDetail2(int hotelRoomFundId);
        Task<int> DeleteHotelRoomFundDetailByFundId(int hotelRoomFundId, int HotelRoomId);
        Task<HotelRoomFundModel> GetById(int id);
        Task<HotelRoomFund> GetByHotelAndSupplier(int hotelId, int supplierId);
    }
}
