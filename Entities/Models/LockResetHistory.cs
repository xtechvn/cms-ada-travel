using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class LockResetHistory
    {
        public long Id { get; set; }
        public int HotelId { get; set; }
        public int RoomId { get; set; }
        public long LockId { get; set; }
        public long? BookingId { get; set; }
        public byte ResetType { get; set; }
        public string PasswordEnc { get; set; }
        public bool SentTele { get; set; }
        public bool SentEmail { get; set; }
        public DateTime ResetAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
