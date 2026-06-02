using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.Tourdepartureschedule;
using Microsoft.Data.SqlClient;
using PdfSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL.Tourdepartureschedule
{
    public class TourDepartureDAL : GenericService<TourDeparture>
    {
        private static DbWorker _DbWorker;
        public TourDepartureDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<int> InsertTourDeparture(TourDeparture Model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[13];

                objParam[0] = new SqlParameter("@TourProductId", Model.TourProductId);
                objParam[1] = new SqlParameter("@StartDate", Model.StartDate);
                objParam[2] = new SqlParameter("@EndDate", Model.EndDate);
                objParam[3] = new SqlParameter("@Total", Model.Total ?? (object)DBNull.Value);
                objParam[4] = new SqlParameter("@BookingDeadline", Model.BookingDeadline ?? (object)DBNull.Value);
                objParam[5] = new SqlParameter("@Status", Model.Status ?? (object)DBNull.Value);
                objParam[6] = new SqlParameter("@Note", Model.Note ?? (object)DBNull.Value);
                objParam[7] = new SqlParameter("@IsShowWebsite", Model.IsShowWebsite ?? (object)DBNull.Value);
                objParam[8] = new SqlParameter("@IsShowWaitingList", Model.IsShowWaitingList ?? (object)DBNull.Value);
                objParam[9] = new SqlParameter("@IsAllowReserveOnline", Model.IsAllowReserveOnline ?? (object)DBNull.Value);
                objParam[10] = new SqlParameter("@IsAllowDepositOnline", Model.IsAllowDepositOnline ?? (object)DBNull.Value);
                objParam[11] = new SqlParameter("@IsFeatured", Model.IsFeatured ?? (object)DBNull.Value);
                objParam[12] = new SqlParameter("@CreatedBy", Model.CreatedBy ?? (object)DBNull.Value);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertTourDeparture, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertTourDeparture - TourDepartureDAL: " + ex);
            }
            return 0;
        }
        public async Task<int> UpdateTourDeparture(TourDeparture Model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[14];

                objParam[0] = new SqlParameter("@Id", Model.Id);
                objParam[1] = new SqlParameter("@TourProductId", Model.TourProductId);
                objParam[2] = new SqlParameter("@StartDate", Model.StartDate);
                objParam[3] = new SqlParameter("@EndDate", Model.EndDate);
                objParam[4] = new SqlParameter("@Total", Model.Total ?? (object)DBNull.Value);
                objParam[5] = new SqlParameter("@BookingDeadline", Model.BookingDeadline ?? (object)DBNull.Value);
                objParam[6] = new SqlParameter("@Status", Model.Status ?? (object)DBNull.Value);
                objParam[7] = new SqlParameter("@Note", Model.Note ?? (object)DBNull.Value);
                objParam[8] = new SqlParameter("@IsShowWebsite", Model.IsShowWebsite ?? (object)DBNull.Value);
                objParam[9] = new SqlParameter("@IsShowWaitingList", Model.IsShowWaitingList ?? (object)DBNull.Value);
                objParam[10] = new SqlParameter("@IsAllowReserveOnline", Model.IsAllowReserveOnline ?? (object)DBNull.Value);
                objParam[11] = new SqlParameter("@IsAllowDepositOnline", Model.IsAllowDepositOnline ?? (object)DBNull.Value);
                objParam[12] = new SqlParameter("@IsFeatured", Model.IsFeatured ?? (object)DBNull.Value);
                objParam[13] = new SqlParameter("@UpdatedBy", Model.UpdatedBy ?? (object)DBNull.Value);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateTourDeparture, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateTourDeparture - TourDepartureDAL: " + ex);
            }
            return 0;
        } 
        public async Task<DetailTourDepartureModel> GetTourDepartureDetailById(int Id)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@Id", Id);
               
                var dt= _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetTourDepartureById, objParam);
                if(dt != null && dt.Rows.Count > 0)
                {
                    var data=dt.ToList<DetailTourDepartureModel>();
                    return data[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourDepartureDetailById - TourDepartureDAL: " + ex);
            }
            return null;
        }
        public async Task<List<GetListTourDepartureModel>> GetListTourDeparture(TourDepartureSeachModel model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[6];

                objParam[0] = new SqlParameter("@TourProductId ", model.TourProductId);
                objParam[1] = new SqlParameter("@Status ", model.Status);
                objParam[2] = new SqlParameter("@FromDate ", model.FromDate);
                objParam[3] = new SqlParameter("@ToDate ", model.ToDate);
                objParam[4] = new SqlParameter("@PageIndex  ", model.PageIndex);
                objParam[5] = new SqlParameter("@PageSize   ", model.PageSize);
               
                var dt= _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListTourDeparture, objParam);
                if(dt != null && dt.Rows.Count > 0)
                {
                    var data=dt.ToList<GetListTourDepartureModel>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourDepartureDetailById - TourDepartureDAL: " + ex);
            }
            return null;
        }
    }
}
