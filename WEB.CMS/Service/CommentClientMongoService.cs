using Entities.ViewModels;
using Entities.ViewModels.Mongo;
using Entities.ViewModels.TransferSms;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;

namespace WEB.CMS.Service
{
    public class CommentClientMongoService
    {
        private readonly IConfiguration configuration;
        public CommentClientMongoService(IConfiguration _configuration)
        {
            configuration = _configuration;

        }
        public async Task<long> InsertCommentClient(CommentClientMongoModel model)
        {
            try
            {
                string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] + ":" + configuration["DataBaseConfig:MongoServer:pwd"] + "@" + configuration["DataBaseConfig:MongoServer:Host"] + ":" + configuration["DataBaseConfig:MongoServer:Port"] + "/" + configuration["DataBaseConfig:MongoServer:catalog_log"];
                var client = new MongoClient(url);

                IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog_log"]);
                CommentClientMongoModel log = new CommentClientMongoModel()
                {
                    _id = ObjectId.GenerateNewId().ToString(),
                    Note = model.Note,
                    UserId = model.UserId,
                    FullName = model.FullName,
                    ClientId = model.ClientId,
                    UserName = model.UserName,
                    CreatedTime = DateTime.Now,
                    Type = model.Type,

                };
                IMongoCollection<CommentClientMongoModel> affCollection = db.GetCollection<CommentClientMongoModel>(configuration["DataBaseConfig:MongoServer:Comment_Clent_collection"]);


                await affCollection.InsertOneAsync(log);


                return model.UserId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PushLog - CommentClientMongoService: " + ex.Message);
            }
            return 0;
        }
        public List<CommentClientMongoModel> GetListComment(CommentClientMongoModel searchModel)
        {
            var list_comment = new List<CommentClientMongoModel>();
            try
            {
                var db = MongodbService.GetDatabase();


                var collection = db.GetCollection<CommentClientMongoModel>(configuration["DataBaseConfig:MongoServer:Comment_Clent_collection"]);
                var filter = Builders<CommentClientMongoModel>.Filter.Empty;
                filter &= Builders<CommentClientMongoModel>.Filter.Eq(n => n.ClientId, searchModel.ClientId);
                var S = Builders<CommentClientMongoModel>.Sort.Descending("CreatedTime");


                list_comment = collection.Find(filter).Sort(S).ToList();

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListLogActions - CommentClientMongoService. " + JsonConvert.SerializeObject(ex));
            }
            return list_comment;
        }
        public CommentClientMongoModel CommentNew(CommentClientMongoModel searchModel)
        {
            var list_comment = new CommentClientMongoModel();
            try
            {
                var db = MongodbService.GetDatabase();


                var collection = db.GetCollection<CommentClientMongoModel>(configuration["DataBaseConfig:MongoServer:Comment_Clent_collection"]);
                var filter = Builders<CommentClientMongoModel>.Filter.Empty;
                filter &= Builders<CommentClientMongoModel>.Filter.Eq(n => n.ClientId, searchModel.ClientId);
                filter &= Builders<CommentClientMongoModel>.Filter.Where(n => n.Type != (int)CommentClientMongoType.nhu_cau);
                var S = Builders<CommentClientMongoModel>.Sort.Descending("CreatedTime");


                list_comment = collection.Find(filter).Sort(S).FirstOrDefault();

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListLogActions - CommentClientMongoService. " + JsonConvert.SerializeObject(ex));
            }
            return list_comment;
        }
        public CommentClientMongoModel CommentNhuCau(CommentClientMongoModel searchModel)
        {
            var list_comment = new CommentClientMongoModel();
            try
            {
                var db = MongodbService.GetDatabase();


                var collection = db.GetCollection<CommentClientMongoModel>(configuration["DataBaseConfig:MongoServer:Comment_Clent_collection"]);
                var filter = Builders<CommentClientMongoModel>.Filter.Empty;
                filter &= Builders<CommentClientMongoModel>.Filter.Eq(n => n.ClientId, searchModel.ClientId);
                filter &= Builders<CommentClientMongoModel>.Filter.Eq(n => n.Type, (int)CommentClientMongoType.nhu_cau);
                var S = Builders<CommentClientMongoModel>.Sort.Descending("CreatedTime");


                list_comment = collection.Find(filter).Sort(S).FirstOrDefault();

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListLogActions - CommentClientMongoService. " + JsonConvert.SerializeObject(ex));
            }
            return list_comment;
        }
        public List<SystemLogMongDBRecruitmentModel> GetListRecruitment(RecruitmentSearchModel searchModel)
        {
            var list_Recruitment = new List<SystemLogMongDBRecruitmentModel>();
            try
            {
                var db = MongodbService.GetDatabase();

                var collection = db.GetCollection<SystemLogMongDBRecruitmentModel>(configuration["DataBaseConfig:MongoServer:Recruitment_Log"]);
                var filter = Builders<SystemLogMongDBRecruitmentModel>.Filter.Empty;
                filter &= Builders<SystemLogMongDBRecruitmentModel>.Filter.Eq(n => n.location, searchModel.location);
                filter &= Builders<SystemLogMongDBRecruitmentModel>.Filter.Eq(n => n.area, searchModel.area);
                if (!string.IsNullOrEmpty(searchModel.FromDateStr))
                {
                    filter &= Builders<SystemLogMongDBRecruitmentModel>.Filter.Gte("CreatedTime", searchModel.FromDate);
                }
                if (!string.IsNullOrEmpty(searchModel.ToDateStr))
                {
                    filter &= Builders<SystemLogMongDBRecruitmentModel>.Filter.Lte("CreatedTime", searchModel.ToDate);
                }
                var S = Builders<SystemLogMongDBRecruitmentModel>.Sort.Descending("CreatedTime");

                list_Recruitment = collection.Find(filter).Sort(S).ToList();

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListLogActions - CommentClientMongoService. " + JsonConvert.SerializeObject(ex));
            }
            return list_Recruitment;
        }
        public async Task<long> updateCommentClient(CommentClientMongoModel model)
        {
            try
            {
                string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] + ":" + configuration["DataBaseConfig:MongoServer:pwd"] + "@" + configuration["DataBaseConfig:MongoServer:Host"] + ":" + configuration["DataBaseConfig:MongoServer:Port"] + "/" + configuration["DataBaseConfig:MongoServer:catalog_log"];
                var client = new MongoClient(url);

                IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog_log"]);
  
                IMongoCollection<CommentClientMongoModel> affCollection = db.GetCollection<CommentClientMongoModel>(configuration["DataBaseConfig:MongoServer:Comment_Clent_collection"]);

                var filter = Builders<CommentClientMongoModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await affCollection.FindOneAndReplaceAsync(filterDefinition, model);

                return model.UserId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PushLog - CommentClientMongoService: " + ex.Message);
            }
            return 0;
        }
    }
}
