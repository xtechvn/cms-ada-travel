using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class QuickOrderRequestModel
    {
        public long ClientId { get; set; }
        public long SalerId { get; set; }
        public long OperatorId { get; set; }
        public List<long> SubSalerIds { get; set; }
        public string Label { get; set; }
        public int BranchCode { get; set; }
        public string Note { get; set; }
        public int HotelId { get; set; }
        public int SupplierId { get; set; }
        
        public int Adult { get; set; }
        public int Child { get; set; }
        public int Infant { get; set; }
        public double OtherAmount { get; set; }
        public double Commission { get; set; }

        public List<QuickRoomModel> Rooms { get; set; }
        public List<QuickGuestModel> Guests { get; set; }
    }

    public class QuickRoomModel
    {
        public int FundId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int NumberOfRooms { get; set; }
        public List<QuickRoomPackageModel> Packages { get; set; }
    }

    public class QuickRoomPackageModel
    {
        public int FundId { get; set; }
        public string PackageName { get; set; }
        public double ImportPrice { get; set; }
        public double ExportPrice { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class QuickGuestModel
    {
        public string Name { get; set; }
        public int RoomId { get; set; }
        public string Note { get; set; }
        public int Type { get; set; }
        public string Birthday { get; set; }
    }
}
