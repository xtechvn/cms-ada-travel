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
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListFlightWarehouseHoldTicket, objParam);
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

                SqlParameter[] objParam = new SqlParameter[15];

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
     
          
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertFlightWarehouseHoldTicket, objParam);
              

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertFlightWarehouseHoldTicket - FlightWarehouseHoldTicketDAL: " + ex);
            }
            return -1;
        }


    }
}
