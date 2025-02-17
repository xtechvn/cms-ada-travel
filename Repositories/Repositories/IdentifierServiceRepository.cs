using DAL;
using DAL.Identifier;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Nest;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types.Payments;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class IdentifierServiceRepository : IIdentifierServiceRepository
    {

        private readonly OrderDAL orderDAL;
        private readonly ContractPayDAL contractPayDAL;
        private readonly ContractDAL contractDAL;
        private readonly IdentifierDAL identifierDAL;

        public IdentifierServiceRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            orderDAL = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            contractDAL = new ContractDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            identifierDAL = new IdentifierDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }



        public async Task<string> buildServiceNo(int service_type, int? tenant_id = null)
        {
            string service_name = ServicesTypeCode.service[Convert.ToInt16(service_type)];
            try
            {
                int count = identifierDAL.countServiceUse(service_type,tenant_id);
                //format numb
                string s_format = string.Format(String.Format("{0,4:0000}", count + 1));
                service_name += string.Format("{0:D3}", (tenant_id == null ? 0 : (int)tenant_id));

                service_name += s_format;

                return service_name;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildServiceNo - IndentiferService" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var num_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                service_name = service_name + num_default;
                return service_name;
            }
        }
        public async Task<string> buildOrderManual(int? tenant_id = null)
        {
            string order_no = string.Empty;
            try
            {
                var months = new Dictionary<int, string> { { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "K" }, { 10, "L" }, { 11, "M" }, { 12, "N" } };
                var current_date = DateTime.Now;

                order_no += string.Format("{0:D3}", (tenant_id == null ? 0 : (int)tenant_id));

                //3. 2 số cuối của năm
                order_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //4. Tháng hiện tại là index tham chiếu sang bảng chữ cái lấy chữ
                order_no += months[current_date.Month];

                ////5. Ngày giao dich 
                //order_no += current_date.ToString("dd");

                //6. Số thứ tự đơn hàng trong năm.
                long order_count =  orderDAL.CountOrderInYear(tenant_id);

                ////6.1: Check số đơn hàng này có chưa
                //var order_check =  orderDAL.GetByOrderNo(order_no + order_count);

                //if (order_check != null && order_check.OrderId > 0)
                //{
                //    //Nếu có rồi tăng lên 1
                //    // order_no += (order_count + 1);
                //    order_no += string.Format(String.Format("{0,6:D6}", order_count + 1));

                //}
                //else
                //{
                //    //order_no += order_count.ToString();
                //    order_no += string.Format(String.Format("{0,6:D6}", order_count));


                //}

                //order_no = string.Format(String.Format("{0,4:0000}", order_no));

                order_no += string.Format(String.Format("{0,6:D6}", order_count));
                return order_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildOrderManual - IndentiferService" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var order_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                order_no = "MB-" + order_default;
                return order_no;
            }
        }
        public bool IsOrderManual(string orderNo, int? tenant_id = null)
        {
            if (orderNo.Length >= 3 && char.IsDigit(orderNo[0]) && char.IsDigit(orderNo[1]) && char.IsDigit(orderNo[2]))
            {
                return true;
            }
            return false;

        }
        public async Task<string> BuildPaymentRequest(int? tenant_id = null)
        {
            string bill_no = string.Empty;
            try
            {
                var months = new Dictionary<int, string> { { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "K" }, { 10, "L" }, { 11, "M" }, { 12, "N" } };

                var current_date = DateTime.Now;
                bill_no = "YCC";

                bill_no += string.Format("{0:D3}", (tenant_id == null ? 0 : (int)tenant_id));

                // 2 số cuối của năm
                bill_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //Tháng hiện tại
                bill_no += months[current_date.Month];

                //2. Số thứ tự đã dùng.
                long bill_count = contractPayDAL.CountPaymentRequest(tenant_id);

                //format numb
                string s_bill_new = string.Format(String.Format("{0,5:00000}", bill_count + 1));

                bill_no += s_bill_new;

                return bill_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BuildPaymentRequest - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                bill_no = "PYCC-" + contract_pay_default;
                return bill_no;
            }
        }

        public async Task<string> buildContractPay( int? tenant_id = null)
        {
            string bill_no = string.Empty;
            try
            {
                var current_date = DateTime.Now;
                bill_no = "PT";

                bill_no += string.Format("{0:D3}", (tenant_id == null ? 0 : (int)tenant_id));

                //1. 2 số cuối của năm
                bill_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //2. Số thứ tự phiếu thu trong năm.
                long bill_count = contractPayDAL.CountContractPayInYear(tenant_id);

                //format numb
                string s_bill_new = string.Format(String.Format("{0,5:00000}", bill_count + 1));

                //3.1 Check số phiếu thu này có chưa
                var check = await contractPayDAL.getContractPayByBillNo(bill_no + s_bill_new);

                if (!string.IsNullOrEmpty(check))
                {
                    //Nếu có rồi tăng lên 1                 
                    //LogHelper.InsertLogTelegram("buildContractPay - IdentifierServiceRepository" + bill_no + s_bill_new + " đã có. Check lại code");
                    bill_no += string.Format(String.Format("{0,5:00000}", bill_count + 2));
                }
                else
                {
                    bill_no += s_bill_new;
                }

                return bill_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildContractPay - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                bill_no = "PT-" + contract_pay_default;
                return bill_no;
            }
        }
        public async Task<string> BuildPaymentVoucher(int? tenant_id = null)
        {
            string bill_no = string.Empty;
            try
            {
                var current_date = DateTime.Now;
                bill_no = "PC";

                bill_no += string.Format("{0:D3}", (tenant_id == null ? 0 : (int)tenant_id));

                //1. 2 số cuối của năm
                bill_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);


                //2. Số thứ tự trong năm.
                long bill_count = contractPayDAL.CountPaymentVoucherInYear(tenant_id);

                //format numb
                string s_bill_new = string.Format(String.Format("{0,5:00000}", bill_count + 1));

                bill_no += s_bill_new;

                return bill_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BuildPaymentVoucher - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                bill_no = "PC-" + contract_pay_default;
                return bill_no;
            }
        }
        public async Task<string> buildContractNo( int? tenant_id = null)
        {
            string contract_no = string.Empty;
            try
            {
                var current_date = DateTime.Now;
                contract_no = "HD";
                contract_no += string.Format("{0:D3}", (tenant_id == null ? 0 : (int)tenant_id));

                //1. 2 số cuối của năm
                contract_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //2. Số thứ tự  trong năm.
                long order_count = contractDAL.CountContractInYear(tenant_id);

                //format numb
                string s_format = string.Format(String.Format("{0,4:0000}", order_count + 1));

                contract_no += s_format;

                return contract_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildContractNo - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                contract_no = "ADA-HD-" + contract_pay_default;
                return contract_no;
            }
        }
        public async Task<string> buildClientNo(int client_type, int? tenant_id = null)
        {
            string code = ClientTypeName.service[Convert.ToInt16(client_type)];

            try
            {
                var current_date = DateTime.Now;
                int count = identifierDAL.countClientTypeUse(client_type,tenant_id);

                //so tu tang
                string s_format = string.Format(String.Format("{0,5:00000}", count + 1));

                //1. 2 số cuối của năm
                string two_year_last = current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);
                // TenantID
                string tenant_code = string.Format("{0:D3}", (tenant_id == null ? 0 : (int)tenant_id));

                code = code+ tenant_code + two_year_last + s_format;

                return code;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildClientNo - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var num_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                code = code + num_default;
                return code;
            }
        }

    }
}
