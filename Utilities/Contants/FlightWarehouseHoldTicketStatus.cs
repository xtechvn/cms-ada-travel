using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Contants
{
    public enum FlightWarehouseHoldTicketStatus
    {
        /// <summary>
        /// Đang giữ chỗ
        /// </summary>
        HOLD = 0,
        /// <summary>
        /// Đã xuất vé
        /// </summary>
        ISSUED = 1,
        /// <summary>
        /// Hủy giữ chỗ
        /// </summary>
        CANCELLED = 2,
    }
}
