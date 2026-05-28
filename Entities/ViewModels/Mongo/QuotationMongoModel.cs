using System;
using System.Collections.Generic;

namespace Entities.ViewModels.Mongo
{
    public class QuotationMongoModel
    {
        public string _id { get; set; } // MongoDB ObjectId dạng string
        public string QuotationNo { get; set; } // BG250526-001
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        
        public long CreatedBy { get; set; } // Sales phụ trách (Người tạo)
        public string SalerName { get; set; }
        public string Email { get; set; }
        
        public string Note { get; set; } // Ghi chú hiển thị cho khách hàng
        public int Status { get; set; } // 0: Nháp, 1: Đã gửi báo giá, 2: Khách chốt
        
        // Tổng hợp tài chính
        public double ServicesPriceImport { get; set; } // Tổng tiền nhập gốc của các dịch vụ
        public double ServicesPriceExport { get; set; } // Tổng tiền bán gốc của các dịch vụ
        
        public double OtherFees { get; set; } // Chi phí khác
        public double CollaboratorComm { get; set; } // Hoa hồng CTV
        public double CustomerCareFund { get; set; } // Quỹ CSKH
        
        public double TotalAmount { get; set; } // Tổng tiền bán = ServicesPriceExport
        public double TotalPrice { get; set; } // Tổng tiền bán = ServicesPriceExport
        public double TotalProfit { get; set; } // Lợi nhuận = TotalPrice - ServicesPriceImport - OtherFees - CollaboratorComm - CustomerCareFund
        
        // Các danh sách dịch vụ tách riêng theo model gốc
        public List<QuotationHotelService> Hotels { get; set; } = new List<QuotationHotelService>();
        public List<QuotationFlightService> Flights { get; set; } = new List<QuotationFlightService>();
        public List<QuotationTourService> Tours { get; set; } = new List<QuotationTourService>();
        public List<QuotationOtherService> Others { get; set; } = new List<QuotationOtherService>();
        public List<QuotationVinWonderService> VinWonders { get; set; } = new List<QuotationVinWonderService>();
    }

    #region 1. DỊCH VỤ KHÁCH SẠN (HOTEL)
    public class QuotationHotelService
    {
        public string Id { get; set; }
        public string HotelId { get; set; }
        public string HotelName { get; set; }
        public string ServiceCode { get; set; }
        public int NumberOfRooms { get; set; }
        public DateTime ArriveDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public int NumberOfAdult { get; set; }
        public int NumberOfChild { get; set; }
        public int NumberOfInfant { get; set; }
        public string Note { get; set; }
        public double OtherAmount { get; set; }
        public double Discount { get; set; }
        public double Voucher { get; set; }
        public double Amount { get; set; } // Tổng tiền bán của khách sạn
        
        public List<QuotationHotelRoom> Rooms { get; set; } = new List<QuotationHotelRoom>();
        public List<QuotationHotelExtraPackage> ExtraPackages { get; set; } = new List<QuotationHotelExtraPackage>();
        public List<QuotationHotelGuest> Guests { get; set; } = new List<QuotationHotelGuest>();
    }

    public class QuotationHotelRoom
    {
        public string Id { get; set; }
        public int RoomNo { get; set; }
        public string RoomTypeId { get; set; }
        public string RoomTypeCode { get; set; }
        public string RoomTypeName { get; set; }
        public short NumberOfRooms { get; set; }
        public int NumberOfAdult { get; set; }
        public int NumberOfChild { get; set; }
        public int NumberOfInfant { get; set; }
        public List<QuotationHotelRoomRate> Package { get; set; } = new List<QuotationHotelRoomRate>();
    }

    public class QuotationHotelRoomRate
    {
        public string Id { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public double OperatorPrice { get; set; } // Giá nhập
        public double SalePrice { get; set; } // Giá bán
        public double Amount { get; set; } // Thành tiền bán
        public double Profit { get; set; }
        public short Nights { get; set; }
    }

    public class QuotationHotelExtraPackage
    {
        public string Id { get; set; }
        public int PackageId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double OperatorPrice { get; set; }
        public double SalePrice { get; set; }
        public int Nights { get; set; }
        public int NumberOfExtraPackages { get; set; }
        public double Amount { get; set; }
        public double Profit { get; set; }
    }

    public class QuotationHotelGuest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public int RoomNo { get; set; }
        public string Note { get; set; }
        public short Type { get; set; }
    }
    #endregion

