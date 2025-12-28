using Entities.Models;
using Entities.ViewModels.Hotel;
using System;
using System.Collections.Generic;

namespace Entities.ViewModels.Apartment
{
    public class ApartmentSuggestViewModel
    {
        /// <summary>
        /// Id căn hộ (Hotel.Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tên căn hộ (Hotel.Name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Địa chỉ hiển thị (ghép Street, City, State)
        /// </summary>
        public string Address { get; set; }
    }
    public class ApartmentRoomLedger
    {
        public int Id { get; set; }
        public int RoomId { get; set; }

        /// <summary>
        /// 1 = Thu, 2 = Chi
        /// </summary>
        public int LedgerType { get; set; }

        // ===================== THU =====================
        public string CustomerName { get; set; }
        public DateTime? ContractDate { get; set; }
        public DateTime? ContractExpired { get; set; }
        public decimal? RoomPrice { get; set; }
        public decimal? ServiceFee { get; set; }

        /// <summary>
        /// Số tiền đã thu (mapping đúng với AmountPaid trong DB)
        /// </summary>
        public decimal? AmountPaid { get; set; }


        // ====================== CHI ======================
        public int? ExpenseTypeId { get; set; }
        public string ExpenseType { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Số tiền chi (mapping đúng với ExpenseAmount trong DB)
        /// </summary>
        public decimal? ExpenseAmount { get; set; }

        public DateTime? ExpenseDate { get; set; }


        // ====================== SYSTEM ======================
        public DateTime CreatedDate { get; set; }
        public string PhoneNumber { get; set; }
        public int? DurationMonth { get; set; }
        public decimal? RentAmount { get; set; }
        public decimal? DepositAmount { get; set; }

    }

    public class ApartmentRoomLedgerViewModel
    {
        public HotelRoom RoomInfo { get; set; }
        public List<ApartmentRoomLedger> Ledgers { get; set; }
    }


    public class RoomLedgerViewModel
    {
        public int RoomId { get; set; }
        public List<LedgerThuViewModel> Thu { get; set; }
        public List<LedgerChiViewModel> Chi { get; set; }
    }

    public class LedgerThuViewModel
    {
        public string CustomerName { get; set; }
        public string ContractDate { get; set; }
        public string ExpireDate { get; set; }
        public decimal RoomPrice { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalPaid { get; set; }
    }
    public class ApartmentRoomLedgerModel
    {
        public int? Id { get; set; }
        public long? OrderId { get; set; }
        public int? HotelId { get; set; }
        public int RoomId { get; set; }
        public int LedgerType { get; set; }  // 1 = Thu, 2 = Chi

        // Thu
        public string CustomerName { get; set; }
        public DateTime? ContractDate { get; set; }
        public DateTime? ContractExpired { get; set; }
        public decimal? RoomPrice { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? AmountPaid { get; set; }

        // Chi
        public int? ExpenseTypeId { get; set; }
        public string ExpenseType { get; set; }
        public string Description { get; set; }
        public decimal? ExpenseAmount { get; set; }
        public DateTime? ExpenseDate { get; set; }

        public int CreatedBy { get; set; }
        public string PhoneNumber { get; set; }
        public int? DurationMonth { get; set; }
        public decimal? RentAmount { get; set; }
        public decimal? DepositAmount { get; set; }

    }


    public class LedgerChiViewModel
    {
        public int ExpenseTypeId { get; set; }   // 1 – Hoa hồng, 2 – Dịch vụ
        public string ExpenseType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string ExpenseDate { get; set; }
    }
    public class RoomLedgerInput
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }   // <<< THÊM
        public List<LedgerThuInput> Thu { get; set; }
        public List<LedgerChiInput> Chi { get; set; }
        public List<int> DeletedIds { get; set; } = new List<int>();
    }

    public class LedgerThuInput
    {
        public int Id { get; set; }  // Id bản ghi (nếu có)
        public string CustomerName { get; set; }
        public DateTime? ContractDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public decimal RoomPrice { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalPaid { get; set; }
        public string PhoneNumber { get; set; }
        public int DurationMonth { get; set; }
        public decimal RentAmount { get; set; }
        public decimal DepositAmount { get; set; }

    }

    public class LedgerChiInput
    {
        public int Id { get; set; }  // Id bản ghi (nếu có)
        public int ExpenseTypeId { get; set; }   // 1 – Hoa hồng, 2 – Dịch vụ
        public string ExpenseType { get; set; }
        public string Description { get; set; }
        
        public decimal ExpenseAmount { get; set; }

        public DateTime? ExpenseDate { get; set; }
    }
    public class RoomLedgerSummaryModel
    {
        public int RoomId { get; set; }
        public decimal TotalMustPay { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalCost { get; set; }
        public int Status { get; set; } // 0 = Đang xử lý, 1 = Hoàn thành

    }
}
