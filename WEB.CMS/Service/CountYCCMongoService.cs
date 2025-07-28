using Entities.ViewModels.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using WEB.Adavigo.CMS.Service;

namespace WEB.CMS.Service
{
    public class CountYCCMongoService
    {
        private readonly IConfiguration configuration;
        public CountYCCMongoService(IConfiguration _configuration)
        {
            configuration = _configuration;

        }
        public async Task<long> Insert(PaymentRequestMongoModel model)
        {
            try
            {
                string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] + ":" + configuration["DataBaseConfig:MongoServer:pwd"] + "@" + configuration["DataBaseConfig:MongoServer:Host"] + ":" + configuration["DataBaseConfig:MongoServer:Port"] + "/" + configuration["DataBaseConfig:MongoServer:catalog_log"];
                var client = new MongoClient(url);

                IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog_log"]);
                PaymentRequestMongoModel log = new PaymentRequestMongoModel()
                {
                    _id = ObjectId.GenerateNewId().ToString(),
                    PaymentRequestId = model.PaymentRequestId,
                    Count = model.Count,
                    CreatedUserName = model.CreatedUserName,
                    CreatedTime = DateTime.Now

                };
                IMongoCollection<PaymentRequestMongoModel> affCollection = db.GetCollection<PaymentRequestMongoModel>(configuration["DataBaseConfig:MongoServer:Count_ycc"]);


                await affCollection.InsertOneAsync(log);


                return model.PaymentRequestId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - CountYCCMongoService: " + ex.Message);
            }
            return 0;
        }
        public int GetCountYCC(long PaymentRequestId)
        {
            var Count = 0;
            var listLog = new List<PaymentRequestMongoModel>();
            try
            {
                var db = MongodbService.GetDatabase();


                var collection = db.GetCollection<PaymentRequestMongoModel>(configuration["DataBaseConfig:MongoServer:Count_ycc"]);
                var filter = Builders<PaymentRequestMongoModel>.Filter.Empty;
                filter &= Builders<PaymentRequestMongoModel>.Filter.Where(s => s.PaymentRequestId == PaymentRequestId);
                var S = Builders<PaymentRequestMongoModel>.Sort.Descending("_id");
                listLog = collection.Find(filter).Sort(S).ToList();
                Count = listLog.Count;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetCountYCC - CountYCCMongoService . " + JsonConvert.SerializeObject(ex));
            }
            return Count;
        }
    }
}
