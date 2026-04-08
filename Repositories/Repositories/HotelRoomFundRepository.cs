using DAL;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Repositories.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class HotelRoomFundRepository : IHotelRoomFundRepository
    {
        private readonly HotelRoomFundDAL _hotelRoomFundDAL;
        private readonly HotelRoomFundDetailDAL _hotelRoomFundDetailDAL;

        public HotelRoomFundRepository(IConfiguration configuration)
        {
            _hotelRoomFundDAL = new HotelRoomFundDAL(configuration["DataBaseConfig:SqlServer:Adavigo"]);
            _hotelRoomFundDetailDAL = new HotelRoomFundDetailDAL(configuration["DataBaseConfig:SqlServer:Adavigo"]);
        }

        public async Task<List<HotelRoomFundModel>> GetListHotelRoomFund(HotelRoomFundSearchMode searchModel)
        {
            return await _hotelRoomFundDAL.GetListHotelRoomFund(searchModel);
        }

        public async Task<int> InsertHotelRoomFund(HotelRoomFund model)
        {
            return await _hotelRoomFundDAL.InsertHotelRoomFund(model);
        }

        public async Task<int> UpdateHotelRoomFund(HotelRoomFund model)
        {
            return await _hotelRoomFundDAL.UpdateHotelRoomFund(model);
        }

        public async Task<int> InsertHotelRoomFundDetail(HotelRoomFundDetail model)
        {
          
            return await _hotelRoomFundDetailDAL.InsertHotelRoomFundDetail(model);
        }

        public async Task<List<HotelRoomFundDetail>> GetListHotelRoomFundDetail(int hotelRoomFundId)
        {
            return await _hotelRoomFundDetailDAL.GetListHotelRoomFundDetail(hotelRoomFundId);
        }

        public async Task<int> DeleteHotelRoomFundDetailByFundId(int hotelRoomFundId, int HotelRoomId)
        {
            return await _hotelRoomFundDetailDAL.DeleteHotelRoomFundDetailByHotelRoomFundIdAndHotelRoomId(hotelRoomFundId, HotelRoomId);
        }

        public async Task<HotelRoomFundModel> GetById(int id)
        {
            return await _hotelRoomFundDAL.GetById(id);
        }

        public async Task<HotelRoomFund> GetByHotelAndSupplier(int hotelId, int supplierId)
        {
            return await _hotelRoomFundDAL.GetByHotelAndSupplier(hotelId, supplierId);
        }
    }
}
