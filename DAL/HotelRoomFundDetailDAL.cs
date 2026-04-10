using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.CustomerManager;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class HotelRoomFundDetailDAL : GenericService<HotelRoomFundDetail>
    {

        private static DbWorker _DbWorker;
        public HotelRoomFundDetailDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<int> InsertHotelRoomFundDetail(HotelRoomFundDetail Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[5];

                objParam[0] = new SqlParameter("@HotelRoomFundId", Model.HotelRoomFundId);
                objParam[1] = new SqlParameter("@HotelRoomId", Model.HotelRoomId);
                objParam[2] = new SqlParameter("@NumberOfRooms", Model.NumberOfRooms);
                objParam[3] = new SqlParameter("@StartDate", Model.StartDate);
                objParam[4] = new SqlParameter("@EndDate", Model.EndDate);


                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertHotelRoomFundDetail, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return 0;
        }
        public async Task<int> UpdateHotelRoomFundDetail(HotelRoomFundDetail Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[4];

                objParam[0] = new SqlParameter("@Id", Model.Id);
                objParam[0] = new SqlParameter("@HotelRoomFundId", Model.HotelRoomFundId);
                objParam[0] = new SqlParameter("@HotelRoomId", Model.HotelRoomId);
                objParam[0] = new SqlParameter("@NumberOfRooms", Model.NumberOfRooms);
                objParam[0] = new SqlParameter("@StartDate", Model.StartDate);
                objParam[0] = new SqlParameter("@EndDate", Model.EndDate);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateHotelRoomFundDetail, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return 0;
        }
        public async Task<List<HotelRoomFundDetailModel>> GetListHotelRoomFundDetail(int HotelRoomFundId, DateTime? StartDate, DateTime? EndDate)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[3];

                objParam[0] = new SqlParameter("@HotelRoomFundId", HotelRoomFundId);
                objParam[1] = new SqlParameter("@StartDate", StartDate == null ? DBNull.Value : StartDate);
                objParam[2] = new SqlParameter("@EndDate", EndDate == null ? DBNull.Value : EndDate);


                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListHotelRoomFundDetail, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<HotelRoomFundDetailModel>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return null;
        }

        public async Task<int> DeleteHotelRoomFundDetailByHotelRoomFundIdAndHotelRoomId(int hotelRoomFundId, int HotelRoomId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];

                objParam[0] = new SqlParameter("@HotelRoomFundId", hotelRoomFundId);
                objParam[1] = new SqlParameter("@HotelRoomId", HotelRoomId);


                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_DeleteHotelRoomFundDetailByHotelRoomFundIdAndHotelRoomId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteByHotelRoomFundId - HotelRoomFundDetailDAL: " + ex);
                return -1;
            }
        }
        public async Task<List<HotelRoomFundDetail>> GetListHotelRoomFundDetail2(int HotelRoomFundId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@HotelRoomFundId", HotelRoomFundId);


                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListHotelRoomFundDetail, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<HotelRoomFundDetail>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return null;
        }
    }
}
