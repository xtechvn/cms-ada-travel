using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
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
    public class FlightWarehousePriceDAL : GenericService<FlightWarehousePrice>
    {
        private static DbWorker _DbWorker;
        public FlightWarehousePriceDAL(string connection) : base(connection)
        {

            _DbWorker = new DbWorker(connection);
        }
        public async Task<long> Upsert(FlightWarehousePrice model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[10];

                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@BookingId", model.BookingId);
                objParam[2] = new SqlParameter("@PriceType", model.PriceType ?? (object)DBNull.Value);
                objParam[3] = new SqlParameter("@ApplyAgencyIds", model.ApplyAgencyIds ?? (object)DBNull.Value);
                objParam[4] = new SqlParameter("@AdtPrice", model.AdtPrice ?? (object)DBNull.Value);
                objParam[5] = new SqlParameter("@AdtAmount", model.AdtAmount ?? (object)DBNull.Value);
                objParam[6] = new SqlParameter("@ChdPrice", model.ChdPrice ?? (object)DBNull.Value);
                objParam[7] = new SqlParameter("@ChdAmount", model.ChdAmount ?? (object)DBNull.Value);
                objParam[8] = new SqlParameter("@InfPrice", model.InfPrice ?? (object)DBNull.Value);
                objParam[9] = new SqlParameter("@InfAmount", model.InfAmount ?? (object)DBNull.Value);
               
                var result = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpsertFlightWarehousePrice, objParam);
               
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Upsert - FlightWarehousePriceDAL: " + ex);
            }
            return -1;
        }

        public async Task<List<FlightWarehousePriceModel>> GetByBookingId(long bookingId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@BookingId", bookingId);
               


                var result = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetDetailFlightWarehousePriceByBookingId, objParam);
                if(result!=null && result.Rows.Count > 0)
                {
                    var data = result.ToList<FlightWarehousePriceModel>();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByBookingId - FlightWarehousePriceDAL: " + ex);
            }
            return null;
        }
    }
}
