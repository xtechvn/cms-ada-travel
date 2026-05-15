using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.FlightWarehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Contants;
using Utilities;
using Microsoft.Data.SqlClient;


namespace DAL.FlightWarehouse
{
    public class FlightWarehouseHoldTicketDAL : GenericService<FlightWarehouseHoldTicket>
    {
        private static DbWorker _DbWorker;
        public FlightWarehouseHoldTicketDAL(string connection) : base(connection)
        {

            _DbWorker = new DbWorker(connection);
        }
        public async Task<List<FlightWarehouseHoldTicketViewModel>> GetListFlightWarehouseHoldTicket(FlightWarehouseHoldTicketSearchModel searchModel)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[5];

                objParam[0] = new SqlParameter("@FlightWarehouseBookingId", searchModel.FlightWarehouseBookingId ?? (object)DBNull.Value);
                objParam[1] = new SqlParameter("@GroupObject", searchModel.GroupObject ?? (object)DBNull.Value);
                objParam[2] = new SqlParameter("@Status", searchModel.Status ?? (object)DBNull.Value);
                objParam[3] = new SqlParameter("@PageIndex", searchModel.pageIndex);
                objParam[4] = new SqlParameter("@PageSize", searchModel.pageSize);
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListFlightWarehouseHoldTicketByBookingId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<FlightWarehouseHoldTicketViewModel>();
                    return data;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListFlightWarehouseHoldTicket - FlightWarehouseHoldTicketDAL: " + ex);
            }
            return null;
        }
              public async Task<int> InsertFlightWarehouseHoldTicket(FlightWarehouseHoldTicket Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[16];

                objParam[0] = new SqlParameter("@FlightWarehouseBookingId", Model.FlightWarehouseBookingId);
                objParam[1] = new SqlParameter("@GroupObject", Model.GroupObject);
                objParam[2] = new SqlParameter("@Status", Model.Status);
                objParam[3] = new SqlParameter("@AdultQuantity", Model.AdultQuantity);
                objParam[4] = new SqlParameter("@ChildQuantity", Model.ChildQuantity);
                objParam[5] = new SqlParameter("@InfantQuantity", Model.InfantQuantity);
                objParam[6] = new SqlParameter("@AdultAmount", Model.AdultAmount);
                objParam[7] = new SqlParameter("@AdultPrice", Model.AdultPrice);
                objParam[8] = new SqlParameter("@ChildAmount", Model.ChildAmount);
                objParam[9] = new SqlParameter("@ChildPrice", Model.ChildPrice);
                objParam[10] = new SqlParameter("@InfantAmount", Model.InfantAmount);
                objParam[11] = new SqlParameter("@InfantPrice", Model.InfantPrice);
                objParam[12] = new SqlParameter("@CreatedBy", Model.CreatedBy);
                objParam[13] = new SqlParameter("@CreatedDate", DBNull.Value );
                objParam[14] = new SqlParameter("@TotalTicket", Model.TotalTicket);
                objParam[15] = new SqlParameter("@ExpirationDate", Model.ExpirationDate);
     
          
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertFlightWarehouseHoldTicket, objParam);
              

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertFlightWarehouseHoldTicket - FlightWarehouseHoldTicketDAL: " + ex);
            }
            return -1;
        }

        public async Task<List<FlightWarehouseHoldTicketViewModel>> GetList(FlightWarehouseHoldTicketSearchModel searchModel)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[8];

                objParam[0] = new SqlParameter("@CreatedBy", searchModel.CreatedBy ?? (object)DBNull.Value);
                objParam[1] = new SqlParameter("@GroupObject", searchModel.GroupObject ?? (object)DBNull.Value);
                objParam[2] = new SqlParameter("@Status", searchModel.Status ?? (object)DBNull.Value);
                objParam[3] = new SqlParameter("@PageIndex", searchModel.pageIndex);
                objParam[4] = new SqlParameter("@PageSize", searchModel.pageSize);
                objParam[5] = new SqlParameter("@DeparturePoint", searchModel.DeparturePoint);
                objParam[6] = new SqlParameter("@ArrivalPoint", searchModel.ArrivalPoint);
                objParam[7] = new SqlParameter("@Date", searchModel.Date);
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListFlightWarehouseHoldTicket, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<FlightWarehouseHoldTicketViewModel>();
                    return data;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - FlightWarehouseHoldTicketDAL: " + ex);
            }
            return null;
        }  
        public async Task<FlightWarehouseHoldTicket> GetById(long id)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@Id", id);
              
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailFlightWarehouseHoldTicketById, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<FlightWarehouseHoldTicket>();
                    return data[0];
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - FlightWarehouseHoldTicketDAL: " + ex);
            }
            return null;
        } 
        public async Task<int> UpdateFlightWarehouseHoldTicket(FlightWarehouseHoldTicket model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[14];

                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@GroupObject", model.GroupObject);
                objParam[2] = new SqlParameter("@Status", model.Status);
                objParam[3] = new SqlParameter("@AdultQuantity", model.AdultQuantity);
                objParam[4] = new SqlParameter("@ChildQuantity", model.ChildQuantity);
                objParam[5] = new SqlParameter("@InfantQuantity", model.InfantQuantity);
                objParam[6] = new SqlParameter("@AdultAmount", model.AdultAmount);
                objParam[7] = new SqlParameter("@AdultPrice", model.AdultPrice);
                objParam[8] = new SqlParameter("@ChildAmount", model.ChildAmount);
                objParam[9] = new SqlParameter("@ChildPrice", model.ChildPrice);
                objParam[10] = new SqlParameter("@InfantAmount", model.InfantAmount);
                objParam[11] = new SqlParameter("@InfantPrice", model.InfantPrice);
                objParam[12] = new SqlParameter("@OrderId", model.OrderId);
                objParam[13] = new SqlParameter("@OrderNo", model.OrderNo);
              
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateFlightWarehouseHoldTicket, objParam);
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - FlightWarehouseHoldTicketDAL: " + ex);
            }
            return -1;
        }   
        public async Task<int> UpdateFlightWarehouseHoldTicketStatus(long id,int Status)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[2];

                objParam[0] = new SqlParameter("@Id", id);
                objParam[1] = new SqlParameter("@Status", Status);
              
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateFlightWarehouseHoldTicketStatus, objParam);
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - FlightWarehouseHoldTicketDAL: " + ex);
            }
            return -1;
        }
    }
}
