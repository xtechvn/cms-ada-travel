using Entities.ViewModels.Mongo;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.Adavigo.CMS.Service;

namespace WEB.CMS.Service
{
    public class QuotationMongoService
    {
        private readonly IMongoCollection<QuotationMongoModel> _quotationCollection;

        public QuotationMongoService(IConfiguration configuration)
        {
            var db = MongodbService.GetDatabase();
            _quotationCollection = db.GetCollection<QuotationMongoModel>("Quotation");
        }

        /// <summary>
        /// Lưu hoặc cập nhật Báo giá trong MongoDB
        /// </summary>
        public async Task<string> SaveOrUpdate(QuotationMongoModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model._id))
                {
                    model._id = ObjectId.GenerateNewId().ToString();
                    model.CreatedDate = DateTime.Now;
                    model.QuotationNo = await GenerateQuotationNo();
                    
                    await _quotationCollection.InsertOneAsync(model);
                    return model._id;
                }
                else
                {
                    var filter = Builders<QuotationMongoModel>.Filter.Eq(x => x._id, model._id);
                    var existing = await _quotationCollection.Find(filter).FirstOrDefaultAsync();
                    if (existing == null)
                    {
                        // Trường hợp ID có sẵn nhưng không tồn tại trong DB, chèn mới
                        model.CreatedDate = DateTime.Now;
                        model.QuotationNo = await GenerateQuotationNo();
                        await _quotationCollection.InsertOneAsync(model);
                        return model._id;
                    }
                    
                    // Giữ lại các trường lịch sử bất biến
                    model.CreatedDate = existing.CreatedDate;
                    model.QuotationNo = existing.QuotationNo;
                    model.CreatedBy = existing.CreatedBy;
                    model.SalerName = existing.SalerName;
                    
                    await _quotationCollection.ReplaceOneAsync(filter, model);
                    return model._id;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("QuotationMongoService - SaveOrUpdate: " + ex);
                return null;
            }
        }

        /// <summary>
        /// Lấy chi tiết báo giá theo ID
        /// </summary>
        public async Task<QuotationMongoModel> GetById(string id)
        {
            try
            {
                var filter = Builders<QuotationMongoModel>.Filter.Eq(x => x._id, id);
                return await _quotationCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("QuotationMongoService - GetById: " + ex);
                return null;
            }
        }

        /// <summary>
        /// Phân trang và tìm kiếm danh sách Báo giá
        /// </summary>
        public async Task<Tuple<List<QuotationMongoModel>, long>> GetPagingList(
            string query, int status, long salerId, DateTime? fromDate, DateTime? toDate, int pageIndex, int pageSize)
        {
            try
            {
                var filterBuilder = Builders<QuotationMongoModel>.Filter;
                var filter = filterBuilder.Empty;

                // 1. Lọc theo từ khóa (Mã báo giá hoặc Ghi chú)
                if (!string.IsNullOrEmpty(query))
                {
                    var cleanQuery = query.Trim();
                    filter &= filterBuilder.Or(
                        filterBuilder.Regex(x => x.QuotationNo, new BsonRegularExpression(cleanQuery, "i")),
                        filterBuilder.Regex(x => x.Note, new BsonRegularExpression(cleanQuery, "i"))
                    );
                }

                // 2. Lọc theo Trạng thái (0: Nháp, 1: Đã gửi, 2: Khách chốt)
                if (status >= 0)
                {
                    filter &= filterBuilder.Eq(x => x.Status, status);
                }

                // 3. Lọc theo Sales người tạo
                if (salerId > 0)
                {
                    filter &= filterBuilder.Eq(x => x.CreatedBy, salerId);
                }

                // 4. Lọc theo khoảng ngày tạo
                if (fromDate.HasValue)
                {
                    filter &= filterBuilder.Gte(x => x.CreatedDate, fromDate.Value.Date);
                }
                if (toDate.HasValue)
                {
                    filter &= filterBuilder.Lte(x => x.CreatedDate, toDate.Value.Date.AddDays(1).AddTicks(-1));
                }

                // Thực hiện đếm tổng số bản ghi thỏa mãn bộ lọc
                long totalCount = await _quotationCollection.CountDocumentsAsync(filter);

                // Lấy danh sách phân trang (sắp xếp giảm dần theo ngày tạo)
                var list = await _quotationCollection.Find(filter)
                    .SortByDescending(x => x.CreatedDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                return new Tuple<List<QuotationMongoModel>, long>(list, totalCount);
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("QuotationMongoService - GetPagingList: " + ex);
                return new Tuple<List<QuotationMongoModel>, long>(new List<QuotationMongoModel>(), 0);
            }
        }

        /// <summary>
        /// Đếm số lượng báo giá theo từng trạng thái để làm các Tab Card ở trên
        /// </summary>
        public async Task<Dictionary<int, long>> GetStatusCounts(string query, long salerId, DateTime? fromDate, DateTime? toDate)
        {
            var counts = new Dictionary<int, long>
            {
                { -1, 0 }, // Tất cả
                { 0, 0 },  // Nháp
                { 1, 0 },  // Đã gửi
                { 2, 0 }   // Khách chốt
            };

            try
            {
                var filterBuilder = Builders<QuotationMongoModel>.Filter;
                var filter = filterBuilder.Empty;

                if (!string.IsNullOrEmpty(query))
                {
                    var cleanQuery = query.Trim();
                    filter &= filterBuilder.Or(
                        filterBuilder.Regex(x => x.QuotationNo, new BsonRegularExpression(cleanQuery, "i")),
                        filterBuilder.Regex(x => x.Note, new BsonRegularExpression(cleanQuery, "i"))
                    );
                }

                if (salerId > 0)
                {
                    filter &= filterBuilder.Eq(x => x.CreatedBy, salerId);
                }

                if (fromDate.HasValue)
                {
                    filter &= filterBuilder.Gte(x => x.CreatedDate, fromDate.Value.Date);
                }
                if (toDate.HasValue)
                {
                    filter &= filterBuilder.Lte(x => x.CreatedDate, toDate.Value.Date.AddDays(1).AddTicks(-1));
                }

                // Tính tổng số bản ghi
                counts[-1] = await _quotationCollection.CountDocumentsAsync(filter);

                // Tính số lượng của từng trạng thái riêng biệt
                counts[0] = await _quotationCollection.CountDocumentsAsync(filter & filterBuilder.Eq(x => x.Status, 0));
                counts[1] = await _quotationCollection.CountDocumentsAsync(filter & filterBuilder.Eq(x => x.Status, 1));
                counts[2] = await _quotationCollection.CountDocumentsAsync(filter & filterBuilder.Eq(x => x.Status, 2));
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("QuotationMongoService - GetStatusCounts: " + ex);
            }

            return counts;
        }

        /// <summary>
        /// Xóa báo giá theo ID
        /// </summary>
        public async Task<bool> Delete(string id)
        {
            try
            {
                var filter = Builders<QuotationMongoModel>.Filter.Eq(x => x._id, id);
                var result = await _quotationCollection.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("QuotationMongoService - Delete: " + ex);
                return false;
            }
        }

        /// <summary>
        /// Tự động sinh mã Báo giá dạng BG[yymmdd]-[Sequence]
        /// </summary>
        private async Task<string> GenerateQuotationNo()
        {
            var todayStr = DateTime.Now.ToString("yyMMdd"); // Ví dụ: 260526
            var prefix = $"BG{todayStr}-"; // BG260526-

            try
            {
                // Lọc tất cả các bản ghi có mã bắt đầu bằng tiền tố của ngày hôm nay
                var filter = Builders<QuotationMongoModel>.Filter.Regex(
                    x => x.QuotationNo, new BsonRegularExpression($"^{prefix}")
                );

                var lastQuotation = await _quotationCollection.Find(filter)
                    .SortByDescending(x => x.QuotationNo)
                    .FirstOrDefaultAsync();

                int nextSeq = 1;
                if (lastQuotation != null && !string.IsNullOrEmpty(lastQuotation.QuotationNo))
                {
                    // Lấy phần đuôi số thứ tự (ví dụ: "008" từ "BG260526-008")
                    var parts = lastQuotation.QuotationNo.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int currentSeq))
                    {
                        nextSeq = currentSeq + 1;
                    }
                }

                return $"{prefix}{nextSeq:D3}"; // Định dạng 3 chữ số: BG260526-001
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("QuotationMongoService - GenerateQuotationNo: " + ex);
                return $"{prefix}001";
            }
        }
    }
}
