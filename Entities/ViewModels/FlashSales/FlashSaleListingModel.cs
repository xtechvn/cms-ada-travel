using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.FlashSales
{
    public class FlashSaleListingModel: Entities.Models.FlashSale
    {
        public long ActiveFlashSaleProductCount { get; set; }
        public long FlashSaleProductCount { get; set; }
        public string FlashSaleStatusText { get; set; }
        public string SupplierName { get; set; }
        public int TotalRow { get; set; }
        public string UserCreateName { get; set; } // JOIN thêm User
    }
}
