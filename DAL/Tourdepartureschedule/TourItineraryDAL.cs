using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.Tourdepartureschedule;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL.Tourdepartureschedule
{
    public class TourItineraryDAL : GenericService<TourItinerary>
    {
        private static DbWorker _DbWorker;
        public TourItineraryDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<int> InsertTourItinerary(TourItinerary Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[9];

                objParam[0] = new SqlParameter("@TourDepartureId", Model.TourDepartureId);
                objParam[1] = new SqlParameter("@RouteType", Model.RouteType);
                objParam[2] = new SqlParameter("@TransportType", Model.TransportType);
                objParam[3] = new SqlParameter("@StartPoint", Model.StartPoint);
                objParam[4] = new SqlParameter("@EndPoint", Model.EndPoint);
                objParam[5] = new SqlParameter("@TransportProvider", Model.TransportProvider);
                objParam[6] = new SqlParameter("@DepartureDate", Model.DepartureDate);
                objParam[7] = new SqlParameter("@TransportCode", Model.TransportCode);
                objParam[8] = new SqlParameter("@Note", Model.Note);


                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertTourItinerary, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return 0;
        }
        public async Task<int> UpdateTourItinerary(TourItinerary Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[9];

                objParam[0] = new SqlParameter("@Id", Model.Id);
                objParam[1] = new SqlParameter("@RouteType", Model.RouteType);
                objParam[2] = new SqlParameter("@TransportType", Model.TransportType);
                objParam[3] = new SqlParameter("@StartPoint", Model.StartPoint);
                objParam[4] = new SqlParameter("@EndPoint", Model.EndPoint);
                objParam[5] = new SqlParameter("@TransportProvider", Model.TransportProvider);
                objParam[6] = new SqlParameter("@DepartureDate", Model.DepartureDate);
                objParam[7] = new SqlParameter("@TransportCode", Model.TransportCode);
                objParam[8] = new SqlParameter("@Note", Model.Note);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateTourItinerary, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return 0;
        }
        public async Task<List<TourItinerary>> GetTourItineraryByDepartureId(int TourDepartureId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@TourDepartureId", TourDepartureId);

                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetTourItineraryByDepartureId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<TourItinerary>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourItineraryByDepartureId - TourItineraryDAL: " + ex);
            }
            return new List<TourItinerary>();
        }
    }
}
