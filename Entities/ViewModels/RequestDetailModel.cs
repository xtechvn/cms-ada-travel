using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class RequestDetailModel: Entities.Models.Request
    {
        public string SalerName { get; set; }
        public string StatusName { get; set; }
        public string OrderNo { get; set; }
        public string HotelName { get; set; }
        public string Email { get; set; }

        public double Amount { get; set; }
        public long VoucherId { get; set; }
        public string VoucherName { get; set; }
        public double Discount { get; set; }
    }
}
