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
    public class FlightWarehouseSegmentDAL : GenericService<FlightWarehouseSegment>
    {
        private static DbWorker _DbWorker;
        public FlightWarehouseSegmentDAL(string connection) : base(connection)
        {

            _DbWorker = new DbWorker(connection);
        }
        public async Task<long> Upsert(FlightWarehouseSegment model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[8];

                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@BookingId", model.BookingId);
                objParam[2] = new SqlParameter("@SegmentType", model.SegmentType ?? (object)DBNull.Value);
                objParam[3] = new SqlParameter("@FlightDate", model.FlightDate ?? (object)DBNull.Value);
                objParam[4] = new SqlParameter("@Airline", model.Airline ?? (object)DBNull.Value);
                objParam[5] = new SqlParameter("@FlightCode", model.FlightCode ?? (object)DBNull.Value);
                objParam[6] = new SqlParameter("@PNRCode", model.Pnrcode ?? (object)DBNull.Value);
                objParam[7] = new SqlParameter("@Identity", SqlDbType.BigInt) { Direction = ParameterDirection.Output };


                var result = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpsertFlightWarehouseSegment, objParam);
                if (objParam[7].Value != DBNull.Value)
                {
                    return (long)objParam[7].Value;
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Upsert - FlightWarehouseSegmentDAL: " + ex);
            }
            return -1;
        }

        public async Task<List<FlightWarehouseSegmentModel>> GetByBookingId(long bookingId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@BookingId", bookingId);



                var result = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetDetailFlightWarehouseSegmentByBookingId, objParam);
                if (result != null && result.Rows.Count > 0)
                {
                    var data = result.ToList<FlightWarehouseSegmentModel>();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByBookingId - FlightWarehouseSegmentDAL: " + ex);
            }
            return null;
        }
    }
}
