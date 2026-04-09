using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.CustomerManager;
using Microsoft.Data.SqlClient;
using Nest;
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

    public class HotelRoomFundDAL : GenericService<HotelRoomFund>
    {
        private static DbWorker _DbWorker;
        public HotelRoomFundDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<int> InsertHotelRoomFund(HotelRoomFund model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[6];

                objParam[0] = new SqlParameter("@HotelId", model.HotelId);
                objParam[1] = new SqlParameter("@SupplierId", model.SupplierId);  
                objParam[2] = new SqlParameter("@TotalRooms", model.TotalRooms);
                objParam[3] = new SqlParameter("@CreateBy", model.CreateBy);
                objParam[4] = new SqlParameter("@CreateDate", model.CreateDate);
                objParam[5] = new SqlParameter("@UpdateDate", model.UpdateDate);


                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertHotelRoomFund, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return -1;
        }
        public async Task<int> UpdateHotelRoomFund(HotelRoomFund Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[5];

                objParam[0] = new SqlParameter("@Id", Model.Id);
                objParam[1] = new SqlParameter("@HotelId", Model.HotelId);
                objParam[2] = new SqlParameter("@SupplierId", Model.SupplierId);
                objParam[3] = new SqlParameter("@TotalRooms", Model.TotalRooms);
                objParam[4] = new SqlParameter("@UpdateBy", Model.UpdateBy);      

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateHotelRoomFund, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return -1;
        }
        public async Task<List<HotelRoomFundModel>> GetListHotelRoomFund(HotelRoomFundSearchMode searchModel)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[4];

              
                objParam[0] = new SqlParameter("@HotelId", searchModel.HotelId);
                objParam[1] = new SqlParameter("@SupplierId", searchModel.SupplierId);
                objParam[2] = new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[3] = new SqlParameter("@PageSize", searchModel.PageSize);
  
              
                var dt= _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListHotelRoomFund, objParam);
                if(dt!=null && dt.Rows.Count > 0)
                {
                  var data= dt.ToList<HotelRoomFundModel>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return null;
        }

        public async Task<HotelRoomFundModel> GetById(int id)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];


                objParam[0] = new SqlParameter("@Id", id);
              
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetDetailHotelRoomFund, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<HotelRoomFundModel>().FirstOrDefault();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return null;
        }

        public async Task<HotelRoomFund> GetByHotelAndSupplier(int hotelId, int supplierId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@HotelId", hotelId);
                objParam[1] = new SqlParameter("@SupplierId", supplierId);
                objParam[2] = new SqlParameter("@PageIndex", 1);
                objParam[3] = new SqlParameter("@PageSize", 1);
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListHotelRoomFund, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<HotelRoomFund>().FirstOrDefault();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByHotelAndSupplier - HotelRoomFundDAL: " + ex);
            }
            return null;
        }
    }
}
