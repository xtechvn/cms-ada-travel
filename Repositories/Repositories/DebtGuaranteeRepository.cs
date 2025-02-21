using DAL;
using Entities.ConfigModels;
using Entities.ViewModels.Contract;
using Entities.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.ViewModels.DebtGuarantee;
using Utilities;
using Entities.Models;
using Nest;

namespace Repositories.Repositories
{
    public class DebtGuaranteeRepository: IDebtGuaranteeRepository
    {
        private readonly DebtGuaranteeDAL debtGuaranteeDAL;

        public DebtGuaranteeRepository(IOptions<DataBaseConfig> dataBaseConfig, ILogger<AllCodeRepository> logger)
        {
            debtGuaranteeDAL = new DebtGuaranteeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<GenericViewModel<DebtGuaranteeViewModel>> GetListDebtGuarantee(SearchDebtGuarantee Searchmodel)
        {
            try
            {
                var model = new GenericViewModel<DebtGuaranteeViewModel>();
                DataTable dt = await debtGuaranteeDAL.GetListDebtGuarantee(Searchmodel);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<DebtGuaranteeViewModel>();
                              

                    model.ListData = data;
                    model.PageSize = Searchmodel.PageSize;
                    model.CurrentPage = Searchmodel.PageIndex;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    return model;
                }
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListDebtGuarantee - DebtGuaranteeRepository: " + ex);
                return null;
            }
        }
        public async Task<long> UpdateDebtGuarantee(int id, int Status, int CreatedBy)
        {
            try
            {
                var model = new DebtGuarantee();
                model.Id = id;
                model.UpdatedBy = CreatedBy;
                model.Status = Status;
                return await debtGuaranteeDAL.UpdateDebtGuarantee(model);
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdataContactStatus - PolicyDetailDAL. " + ex);
                return 0;
            }
        }
        public async Task<long> InsertDebtGuarantee(DebtGuarantee model)
        {
            try
            {
                return await debtGuaranteeDAL.InsertDebtGuarantee(model);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertDebtGuarantee - PolicyDetailDAL. " + ex);
                return 0;
            }
        }
        public async Task<DebtGuaranteeViewModel> GetDetailDebtGuarantee(int Id)
        {
            try
            {
                DataTable dt = await debtGuaranteeDAL.DetailDebtGuarantee(Id);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<DebtGuaranteeViewModel>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertDebtGuarantee - PolicyDetailDAL. " + ex);
                return null;
            }
        }  
        public async Task<DebtGuaranteeViewModel> DetailDebtGuaranteebyOrderid(int Id)
        {
            try
            {
                DataTable dt = await debtGuaranteeDAL.DetailDebtGuaranteebyOrderid(Id);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<DebtGuaranteeViewModel>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertDebtGuarantee - PolicyDetailDAL. " + ex);
                return null;
            }
        }
    }
}
