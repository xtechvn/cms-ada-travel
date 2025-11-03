using Aspose.Cells;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Contract;
using Entities.ViewModels.DebtGuarantee;
using Entities.ViewModels.Vinpearl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

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
        public async Task<string> ExportDeposit(SearchDebtGuarantee searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                var data = new List<DebtGuaranteeViewModel>();
                DataTable dt = await debtGuaranteeDAL.GetListDebtGuarantee(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {
                    data = dt.ToList<DebtGuaranteeViewModel>();
                }
                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách đơn hàng công nợ";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 14);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 20);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 30);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);
                    cell.SetColumnWidth(10, 25);
                    cell.SetColumnWidth(11, 25);
                    cell.SetColumnWidth(12, 25);
                    cell.SetColumnWidth(13, 25);



                    // Set header value
                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Mã bảo lãnh");
                    ws.Cells["C1"].PutValue("Mã đơn hàng");
                    ws.Cells["D1"].PutValue("Ngày bắt đầu ");
                    ws.Cells["E1"].PutValue("Ngày  kết thúc");
                    ws.Cells["F1"].PutValue("Tên khách hàng");
                    ws.Cells["G1"].PutValue("Nhãn đơn");
                    ws.Cells["H1"].PutValue("thanh toán");
                    ws.Cells["I1"].PutValue("Giá trị đơn hàng");
                    ws.Cells["J1"].PutValue("Lợi nhuận");
                    ws.Cells["K1"].PutValue("Ngày tạo");
                    ws.Cells["L1"].PutValue("Sale phụ trách");
                    ws.Cells["M1"].PutValue("Trạng thái");
                  
                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, data.Count, 14);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["A2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    foreach (var item in data)
                    {
                   
                        RowIndex++;
                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 1);
                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["B" + RowIndex].PutValue(item.Code);
                        ws.Cells["C" + RowIndex].PutValue(item.OrderNo );
                        ws.Cells["D" + RowIndex].PutValue(item.StartDate.ToString("dd/MM/yyyy"));
                        ws.Cells["E" + RowIndex].PutValue(item.EndDate.ToString("dd/MM/yyyy"));
                        ws.Cells["F" + RowIndex].PutValue(item.ClientName);
                        ws.Cells["G" + RowIndex].PutValue(item.Label);
                        ws.Cells["H" + RowIndex].PutValue(item.Payment);
                        ws.Cells["H" + RowIndex].SetStyle(numberStyle);
                        ws.Cells["I" + RowIndex].PutValue(item.Amount);
                        ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                        ws.Cells["J" + RowIndex].PutValue(item.Profit);
                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                        ws.Cells["K" + RowIndex].PutValue(item.CreatedDate.ToString("dd/MM/yyyy"));
                        //ws.Cells["L" + RowIndex].PutValue(item.Amount.ToString("N0"));
                        ws.Cells["L" + RowIndex].PutValue(item.UserCreateName);
                        ws.Cells["M" + RowIndex].PutValue(item.StatusName);
           

                    }
                 
                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportPaymentRequest - HotelBookingRepositories: " + ex);
            }
            return pathResult;
        }
        public async Task<List<SumTotalGetListDebtGuaranteeModel>> SumTotalGetListDebtGuarantee(SearchDebtGuarantee Searchmodel)
        {
            try
            {
                var data = new List<SumTotalGetListDebtGuaranteeModel>();
                DataTable dt = await debtGuaranteeDAL.SumTotalGetListDebtGuarantee(Searchmodel);
                if (dt != null && dt.Rows.Count > 0)
                {
                     data = dt.ToList<SumTotalGetListDebtGuaranteeModel>();
                    return data;
                }
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SumTotalGetListDebtGuarantee - DebtGuaranteeRepository: " + ex);
                return null;
            }
        }
    }
}
