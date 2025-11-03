using DAL;
using DAL.Identifier;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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


        
        public async Task<string> buildOrderNoManual(int company_type=0)
        {
            string order_no_manual = string.Empty;
            try
            {
                var months = new Dictionary<int, string> { { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "K" }, { 10, "L" }, { 11, "M" }, { 12, "N" } };
                var current_date = DateTime.Now;
                switch (company_type)
                {
                    case 0:
                        {
                            order_no_manual = "A";
                        }
                        break;
                    case 1:
                        {
                            order_no_manual = "P";
                        }
                        break;
                    case 2:
                        {
                            order_no_manual = "D";
                        }
                        break;
                    default:
                        {
                            order_no_manual = "O";

                        }break;
                }
                //0. 2 số cuối của năm
                order_no_manual += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //1. Tháng hiện tại là index tham chiếu sang bảng chữ cái lấy chữ
                order_no_manual += months[current_date.Month];

                //2. Số thứ tự  trong năm.
                long order_count = orderDAL.CountOrderInYear();

                //format numb
                string s_order_new = string.Format(String.Format("{0,5:00000}", order_count + 1));

                //3.1 Check số này có chưa
                var check =  orderDAL.GetByOrderNo(order_no_manual + s_order_new);

                if (check!=null && check.OrderId>0)
                {
                    //Nếu có rồi tăng lên 1
                    order_no_manual += string.Format(String.Format("{0,5:00000}", order_count + 2));
                    LogHelper.InsertLogTelegram("buildOrderNoManual - IdentifierServiceRepository" + order_no_manual + s_order_new + " đã có. Check lại code");
                }
                else
                {
                    order_no_manual += s_order_new;
                }

                return order_no_manual;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildOrderNoManual - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                order_no_manual = "DH-" + contract_pay_default;
                return order_no_manual;
            }
        }

        /// <summary>
        /// Mã phiếu thu: PT + 2 ký tự năm tạo + 5 số phía sau tự tăng
        /// </summary>
        /// <param name="service_type"></param>
        /// <returns></returns>
        public async Task<string> buildContractPay()
        {
            string bill_no = string.Empty;
            try
            {
                var current_date = DateTime.Now;
                bill_no = "PT";

                //1. 2 số cuối của năm
                bill_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);


                //2. Số thứ tự phiếu thu trong năm.
                long bill_count = contractPayDAL.CountContractPayInYear();

                //format numb
                string s_bill_new = string.Format(String.Format("{0,5:00000}", bill_count + 1));

                //3.1 Check số phiếu thu này có chưa
                var check = await contractPayDAL.getContractPayByBillNo(bill_no + s_bill_new);

                if (!string.IsNullOrEmpty(check))
                {
                    //Nếu có rồi tăng lên 1
                    bill_no += string.Format(String.Format("{0,5:00000}", bill_count + 2));
                    LogHelper.InsertLogTelegram("buildContractPay - IdentifierServiceRepository" + bill_no + s_bill_new + " đã có. Check lại code");
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
        public async Task<string> buildContractNo()
        {
            string contract_no = string.Empty;
            try
            {
                var current_date = DateTime.Now;
                contract_no = "HD";

                //1. 2 số cuối của năm
                contract_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //2. Số thứ tự  trong năm.
                long order_count = contractDAL.CountContractInYear();

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
        public int countServiceUse(int service_type)
        {
            return identifierDAL.countServiceUse(service_type);
        }
        public async Task<string> buildClientNo( int client_type)
        {
            string code = ClientTypeName.service[Convert.ToInt16(client_type)];

            try
            {
                var current_date = DateTime.Now;
                int count = identifierDAL.countClientTypeUse(client_type);

                //so tu tang
                string s_format = string.Format(String.Format("{0,5:00000}", count + 1));

                //1. 2 số cuối của năm
                string two_year_last = current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                code = code + two_year_last + s_format;

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
