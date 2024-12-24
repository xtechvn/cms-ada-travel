using Telegram.Bot.Types;
using Utilities.Contants;
using Utilities;
using Newtonsoft.Json;
using WEB.CMS.Models;
using Entities.ViewModels;

namespace WEB.CMS.Service
{
    public class CommentService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;



        public CommentService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _context = httpContextAccessor;
        }


        public async Task<List<CommentViewModel>> GetListCommentsAsync(int requestId)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, object>
                {
                    { "request_id", requestId }
                };

                var data_product = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:b2b"]);

                var request = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", token)
                });

                //var url = "https://localhost:44396" + "/api/comment/get-list.json";
                var url = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().GET_COMMENT_LISTING;
                var response = await httpClient.PostAsync(url, request);
                var stringResult = "";
                if (response.IsSuccessStatusCode == true)
                {
                    //var stringResult = await response.Content.ReadAsStringAsync();
                    stringResult = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JsonConvert.DeserializeObject<ObjectResponseAPI<List<CommentViewModel>>>(stringResult);

                    return apiResponse.data; // Trả về danh sách comment

                }



                return new List<CommentViewModel>(); // Nếu không thành công, trả về danh sách rỗng
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"GetListComments Error: {ex}");
                return new List<CommentViewModel>();
            }
        }




        public async Task<CommentViewModel> AddCommentAsync(int requestId, int userid, int type, string content, List<string> attachFileUrls, List<string> attachFileNames, int createdBy)
        {
            try
            {

                // Chuẩn bị dữ liệu gọi API lưu comment
                var commentData = new
                {
                    request_id = requestId,
                    userid,
                    type = (int)AttachmentType.Addservice_Comment,
                    content,
                    attach_files = attachFileUrls.Zip(attachFileNames, (url, name) => new { url, name }),
                    created_by = createdBy,
                    user_type = 1,
                };

                var data_product = JsonConvert.SerializeObject(commentData);
                var token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:b2b"]);

                var request = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("token", token)
        });
                using var httpClient = new HttpClient();
                //var url = "https://localhost:44396" + "/api/comment/add.json";
                var url = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().ADD_COMMENT;
                var response = await httpClient.PostAsync(url, request);

                var stringResult = "";
                if (response.IsSuccessStatusCode)
                {
                    stringResult = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ObjectResponseAPI<CommentViewModel>>(stringResult);
                    return result.data;
                }


                return null; // Thất bại
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"AddComment Error: {ex}");
                return null;
            }
        }

    }
}
