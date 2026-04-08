using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class HotelRoomFundModel: HotelRoomFund
    {
        public string CreateName { get; set; }
        public string UpdateName { get; set; }
        public string HotelName { get; set; }
        public string SupplierName { get; set; }
        public int TotalRow { get; set; }
    }
}