    #region 2. DỊCH VỤ VÉ MÁY BAY (FLIGHT)
    public class QuotationFlightService
    {
        public string Id { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ServiceCode { get; set; }
        public string Note { get; set; }
        public double AmountAdt { get; set; }
        public double AmountChd { get; set; }
        public double AmountInf { get; set; }
        public double Profit { get; set; }
        public double OthersAmount { get; set; }
        public double Commission { get; set; }
        
        public QuotationFlightRoute Go { get; set; }
        public QuotationFlightRoute Back { get; set; }
        public List<QuotationFlightPassenger> Passenger { get; set; } = new List<QuotationFlightPassenger>();
        public List<QuotationFlightExtraPackage> ExtraPackages { get; set; } = new List<QuotationFlightExtraPackage>();
    }

    public class QuotationFlightRoute
    {
        public string Id { get; set; }
        public string Airline { get; set; }
        public string FlyCode { get; set; }
        public string BookingCode { get; set; }
    }

    public class QuotationFlightPassenger
    {
        public string Id { get; set; }
        public int Genre { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public string Note { get; set; }
    }

    public class QuotationFlightExtraPackage
    {
        public string Id { get; set; }
        public string PackageId { get; set; }
        public string PackageCode { get; set; }
        public double BasePrice { get; set; } // Giá nhập
        public int Quantity { get; set; }
        public double Amount { get; set; } // Giá bán
        public double Profit { get; set; }
    }
    #endregion

    #region 3. DỊCH VỤ TOUR DU LỊCH (TOUR)
    public class QuotationTourService
    {
        public string Id { get; set; }
        public string TourType { get; set; }
        public int OrganizingType { get; set; }
        public int IsSelfDesigned { get; set; }
        public int StartPoint { get; set; }
        public string EndPoint { get; set; }
        public string TourProductName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ServiceCode { get; set; }
        public string Note { get; set; }
        public double OtherAmount { get; set; }
        public double FundCustomerCare { get; set; }
        public double Commission { get; set; }
        public double Price { get; set; }
        public double Profit { get; set; }
        
        public List<QuotationTourPassenger> Guest { get; set; } = new List<QuotationTourPassenger>();
        public List<QuotationTourExtraPackage> ExtraPackages { get; set; } = new List<QuotationTourExtraPackage>();
    }

    public class QuotationTourPassenger
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public short Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string Cccd { get; set; }
        public string RoomNumber { get; set; }
        public string Note { get; set; }
    }

    public class QuotationTourExtraPackage
    {
        public string Id { get; set; }
        public string PackageId { get; set; }
        public string PackageCode { get; set; }
        public int Quantity { get; set; }
        public double BasePrice { get; set; } // Giá nhập
        public double Amount { get; set; } // Giá bán
        public double Price { get; set; }
        public double Profit { get; set; }
    }
    #endregion

    #region 4. DỊCH VỤ KHÁC (OTHER)
    public class QuotationOtherService
    {
        public string Id { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Note { get; set; }
        public double OthersAmount { get; set; }
        public double Commission { get; set; }
        public List<QuotationOtherPackage> Packages { get; set; } = new List<QuotationOtherPackage>();
    }

    public class QuotationOtherPackage
    {
        public string Id { get; set; }
        public string PackageName { get; set; }
        public double BasePrice { get; set; } // Giá nhập
        public int Quantity { get; set; }
        public double Amount { get; set; } // Giá bán
        public double Profit { get; set; }
        public double SalePrice { get; set; }
    }
    #endregion

    #region 5. DỊCH VỤ VINWONDER (VINWONDER)
    public class QuotationVinWonderService
    {
        public string Id { get; set; }
        public string ServiceCode { get; set; }
        public long LocationId { get; set; }
        public string LocationName { get; set; }
        public string Note { get; set; }
        public double OthersAmount { get; set; }
        public double Commission { get; set; }
        public List<QuotationVinWonderPackage> Packages { get; set; } = new List<QuotationVinWonderPackage>();
        public List<QuotationVinWonderPassenger> Guest { get; set; } = new List<QuotationVinWonderPassenger>();
    }

    public class QuotationVinWonderPackage
    {
        public string Id { get; set; }
        public string PackageName { get; set; }
        public double BasePrice { get; set; } // Giá nhập
        public int Quantity { get; set; }
        public double Amount { get; set; } // Giá bán
        public double Profit { get; set; }
        public DateTime DateUsed { get; set; }
    }

    public class QuotationVinWonderPassenger
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
    }
    #endregion
}
