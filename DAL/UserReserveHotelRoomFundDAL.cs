using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebPages;
using Telegram.Bot.Types;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class UserReserveHotelRoomFundDAL : GenericService<UserReserveHotelRoomFund> 
    {
        private static DbWorker _DbWorker;
        public UserReserveHotelRoomFundDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<int> UpdateUserReserveHotelRoomFund(UserReserveHotelRoomFund model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[7];

                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@UserId", model.UserId);
                objParam[2] = new SqlParameter("@HotelRoomId", model.HotelRoomId);
                objParam[3] = new SqlParameter("@NumberOfRooms", model.NumberOfRooms);
                objParam[4] = new SqlParameter("@StartDate", model.StartDate);
                objParam[5] = new SqlParameter("@EndDate", model.EndDate);
                objParam[6] = new SqlParameter("@Status", model.Status);


                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateUserReserveHotelRoomFund, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return -1;
        } 
        public async Task<int> InsertUserReserveHotelRoomFund(UserReserveHotelRoomFund model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[8];

                objParam[0] = new SqlParameter("@UserId", model.UserId);
                objParam[1] = new SqlParameter("@HotelRoomId", model.HotelRoomId);
                objParam[2] = new SqlParameter("@NumberOfRooms", model.NumberOfRooms);
                objParam[3] = new SqlParameter("@StartDate", model.StartDate);
                objParam[4] = new SqlParameter("@EndDate", model.EndDate);
                objParam[5] = new SqlParameter("@Status", model.Status);
                objParam[6] = new SqlParameter("@HotelId", model.HotelId);
                objParam[7] = new SqlParameter("@SupplierId", model.SupplierId);


                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertUserReserveHotelRoomFund, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertUserReserveHotelRoomFund - UserReserveHotelRoomFundDAL: " + ex);
            }
            return -1;
        } 
        public async Task<List<UserReserveHotelRoomFundViewModel>> GetListUserReserveHotelRoomFund(UserReserveHotelRoomFundSearchModel searchModel)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[7];

                objParam[0] = new SqlParameter("@UserId", searchModel.UserId);
                objParam[1] = new SqlParameter("@HotelRoomId", searchModel.HotelRoomId);
                objParam[2] = new SqlParameter("@HotelId", searchModel.HotelId);
                objParam[3] = new SqlParameter("@SupplierId", searchModel.SupplierId);
                objParam[4] = new SqlParameter("@Status", searchModel.Status);
                objParam[5] = new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[6] = new SqlParameter("@PageSize", searchModel.PageSize);


                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListUserReserveHotelRoomFund, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<UserReserveHotelRoomFundViewModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListUserReserveHotelRoomFund - UserReserveHotelRoomFundDAL: " + ex);
            }
            return null;
        }
        public async Task<UserReserveHotelRoomFund> GetById(int id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@id", id);
              
                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListUserReserveHotelRoomFund, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<UserReserveHotelRoomFund>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - UserReserveHotelRoomFundDAL: " + ex);
            }
            return null;
        }
    }
}
