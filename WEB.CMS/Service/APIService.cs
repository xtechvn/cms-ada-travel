using Entities.ViewModels;
using Entities.ViewModels.ApiSever;
using Entities.ViewModels.Hotel;
using ENTITIES.ViewModels.Notify;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System.Text;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Models;

namespace WEB.DeepSeekTravel.CMS.Service
{
    public class APIService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public APIService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;

        }
        public async Task<BaseObjectResponse<OrderViewdetail>> GetOrderDetail(string order_id)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                BaseObjectResponse<OrderViewdetail> result = null;
                var j_param = new Dictionary<string, string>()
                {
                {"order_id",order_id },
                };
                var data = JsonConvert.SerializeObject(j_param);
                var a = _configuration["DataBaseConfig:key_api:api_manual"];
                var token = EncodeHelpers.Encode(data, _configuration["DataBaseConfig:key_api:api_manual"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().API_GET_ORDER_BY_ORDERID;
                var response = await httpClient.PostAsync(url, request);
                var stringResult = "";

                if (response.IsSuccessStatusCode)
                {
                    stringResult = response.Content.ReadAsStringAsync().Result;

                    result = JsonConvert.DeserializeObject<BaseObjectResponse<OrderViewdetail>>(stringResult);
                    return result;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return null;
            }

        }
        public async Task<int> SendMailResetPassword(string email)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string>()
                {
                {"template_type","2" },
                {"email", email},
                };
                var data = JsonConvert.SerializeObject(j_param);
                var a = _configuration["DataBaseConfig:key_api:api_manual"];
                var token = EncodeHelpers.Encode(data, _configuration["DataBaseConfig:key_api:api_manual"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_Send_Email_Reset_Password;
                var response = await httpClient.PostAsync(url, request);


                if (response.IsSuccessStatusCode)
                {

                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return 1;
            }
        }
        public async Task<string> buildContractNo()
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                string private_token_key = _configuration["DataBaseConfig:key_api:api_manual"];
                // Mã hợp dồng
                JObject jsonObject = new JObject(
                   new JProperty("code_type", "3")
               );
                var j_param = new Dictionary<string, object>
                {
                    { "key",jsonObject}
                };
                var data_product = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data_product, private_token_key);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().Get_Order_no;
                var response = await httpClient.PostAsync(url, request);


                if (response.IsSuccessStatusCode)
                {
                    var text = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<buildContractNoViewModel>(text);
                    if (result.status == 0)
                    {
                        return result.code;
                    }
                    else {
                        LogHelper.InsertLogTelegram("apisever:" + result.msg);
                        return null;
                    }

                }

                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return null;
            }
        }
        public async Task<string> buildClientCode(string client_type)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                string private_token_key = _configuration["DataBaseConfig:key_api:api_manual"];
                // Mã hợp dồng
                JObject jsonObject = new JObject(
                     new JProperty("code_type", "8"),
                     new JProperty("client_type", client_type)
                 );
                var j_param = new Dictionary<string, object>
                {
                    { "key",jsonObject}
                };
                var data_product = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data_product, private_token_key);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().Get_Order_no;
                var response = await httpClient.PostAsync(url, request);


                if (response.IsSuccessStatusCode)
                {
                    var text = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<buildContractNoViewModel>(text);
                    if (result.status == 0)
                    {
                        return result.code;
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("apisever:" + result.msg);
                        return null;
                    }

                }

                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return null;
            }
        }
        public async Task<ObjectResponse<ProductCategoryViewModel>> GetProductCategory()
        {
            try
            {


                var url = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().Get_Product_Category_By_Parent_Id;


                HttpClient httpClient = new HttpClient();

                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", "OmYiIDZWKRUTOm4QBxV/dHY8") });
                var response = await httpClient.PostAsync(url, content);
                var contents = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ObjectResponse<ProductCategoryViewModel>>(contents);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return null;
            }

        }
        public async Task<int> SendMessage(string user_id_send, string module_type, string action_type, string Code, string link_redirect, string role_type, string service_code = "")
        {
            try
            {

                var user = await _userRepository.GetDetailUser(Convert.ToInt32(user_id_send));
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, object>
                {
                   {"user_name_send", user.Entity.FullName.ToString()}, //tên người gửi
                    {"user_id_send", user_id_send}, //id người gửi
                    {"code", Code}, // mã đối tượng gửi
                    {"link_redirect", link_redirect}, // Link mà khi người dùng click vào detail item notify sẽ chuyển sang đó
                    {"module_type", module_type}, // loại module thực thi luồng notify. Ví dụ: Đơn hàng, khách hàng.......
                    {"action_type", action_type}, // action thực hiện. Ví dụ: Duyệt, tạo mới, từ chối....
                    {"role_type", role_type}, // quyền mà sẽ gửi tới
                    {"service_code", service_code}// mã dịch vụ
                };
                var data_product = JsonConvert.SerializeObject(j_param);

                var token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:b2c"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().send_Message;
                var response = await httpClient.PostAsync(url, request);
                if (response.IsSuccessStatusCode)
                {
                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendMessage-apisever:" + ex.ToString());
                return 1;
            }
        }

        public async Task<NotifySummeryViewModel> GetListNotify(string user_id, int pageindex, int pagesize)
        {
            try
            {

                HttpClient httpClient = new HttpClient();
                NotifySummeryViewModel result = null;
                var j_param = new Dictionary<string, object>
                {
                       {"user_id", user_id},
                       {"pageindex", pageindex},
                       {"pagesize", pagesize}
                };
                var data_product = JsonConvert.SerializeObject(j_param);

                var token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:b2c"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().Notify_Get_List;
                var response = await httpClient.PostAsync(url, request);
                var stringResult = "";

                if (response.IsSuccessStatusCode)
                {
                    stringResult = response.Content.ReadAsStringAsync().Result;

                    var data = JsonConvert.DeserializeObject<NotifyRedisViewModel>(stringResult);
                    result = data.data;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListNotify:" + ex.ToString());
                return null;
            }
        }
        public async Task<int> UpdateNotify(string notify_id, string user_seen_id, string seen_status)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, object>
                {
                  /*  {"notify_id", "A1,A32"}, // 
                    {"user_seen_id", "222"},*/
                     {"notify_id", notify_id}, // 
                    {"user_seen_id", user_seen_id},
                    {"seen_status", seen_status}, // SEEN_ALL = 1: click vao chuông |    SEEN_DETAIL = 2  click vao item notify

                };
                var data = JsonConvert.SerializeObject(j_param);
                var a = _configuration["DataBaseConfig:key_api:b2c"];
                var token = EncodeHelpers.Encode(data, _configuration["DataBaseConfig:key_api:b2c"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().Notify_update_status;
                var response = await httpClient.PostAsync(url, request);


                if (response.IsSuccessStatusCode)
                {

                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return 1;
            }
        }
      

        public async Task<string> GetVietQRCode(string account_number, string bank_code, string order_no, double amount)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.vietqr.io/v2/generate");
                request.Headers.Add("x-client-id", "ba09d2bf-7f59-442f-8c26-49a8d48e78d7");
                request.Headers.Add("x-api-key", "a479a45c-47d5-41c1-9f83-990d65cd832a");
                var body = "{ \"accountNo\": \""
                    + account_number
                    + "\", \"accountName\": \"CTCP TM VA DV QUOC TE DAI VIET\", \"acqId\": \""
                    + (bank_code.Length > 6 ? bank_code.Substring(0, 6) : bank_code)
                    + "\", \"addInfo\": \""
                    + order_no
                    + " THANH TOAN\", \"amount\": \"" + Math.Round(amount, 0)
                    + "\", \"template\": \"compact\" }";
                var content = new StringContent(body, null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);

                return await response.Content.ReadAsStringAsync();

            }
            catch
            {

            }
            return null;
        }
        public async Task<List<VietQRBankDetail>> GetVietQRBankList()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.vietqr.io/v2/banks");
                var response = await client.SendAsync(request);

                var jsonData = JObject.Parse( await response.Content.ReadAsStringAsync());
                var status = int.Parse(jsonData["code"].ToString());
                if (status == (int)ResponseType.SUCCESS)
                {
                    return JsonConvert.DeserializeObject<List<VietQRBankDetail>>(jsonData["data"].ToString());
                }

            }
            catch
            {
                throw;
            }
            return null;
        }
        public  async Task<string> UploadImageQRBase64(string order_no, string amount, string ImageData,string type)
        {
            string key_token_api = "wVALy5t0tXEgId5yMDNg06OwqpElC9I0sxTtri4JAlXluGipo6kKhv2LoeGQnfnyQlC07veTxb7zVqDVKwLXzS7Ngjh1V3SxWz69";
            string ImagePath = string.Empty;
            string tokenData = string.Empty;
         
            try
            {
                var objimage = StringHelpers.GetImageSrcBase64Object(ImageData);
                var j_param = new Dictionary<string, string> {
                    { "order_no", order_no },
                    { "amount", amount },
                    { "type", type },
                    { "data_file", objimage.ImageData },  
                    { "extend", objimage.ImageExtension }
                };

                using (HttpClient httpClient = new HttpClient())
                {
                    tokenData = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                    var contentObj = new { token = tokenData };
                    var content = new StringContent(JsonConvert.SerializeObject(contentObj), Encoding.UTF8, "application/json");
                    var url = ReadFile.LoadConfig().IMAGE_DOMAIN + ReadFile.LoadConfig().upload_QR_payment_order;
                    var result = await httpClient.PostAsync(url, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    if (resultContent.status == 0)
                    {
                        return resultContent.url_path;
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("UploadImageQRBase64. Result: " + resultContent.status + ". Message: " + resultContent.msg);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UploadImageQRBase64 - " + ex.Message.ToString() + " Token:" + tokenData);
            }
            return ImagePath;
        }
    
    }
}
