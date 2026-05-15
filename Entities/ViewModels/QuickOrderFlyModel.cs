using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class QuickOrderFlyModel
    {
        public long HoldTicketId { get; set; }
        public int BookingId { get; set; }
        public long ClientId { get; set; }
        public int SalerId { get; set; }
        public int OperatorId { get; set; }
        public int BranchCode { get; set; }
        public string Label { get; set; }
        public string Note { get; set; }

        public int AdultQuantity { get; set; }
        public double AdultPrice { get; set; }
        public double AdultAmount { get; set; }

        public int ChildQuantity { get; set; }
        public double ChildtPrice { get; set; }
        public double ChildAmount { get; set; }

        public int InfantQuantity { get; set; }
        public double InfantPrice { get; set; }
        public double InfantAmount { get; set; }
        public double? OthersAmount { get; set; }
        public double? Commission { get; set; }
        public double? Amount { get; set; }
        public double? Profit { get; set; }
    }
}
