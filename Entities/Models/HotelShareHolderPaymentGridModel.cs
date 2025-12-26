using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class HotelShareHolderPaymentGridModel
    {
        public int Id { get; set; }

        public int ShareHolderId { get; set; }
        public string ShareHolderName { get; set; }
        public string ShareHolderUserName { get; set; }


        public decimal Amount { get; set; }
        public DateTime PayDate { get; set; }
        public string Note { get; set; }

        public string UserCreate { get; set; }
        public DateTime? CreateDate { get; set; }

        public int TotalRow { get; set; }
    }
    public class HotelShareHolderPayment
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        public int ShareHolderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PayDate { get; set; }
        public string Note { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class ShareHolderSearchViewModel
    {
        public int ShareHolderId { get; set; }
        public int HotelId { get; set; }
        public string FullName { get; set; }
        public int UserId { get; set; }
    }

    public class ApartmentPaymentSearchModel
    {
        public string ShareHolderName { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
    public class ApartmentPaymentCreateModel
    {
        public int ShareHolderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PayDate { get; set; }
        public string Note { get; set; }

        public int CreatedBy { get; set; }
    }

}
