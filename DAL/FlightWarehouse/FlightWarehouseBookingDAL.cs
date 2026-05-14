using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.FlightWarehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL.FlightWarehouse
{
    public class FlightWarehouseBookingDAL : GenericService<FlightWarehouseBooking>
    {
        private static DbWorker _DbWorker;
        public FlightWarehouseBookingDAL(string connection) : base(connection)
        {

            _DbWorker = new DbWorker(connection);
        }
        public async Task<List<FlightWarehouseBookingViewModel>> GetListFlightWarehouse(GetListFlightWarehouseModel searchModel, int pageIndex, int pageSize)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[7];

                objParam[0] = new SqlParameter("@BookingCode", searchModel.BookingCode ?? (object)DBNull.Value);
                objParam[1] = new SqlParameter("@DeparturePoint", searchModel.DeparturePoint ?? (object)DBNull.Value);
                objParam[2] = new SqlParameter("@ArrivalPoint", searchModel.ArrivalPoint ?? (object)DBNull.Value);
                objParam[3] = new SqlParameter("@Airline", searchModel.Airline ?? (object)DBNull.Value);
                objParam[4] = new SqlParameter("@PageIndex", pageIndex);
                objParam[5] = new SqlParameter("@PageSize", pageSize);
                objParam[6] = new SqlParameter("@Date", searchModel.Date );
               var dt= _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListFlightWarehouse, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<FlightWarehouseBookingViewModel>();
                    return data;
                }
         
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListFlightWarehouse - FlightWarehouseBookingDAL: " + ex);
            }
            return null;
        }

        public async Task<long> Upsert(FlightWarehouseBooking model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[13];

                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@BookingCode", model.BookingCode ?? (object)DBNull.Value);
                objParam[2] = new SqlParameter("@TripType", model.TripType ?? (object)DBNull.Value);
                objParam[3] = new SqlParameter("@DeparturePoint", model.DeparturePoint ?? (object)DBNull.Value);
                objParam[4] = new SqlParameter("@ArrivalPoint", model.ArrivalPoint ?? (object)DBNull.Value);
                objParam[5] = new SqlParameter("@TotalTicket", model.TotalTicket ?? (object)DBNull.Value);
                objParam[6] = new SqlParameter("@IsRefundable", model.IsRefundable ?? (object)DBNull.Value);
                objParam[7] = new SqlParameter("@CarryOnBaggage", model.CarryOnBaggage ?? (object)DBNull.Value);
                objParam[8] = new SqlParameter("@CheckedBaggage", model.CheckedBaggage ?? (object)DBNull.Value);
                objParam[9] = new SqlParameter("@Note", model.Note ?? (object)DBNull.Value);
                objParam[10] = new SqlParameter("@CreatedBy", model.CreatedBy>0? model.CreatedBy : DBNull.Value);
                objParam[11] = new SqlParameter("@UpdatedBy", model.UpdatedBy > 0 ? model.UpdatedBy : DBNull.Value);
                objParam[12] = new SqlParameter("@AgencyTotalTicket", model.AgencyTotalTicket !=null  ? model.AgencyTotalTicket : 0);
               

                var result = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpsertFlightWarehouseBooking, objParam);
               
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Upsert - FlightWarehouseBookingDAL: " + ex);
            }
            return -1;
        }

        public async Task<FlightWarehouseBookingModel> GetById(long id)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@Id", id);



                var result = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetDetailFlightWarehouseBooking, objParam);
                if (result != null && result.Rows.Count > 0)
                {
                    var data = result.ToList<FlightWarehouseBookingModel>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - FlightWarehouseBookingDAL: " + ex);
            }
            return null;
        }
    }
}
