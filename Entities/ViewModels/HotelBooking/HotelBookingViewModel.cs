using System;
using System.Collections.Generic;
using System.Text;
using Entities.Models;

namespace Entities.ViewModels.HotelBooking
{
   public class HotelBookingViewModel : Entities.Models.HotelBooking
    {
        public string SalerId_name { get; set; }
        public string RoomTypeName { get; set; }
        public double TotalRooms { get; set; }
        public double TotalDays { get; set; }
        public double RoomNumberOfAdult { get; set; }
        public double RoomNumberOfChild { get; set; }
        public double RoomNumberOfInfant { get; set; }
        public double NumberOfRooms { get; set; }
    }
}
