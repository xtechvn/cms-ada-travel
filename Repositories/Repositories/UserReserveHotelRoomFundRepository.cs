using DAL;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class UserReserveHotelRoomFundRepository : IUserReserveHotelRoomFundRepository
    {
        private readonly UserReserveHotelRoomFundDAL _userReserveHotelRoomFundDAL;
        private readonly HotelRoomFundDAL _hotelRoomFundDAL;
        private readonly HotelRoomFundDetailDAL _hotelRoomFundDetailDAL;

        public UserReserveHotelRoomFundRepository(IConfiguration configuration)
        {
            var connectionString = configuration["DataBaseConfig:SqlServer:Adavigo"];
            _userReserveHotelRoomFundDAL = new UserReserveHotelRoomFundDAL(connectionString);
            _hotelRoomFundDAL = new HotelRoomFundDAL(connectionString);
            _hotelRoomFundDetailDAL = new HotelRoomFundDetailDAL(connectionString);
        }

        public async Task<int> InsertUserReserveHotelRoomFund(UserReserveHotelRoomFund model)
        {
            return await _userReserveHotelRoomFundDAL.InsertUserReserveHotelRoomFund(model);
        }

        public async Task<int> UpdateUserReserveHotelRoomFund(UserReserveHotelRoomFund model)
        {
            return await _userReserveHotelRoomFundDAL.UpdateUserReserveHotelRoomFund(model);
        }

        public async Task<List<UserReserveHotelRoomFundViewModel>> GetListUserReserveHotelRoomFund(UserReserveHotelRoomFundSearchModel searchModel)
        {
            return await _userReserveHotelRoomFundDAL.GetListUserReserveHotelRoomFund(searchModel);
        }

        public async Task<UserReserveHotelRoomFundViewModel> GetById(int id)
        {
            return await _userReserveHotelRoomFundDAL.GetById(id);
        }

        public async Task<List<UserReserveHotelRoomFundViewModel>> GetListByIds(List<int> ids)
        {
            return await _userReserveHotelRoomFundDAL.GetListByIds(ids);
        }

        public async Task<List<HotelRoomFundDetailModel>> GetHotelRoomFundDetailWithAvailability(int hotelId, int supplierId)
        {
            // First get the HotelRoomFund ID for this hotel and supplier
            var fund = await _hotelRoomFundDAL.GetByHotelAndSupplier(hotelId, supplierId);
            if (fund == null) return new List<HotelRoomFundDetailModel>();

            // Then get the details which should include availability calculations (done by SP sp_GetListHotelRoomFundDetail)
            var details = await _hotelRoomFundDetailDAL.GetListHotelRoomFundDetail(fund.Id,null,null);
            
            // Filter only those with availability > 0 (as requested: "chỉ hiển thị hạng phòng còn phòng trống")
            if (details != null)
            {
                return details.Where(d => d.NumberOfRoomsAvailable > 0).ToList();
            }
            
            return new List<HotelRoomFundDetailModel>();
        }
    }
}
