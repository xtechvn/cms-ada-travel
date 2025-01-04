using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.Adavigo.CMS.Service
{
    public class IndentiferService
    {
        private readonly IConfiguration _configuration;
        private readonly IIdentifierServiceRepository _identifierServiceRepository;
        private readonly IOrderRepository _orderRepository;

        public IndentiferService(IConfiguration configuration, IIdentifierServiceRepository identifierServiceRepository, IOrderRepository orderRepository)
        {
            _configuration = configuration;
            _identifierServiceRepository = identifierServiceRepository;
            _orderRepository = orderRepository;
        }

        public async Task<string> buildServiceNo(int service_type)
        {
            string service_name = ServicesTypeCode.service[Convert.ToInt16(service_type)];
            try
            {
                int count = _identifierServiceRepository.countServiceUse(service_type);
                //format numb
                string s_format = string.Format(String.Format("{0,4:0000}", count + 1));

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
        public async Task<string> buildOrderManual(int company_type)
        {
            string order_no = string.Empty;
            try
            {
                var months = new Dictionary<int, string> { { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "K" }, { 10, "L" }, { 11, "M" }, { 12, "N" } };
                var current_date = DateTime.Now;

                switch (company_type)
                {
                    case 0:
                        {
                            order_no += "A";

                        }break;
                    case 1:
                        {
                            order_no += "P";

                        }
                        break;
                    case 2:
                        {
                            order_no += "D";

                        }
                        break;
                }

                
                //3. 2 số cuối của năm
                order_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //4. Tháng hiện tại là index tham chiếu sang bảng chữ cái lấy chữ
                order_no += months[current_date.Month];

                //5. Ngày giao dich 
                order_no += current_date.ToString("dd");

                //6. Số thứ tự đơn hàng trong năm.
                long order_count = await _orderRepository.CountOrderInYear();

                //6.1: Check số đơn hàng này có chưa
                var order_check = await _orderRepository.GetOrderByOrderNo(order_no + order_count);

                if (order_check!=null && order_check.OrderId>0)
                {
                    //Nếu có rồi tăng lên 1
                    // order_no += (order_count + 1);
                    order_no += string.Format(String.Format("{0,4:0000}", order_count + 1));

                }
                else
                {
                    //order_no += order_count.ToString();
                    order_no += string.Format(String.Format("{0,4:0000}", order_count));


                }

                order_no = string.Format(String.Format("{0,4:0000}", order_no));

                return order_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildOrderManual - IndentiferService" + ex.ToString() );
                //Trả mã random
                var rd = new Random();
                var order_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                order_no = "MB-" + order_default;
                return order_no;
            }
        }
        public bool IsOrderManual(string orderNo)
        {

            try
            {
                if (orderNo.StartsWith("O")
                    || orderNo.StartsWith("A")
                    || orderNo.StartsWith("P")
                    || orderNo.StartsWith("D")
                    )
                {
                    return true;

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("IsOrderManual - OrderIndentiferService: " + ex.ToString());





            }
            return false;

        }
    }
}
