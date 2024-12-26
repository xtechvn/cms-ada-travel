using ENTITIES.ViewModels.Hotel;
using MongoDB.Driver;
using Utilities;

namespace DAL.MongoDB.Hotel
{
    public class HotelBookingMongoService
    {
        private IMongoCollection<BookingHotelMongoViewModel> bookingCollection;

        public HotelBookingMongoService(IConfiguration _configuration)
        {
            try
            {
                string _connection = "mongodb://" 
                    + _configuration["DataBaseConfig:MongoServer:user"] 
                    + ":"  + _configuration["DataBaseConfig:MongoServer:pwd"]
                    +  "@" + _configuration["DataBaseConfig:MongoServer:Host"] 
                    + ":"  + _configuration["DataBaseConfig:MongoServer:Port"] 
                    + "/" + _configuration["DataBaseConfig:MongoServer:catalog_core"];

                var booking = new MongoClient(_connection);
                IMongoDatabase db = booking.GetDatabase(_configuration["DataBaseConfig:MongoServer:catalog_core"]);
                bookingCollection = db.GetCollection<BookingHotelMongoViewModel>("BookingHotel");
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BookingHotelDAL - BookingHotelDAL: " + ex);
                throw;
            }
        }
        public async Task<string> InsertBooking(BookingHotelMongoViewModel item)
        {
            try
            {
                item.GenID();
                await bookingCollection.InsertOneAsync(item);
                return item._id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertBooking - BookingHotelDAL - Cannot Excute: " + ex.ToString());
                return null;
            }
        }
        public BookingHotelMongoViewModel GetBookingById(string id)
        {
            try
            {
            
                var filter = Builders<BookingHotelMongoViewModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<BookingHotelMongoViewModel>.Filter.Eq(x => x._id, id);
               
                var model = bookingCollection.Find(filterDefinition).FirstOrDefault();
                if (model != null && model._id != null && model._id.Trim() != "")
                    return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetBookingById - BookingHotelDAL - Cannot Excute: " + ex.ToString());
            }
            return null;
        }
        public async Task<string> UpdateBooking(BookingHotelMongoViewModel item,string booking_id)
        {
            try
            {
                if(booking_id!=null && booking_id.Trim() != "")
                {
                    var filter = Builders<BookingHotelMongoViewModel>.Filter;
                    var filterDefinition = filter.Empty;
                    filterDefinition &= Builders<BookingHotelMongoViewModel>.Filter.Eq(x => x._id, booking_id);
                    item._id = booking_id;
                    await bookingCollection.ReplaceOneAsync(filterDefinition, item);
                    return item._id;
                }
                item.GenID();
                await bookingCollection.InsertOneAsync(item);
                return item._id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertBooking - BookingHotelDAL - Cannot Excute: " + ex.ToString());
                return null;
            }
        }
    }
}
