using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Vinpearl
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class VinpearlCreateBookingResponseModel
    {
        public bool isSuccess { get; set; }
        public Data data { get; set; }
        public string requestId { get; set; }
        public string traceId { get; set; }
    }
    public class Amenity
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string icon { get; set; }
    }

    public class Amount
    {
        public int amount { get; set; }
        public string currencyCode { get; set; }
    }

    public class CancellationPolicy
    {
        public string id { get; set; }
        public int amount { get; set; }
        public string type { get; set; }
        public int daysBeforeArrival { get; set; }
        public string weekdays { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public int weekdaysChecksum { get; set; }
        public int order { get; set; }
    }

    public class CancelPolicy
    {
        public string id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public bool isRefundable { get; set; }
        public string propertyId { get; set; }
        public List<Detail> detail { get; set; }
        public int prioritySequence { get; set; }
    }

    public class Country
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string regionId { get; set; }
    }

    public class Data
    {
        public List<Reservation> reservations { get; set; }
        public string paymentUrl { get; set; }
    }

    public class Detail
    {
        public string id { get; set; }
        public int amount { get; set; }
        public string type { get; set; }
        public int daysBeforeArrival { get; set; }
        public string weekdays { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public int weekdaysChecksum { get; set; }
        public int order { get; set; }
        public string depositType { get; set; }
        public int depositAmount { get; set; }
        public List<string> weekdaysEnum { get; set; }
        public string parentId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int timingValue { get; set; }
        public string timingTypeEnum { get; set; }
    }

    public class EmailImage
    {
        public string id { get; set; }
        public string url { get; set; }
        public int order { get; set; }
        public string type { get; set; }
    }

    public class Extend
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string icon { get; set; }
    }

    public class GuaranteePolicy
    {
        public string id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string propertyId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public List<Detail> detail { get; set; }
    }

    public class GuaranteePolicy2
    {
        public string id { get; set; }
        public string type { get; set; }
        public string depositType { get; set; }
        public int depositAmount { get; set; }
        public string weekdays { get; set; }
        public List<string> weekdaysEnum { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public int weekdaysChecksum { get; set; }
        public int order { get; set; }
        public string parentId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int timingValue { get; set; }
        public string timingTypeEnum { get; set; }
    }

    public class HotelType
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string code { get; set; }
        public string type { get; set; }
    }

    public class Market
    {
        public string id { get; set; }
        public string orgId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int status { get; set; }
    }

    public class Metadata
    {
        public string hotelId { get; set; }
        public string id { get; set; }
        public string tripAdvisor { get; set; }
        public string termAndCondition { get; set; }
        public string checkInDescription { get; set; }
        public string checkOutDescription { get; set; }
        public string bankBeneficiary { get; set; }
        public string bankAccountNumber { get; set; }
        public string bankName { get; set; }
        public string bankAddress { get; set; }
        public string bankSwiftCode { get; set; }
        public string generalDirectorName { get; set; }
        public string generalDirectorEmail { get; set; }
        public string generalDirectorPhone { get; set; }
        public string directionNote { get; set; }
        public string hotelMessages { get; set; }
        public string taxNumber { get; set; }
    }

    public class OtherOccupancy
    {
        public string otherOccupancyRefID { get; set; }
        public string otherOccupancyRefCode { get; set; }
        public int quantity { get; set; }
    }

    public class Profile
    {
        public string profileRefID { get; set; }
        public string identityID { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string profileType { get; set; }
        public bool isPrimary { get; set; }
        public DateTime passportExpiredDate { get; set; }
        public string gender { get; set; }
        public string lastName { get; set; }
        public string phoneNumber { get; set; }
    }

    public class PropertyInfo
    {
        public string id { get; set; }
        public string organizationId { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string description { get; set; }
        public int order { get; set; }
        public string locationCoordinates { get; set; }
        public int star { get; set; }
        public DateTime beginDate { get; set; }
        public string countryId { get; set; }
        public string street { get; set; }
        public string postCode { get; set; }
        public string city { get; set; }
        public string countryStateId { get; set; }
        public string telephone { get; set; }
        public string fax { get; set; }
        public string webAddress { get; set; }
        public string email { get; set; }
        public string currencySymbol { get; set; }
        public int numberOfRooms { get; set; }
        public DateTime checkInTime { get; set; }
        public DateTime checkOutTime { get; set; }
        public string baseLanguage { get; set; }
        public string hotelTypeId { get; set; }
        public string code { get; set; }
        public bool status { get; set; }
        public string chainId { get; set; }
        public string regionId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int utc { get; set; }
        public string termAndCondition { get; set; }
        public string checkInDescription { get; set; }
        public string checkOutDescription { get; set; }
        public string tripAdvisor { get; set; }
        public Country country { get; set; }
        public State state { get; set; }
        public List<Thumbnail> thumbnails { get; set; }
        public List<EmailImage> emailImages { get; set; }
        public HotelType hotelType { get; set; }
        public List<Amenity> amenities { get; set; }
        public List<RoomType> roomTypes { get; set; }
        public List<object> policies { get; set; }
        public Metadata metadata { get; set; }
        public string logo { get; set; }
        public string childPolicy { get; set; }
        public string extraBedPolicy { get; set; }
        public string parkingShuttle { get; set; }
        public string petPolicy { get; set; }
        public string smokingPolicy { get; set; }
        public string paymentPolicy { get; set; }
        public string roundingType { get; set; }
        public int allowedDecimal { get; set; }
        public string thousandDelimiter { get; set; }
        public string decimalDelimiter { get; set; }
        public string negativeDisplayType { get; set; }
        public string timezone { get; set; }
    }

    public class RateAmount
    {
        public Amount amount { get; set; }
        public List<object> taxFeeItems { get; set; }
    }

    public class RatePlanInfo
    {
        public string id { get; set; }
        public string ratePlanId { get; set; }
        public string marketId { get; set; }
        public string sourceId { get; set; }
        public string rateCode { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string shortDescription { get; set; }
        public DateTime beginSellDate { get; set; }
        public DateTime endSellDate { get; set; }
        public int minLos { get; set; }
        public int minAdvanceBooking { get; set; }
        public bool isTiered { get; set; }
        public bool isBar { get; set; }
        public bool status { get; set; }
        public string propertyId { get; set; }
        public string orgId { get; set; }
        public string transactionCodeId { get; set; }
        public bool taxIncluded { get; set; }
        public bool isPublic { get; set; }
        public int inheritType { get; set; }
        public int baseRateModifyUnitType { get; set; }
        public int baseRateModifyRoundingType { get; set; }
        public int baseRateModifyRoundingTo { get; set; }
        public string packageModifyRoundingType { get; set; }
        public string packageModifyRoundingValue { get; set; }
        public string currency { get; set; }
        public string parentId { get; set; }
        public bool isDailyRate { get; set; }
        public bool isAdvDailyRate { get; set; }
        public bool isAdvDailyBaseRate { get; set; }
        public bool keepInheritedExtra { get; set; }
        public bool dayUse { get; set; }
        public bool allowZeroRate { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public List<string> roomTypeIds { get; set; }
        public List<Tag> tags { get; set; }
        public CancelPolicy cancelPolicy { get; set; }
        public GuaranteePolicy guaranteePolicy { get; set; }
        public List<object> taxes { get; set; }
        public List<object> distributionChannels { get; set; }
        public Market market { get; set; }
        public Source source { get; set; }
        public string rateType { get; set; }
    }

    public class Reservation
    {
        public string reservationID { get; set; }
        public string status { get; set; }
        public string propertyID { get; set; }
        public string propertyCode { get; set; }
        public string propertyName { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime departureDate { get; set; }
        public DateTime bookingDate { get; set; }
        public DateTime updatedDate { get; set; }
        public string confirmationNumber { get; set; }
        public string itineraryNumber { get; set; }
        public string otaPaymentInfo { get; set; }
        public RoomOccupancy roomOccupancy { get; set; }
        public List<RoomRate> roomRates { get; set; }
        public List<Profile> profiles { get; set; }
        public Total total { get; set; }
        public List<CancellationPolicy> cancellationPolicies { get; set; }
        public List<GuaranteePolicy> guaranteePolicies { get; set; }
        public List<object> amendmentPolicies { get; set; }
        public PropertyInfo propertyInfo { get; set; }
        public string distributionChannel { get; set; }
        public string marketCode { get; set; }
        public string sourceCodeRef { get; set; }
        public bool notPushToPMS { get; set; }
        public bool pushToPMSAlready { get; set; }
        public string travelAgentName { get; set; }
        public string bookerName { get; set; }
        public string guestName { get; set; }
        public string roomTypeCode { get; set; }
        public string roomTypeNames { get; set; }
        public string roomIndex { get; set; }
        public string bookingType { get; set; }
        public string paymentStatus { get; set; }
        public DateTime paymentDueDate { get; set; }
        public string guaranteeType { get; set; }
        public int requestAmount { get; set; }
        public int settledAmount { get; set; }
    }

    public class RoomOccupancy
    {
        public int numberOfAdult { get; set; }
        public List<OtherOccupancy> otherOccupancies { get; set; }
    }

    public class RoomRate
    {
        public DateTime stayDate { get; set; }
        public string roomTypeCode { get; set; }
        public string roomTypeName { get; set; }
        public string roomTypeRefID { get; set; }
        public string ratePlanCode { get; set; }
        public string ratePlanName { get; set; }
        public string ratePlanRefID { get; set; }
        public string rateType { get; set; }
        public RoomTypeInfo roomTypeInfo { get; set; }
        public RatePlanInfo ratePlanInfo { get; set; }
        public RateAmount rateAmount { get; set; }
        public string allotmentId { get; set; }
        public string allotmentCode { get; set; }
        public string allotmentName { get; set; }
        public string allotmentType { get; set; }
        public bool allotmentRequiredPayment { get; set; }
    }

    public class RoomType
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public int numberOfRoom { get; set; }
        public int numberOfTotalRoom { get; set; }
        public int numberOfBed { get; set; }
        public string description { get; set; }
        public int defaultOccupancy { get; set; }
        public int maxOccupancy { get; set; }
        public int maxAdult { get; set; }
        public int maxChild { get; set; }
        public int order { get; set; }
        public string hotelId { get; set; }
        public string organizationId { get; set; }
        public int squareUnit { get; set; }
        public bool status { get; set; }
        public int numberOfBedRoom { get; set; }
        public string typeOfRoom { get; set; }
        public int maxExtraBeds { get; set; }
        public List<Thumbnail> thumbnails { get; set; }
        public List<Extend> extends { get; set; }
        public bool isCombined { get; set; }
        public List<object> combinedFrom { get; set; }
        public string shortDescription { get; set; }
        public string squareUnitType { get; set; }
    }

    public class RoomTypeInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public int numberOfRoom { get; set; }
        public int numberOfTotalRoom { get; set; }
        public int numberOfBed { get; set; }
        public string description { get; set; }
        public string shortDescription { get; set; }
        public int defaultOccupancy { get; set; }
        public int maxOccupancy { get; set; }
        public int maxAdult { get; set; }
        public int maxChild { get; set; }
        public int order { get; set; }
        public string hotelId { get; set; }
        public string organizationId { get; set; }
        public int squareUnit { get; set; }
        public string squareUnitType { get; set; }
        public bool status { get; set; }
        public int numberOfBedRoom { get; set; }
        public string typeOfRoom { get; set; }
        public int maxExtraBeds { get; set; }
        public List<Thumbnail> thumbnails { get; set; }
        public List<Extend> extends { get; set; }
        public bool isCombined { get; set; }
        public List<object> combinedFrom { get; set; }
    }

   

    public class Source
    {
        public string id { get; set; }
        public string orgId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int status { get; set; }
    }

    public class State
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string thumbnail { get; set; }
    }

    public class Tag
    {
        public string id { get; set; }
        public string tagName { get; set; }
        public string tagKey { get; set; }
    }

    public class Thumbnail
    {
        public string id { get; set; }
        public string url { get; set; }
        public int order { get; set; }
        public string type { get; set; }
    }

    public class Total
    {
        public Amount amount { get; set; }
        public List<object> taxFeeItems { get; set; }
    }


}
