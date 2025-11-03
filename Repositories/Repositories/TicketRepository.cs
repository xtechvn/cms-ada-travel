using Aspose.Cells;
using DAL;
using DAL.Funding;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.Hotel;
using Entities.ViewModels.SupplierConfig;
using Entities.ViewModels.Tour;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Repositories.IRepositories;
using Repositories.Repositories.BaseRepos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class TicketRepository : BaseRepository, ITicketRepository
    {
        private readonly TicketDAL _dal;

        public TicketRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig,
           IOptions<DomainConfig> domainConfig, IUserRepository userRepository, IConfiguration configuration) : base(context, dataBaseConfig, configuration, userRepository)
        {
            _dal = new TicketDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
           
        }
        public TicketListItemViewModel GetDetail(int id)
        {
            try
            {
                var dt = _dal.GetDetail(id);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var r = dt.Rows[0];
                    return new TicketListItemViewModel
                    {
                        TicketId = Convert.ToInt32(r["TicketId"]),
                        TicketCode = r["TicketCode"]?.ToString(),
                        SupplierName = r["SupplierName"]?.ToString(),

                        Category = r["CategoryId"] != DBNull.Value ? Convert.ToInt32(r["CategoryId"]) : (int?)null,
                        TicketType = r["TicketTypeId"] != DBNull.Value ? Convert.ToInt32(r["TicketTypeId"]) : (int?)null,
                        PlayZone = r["PlayZoneId"] != DBNull.Value ? Convert.ToInt32(r["PlayZoneId"]) : (int?)null,
                        Status = r["Status"] != DBNull.Value ? Convert.ToInt32(r["Status"]) : (int?)null,

                        ImportDate = r["ImportDate"] != DBNull.Value ? Convert.ToDateTime(r["ImportDate"]) : (DateTime?)null,
                        ExpiredDate = r["ExpiredDate"] != DBNull.Value ? Convert.ToDateTime(r["ExpiredDate"]) : (DateTime?)null,
                        SoldDuration = r["SoldDuration"] != DBNull.Value ? Convert.ToDateTime(r["ImportDate"]) : (DateTime?)null,
                        QRCode = r["QRCode"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - TicketRepository: " + ex);
            }
            return null;
        }
        // ======================================
        // 🔹 Lấy vé theo mã code (dùng cho quét QR)
        // ======================================
        public TicketListItemViewModel GetByCode(string code)
        {
            try
            {
                var dt = _dal.GetByCode(code);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var r = dt.Rows[0];
                    return new TicketListItemViewModel
                    {
                        TicketId = Convert.ToInt32(r["TicketId"]),
                        TicketCode = r["TicketCode"]?.ToString(),
                        SupplierName = r["SupplierName"]?.ToString(),
                        SupplierId = r["SupplierId"] != DBNull.Value ? Convert.ToInt32(r["SupplierId"]) : (int?)null,
                        Status = r["Status"] != DBNull.Value ? Convert.ToInt32(r["Status"]) : (int?)null
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByCode - TicketRepository: " + ex);
            }
            return null;
        }

        // ======================================
        // 🔹 Cập nhật trạng thái vé theo mã (dùng khi quét QR)
        // ======================================
        public bool UpdateStatusByCode(string code, int status)
        {
            try
            {
                return _dal.UpdateStatusByCode(code, status);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateStatusByCode - TicketRepository: " + ex);
                return false;
            }
        }


        public TicketListItemViewModel InsertTicket(TicketListItemViewModel model)
        {
            try
            {
                var dt = _dal.InsertTicket(model);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var r = dt.Rows[0];
                    return new TicketListItemViewModel
                    {
                        TicketId = Convert.ToInt32(r["TicketId"]),
                        TicketCode = r["TicketCode"].ToString(),
                        SupplierName = r["SupplierName"].ToString(),
                        Category = r["Category"] as int?,
                        TicketType = r["TicketType"] as int?,
                        PlayZone = r["PlayZone"] as int?,
                        Status = r["Status"] as int?,
                        ImportDate = r["ImportDate"] as DateTime?,
                        ExpiredDate = r["ExpiredDate"] as DateTime?
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertTicket - TicketRepository: " + ex);
            }
            return null;
        }

        public bool UpdateTicket(TicketListItemViewModel model)
        {
            try
            {
                var result = _dal.UpdateTicket(model);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateTicket - TicketRepository: " + ex);
                return false;
            }
        }
        //    public bool UpdateStatusByCode(string code, int status)
        //    {
        //        string sql = @"UPDATE Ticket SET Status = @status, UpdatedDate = GETDATE()
        //               WHERE TicketCode = @code AND Status = 1";
        //        SqlParameter[] prms = {
        //    new SqlParameter("@code", code),
        //    new SqlParameter("@status", status)
        //};
        //        return _db.ExecuteNonQuery(sql, prms) > 0;
        //    }





        public PagedResult<TicketListItemViewModel> GetListBySupplier(TicketListFilter filter)
        {
            try
            {
                var dt = _dal.GetListBySupplier(
                    filter.SupplierId,
                    filter.PageIndex,
                    filter.PageSize,
                    filter.Search,
                    filter.Status,
                    filter.PlayZoneId,
                    filter.CategoryId,
                    filter.TicketTypeId,
                    filter.ExpiredDate,
                    out var total
                );

                var items = dt?.ToList<TicketListItemViewModel>() ?? new List<TicketListItemViewModel>();

                return new PagedResult<TicketListItemViewModel>
                {
                    Items = items,
                    TotalRecords = total,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketRepository.GetListBySupplier: " + ex);
                return new PagedResult<TicketListItemViewModel>();
            }
        }

    }
}
