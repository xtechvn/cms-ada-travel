using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities.ViewModels
{
    public class UserProfitReportModel
    {
        public string CreateDateFromStr { get; set; }
        public string CreateDateToStr { get; set; }
        public int Type { get; set; } = 1;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public DateTime? CreateDateFrom
        {
            get
            {
                if (!string.IsNullOrEmpty(CreateDateFromStr))
                {
                    var lstDate = CreateDateFromStr.Split('-');
                    if (lstDate.Length == 0 || lstDate.Length == 1)
                        lstDate = CreateDateFromStr.Split('/');
                    CreateDateFromStr = lstDate[0] + "/" + lstDate[1] + "/" + lstDate[2];
                    var fromDate = DateUtil.StringToDate(CreateDateFromStr);
                    return new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 00, 00, 00, DateTimeKind.Local);
                }
                return null;
            }
        }   public DateTime? CreateDateTo
        {
            get
            {
                if (!string.IsNullOrEmpty(CreateDateToStr))
                {
                    var lstDate = CreateDateToStr.Split('-');
                    if (lstDate.Length == 0 || lstDate.Length == 1)
                        lstDate = CreateDateToStr.Split('/');
                    CreateDateToStr = lstDate[0] + "/" + lstDate[1] + "/" + lstDate[2];
                    var toDate = DateUtil.StringToDate(CreateDateToStr);
                    return new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 00, 00, 00, DateTimeKind.Local);
                }
                return null;
            }
        }
    }
    public class UserProfitReportViewModel
    {
            public string FullName { get; set; }
            public string Avata { get; set; }
            public double TotalProfit { get; set; }
    }
}
