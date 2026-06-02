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
    public class TourPriceDAL : GenericService<TourPrice>
    {
        private static DbWorker _DbWorker;
        public TourPriceDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<int> InsertTourPrice(TourPrice Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[14];

                objParam[0] = new SqlParameter("@TourDepartureId", Model.TourDepartureId);
                objParam[1] = new SqlParameter("@PriceType", Model.PriceType);

                objParam[2] = new SqlParameter("@AdultQuantity", Model.AdultQuantity==null?1: Model.AdultQuantity);
                objParam[3] = new SqlParameter("@ChildQuantity", Model.ChildQuantity == null ? 1 : Model.ChildQuantity);
                objParam[4] = new SqlParameter("@InfantQuantity", Model.InfantQuantity == null ? 1 : Model.InfantQuantity);

                objParam[5] = new SqlParameter("@AdtPrice", Model.AdtPrice);
                objParam[6] = new SqlParameter("@AdtAmount", Model.AdtAmount);
                objParam[7] = new SqlParameter("@AdtProfit", Model.AdtProfit);

                objParam[8] = new SqlParameter("@ChdPrice", Model.ChdPrice);
                objParam[9] = new SqlParameter("@ChdAmount", Model.ChdAmount);
                objParam[10] = new SqlParameter("@ChdProfit", Model.ChdProfit);

                objParam[11] = new SqlParameter("@InfPrice", Model.InfPrice);
                objParam[12] = new SqlParameter("@InfAmount", Model.InfAmount);
                objParam[13] = new SqlParameter("@InfProfit", Model.InfProfit);


                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertTourPrice, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertTourPrice - TourPriceDAL: " + ex);
            }
            return 0;
        }
        public async Task<int> UpdateTourPrice(TourPrice Model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[14];

                objParam[0] = new SqlParameter("@Id", Model.Id);
                objParam[1] = new SqlParameter("@PriceType", Model.PriceType);

                objParam[2] = new SqlParameter("@AdultQuantity", Model.AdultQuantity == null ? 1 : Model.AdultQuantity);
                objParam[3] = new SqlParameter("@ChildQuantity", Model.ChildQuantity == null ? 1 : Model.ChildQuantity);
                objParam[4] = new SqlParameter("@InfantQuantity", Model.InfantQuantity == null ? 1 : Model.InfantQuantity);

                objParam[5] = new SqlParameter("@AdtPrice", Model.AdtPrice);
                objParam[6] = new SqlParameter("@AdtAmount", Model.AdtAmount);
                objParam[7] = new SqlParameter("@AdtProfit", Model.AdtProfit);

                objParam[8] = new SqlParameter("@ChdPrice", Model.ChdPrice);
                objParam[9] = new SqlParameter("@ChdAmount", Model.ChdAmount);
                objParam[10] = new SqlParameter("@ChdProfit", Model.ChdProfit);

                objParam[11] = new SqlParameter("@InfPrice", Model.InfPrice);
                objParam[12] = new SqlParameter("@InfAmount", Model.InfAmount);
                objParam[13] = new SqlParameter("@InfProfit", Model.InfProfit);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateTourPrice, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateTourPrice - TourPriceDAL: " + ex);
            }
            return 0;
        }
        public async Task<List<TourPrice>> GetTourPriceByDepartureId(int TourDepartureId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];

                objParam[0] = new SqlParameter("@TourDepartureId", TourDepartureId);

                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetTourPriceByDepartureId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<TourPrice>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourPriceByDepartureId - TourPriceDAL: " + ex);
            }
            return new List<TourPrice>();
        }
    }
}
