using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.DebtGuarantee
{
    public class SearchDebtGuarantee
    {
        public string Code { get; set; }
        public int Status { get; set; } = -1;
        public string OrderId { get; set; }
        public int PageIndex { get; set; } = -1;
        public int PageSize { get; set; } = -1;
    }
}
