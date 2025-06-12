using APP_CHECKOUT.RabitMQ;
using Caching.Elasticsearch;
using Entities.ViewModels.CustomerManager;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Mongo;
using Entities.ViewModels.Vinpearl;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Service;

namespace WEB.CMS.Controllers.CustomerManager
{
    public class CustomerManagerManualController : Controller
    {
        private readonly IConfiguration _configuration;
        private ManagementUser _ManagementUser;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerManagerRepository _customerManagerRepositories;
        private readonly IClientRepository _clientRepository;
        private readonly IUserAgentRepository _userAgentRepository;
        private LogCacheFilterMongoService _logCacheFilterMongoService;
        private UserESRepository _userESRepository;
        private CommentClientMongoService _commentClientMongoService;
        public CustomerManagerManualController(IConfiguration configuration, ManagementUser managementUser, IAllCodeRepository allCodeRepository, IUserRepository userRepository,
            ICustomerManagerRepository customerManagerRepositories, IClientRepository clientRepository, IUserAgentRepository userAgentRepository)
        {
            _configuration = configuration;
            _ManagementUser = managementUser;
            _allCodeRepository = allCodeRepository;
            _userRepository = userRepository;
            _customerManagerRepositories = customerManagerRepositories;
            _clientRepository = clientRepository;
            _userAgentRepository = userAgentRepository;
            _logCacheFilterMongoService = new LogCacheFilterMongoService(configuration);
            _userESRepository = new UserESRepository(_configuration["DataBaseConfig:Elastic:Host"]);
            _commentClientMongoService = new CommentClientMongoService(_configuration);
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var key_token_api = _configuration["DataBaseConfig:key_api:api_manual"];
                //string ApiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_ALLCODE;
                var AgencyType = _allCodeRepository.GetListByType(AllCodeType.AGENCY_TYPE);
                var PermisionType = _allCodeRepository.GetListByType(AllCodeType.PERMISION_TYPE);
                var ClientType = _allCodeRepository.GetListByType(AllCodeType.CLIENT_TYPE);
                ViewBag.AgencyType = AgencyType;
                ViewBag.PermisionType = PermisionType;
                ViewBag.ClientType = ClientType;

                var current_user = _ManagementUser.GetCurrentUser();
                ViewBag.buttomThem = 0;
                if (current_user != null)
                {
                    var i = 0;
                    if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                    {
                        var list = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                        foreach (var item in list)
                        {
                            //kiểm tra chức năng có đc phép sử dụng
                            var listPermissions = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)SortOrder.THEM, (int)MenuId.QL_KHACH_HANG);
                            if (listPermissions == true)
                            {
                                ViewBag.buttomThem = 1;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Index - CustomerManagerController: " + ex);
            }

            return View();
        }
        public async Task<IActionResult> Detail(long id)
        {
            try
            {
                var model = await _clientRepository.GetClientDetailByClientId(id);

                if (model != null && model.ClientType != ClientType.kl)
                {
                    ViewBag.btnStatus = 1;
                }
                ViewBag.id = id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Detail - CustomerManagerController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> DetailCustomerManager(long id)
        {

            try
            {
                var Amount = await _customerManagerRepositories.GetAmountRemainOfContractByClientId(id);
                var data = await _customerManagerRepositories.GetDetailClient(id);
                var model = _userAgentRepository.UserAgentByClient((int)id, 0);
                ViewBag.userAgent = model;
                if (Amount != null) { ViewBag.Amount = Amount.AmountRemain; }
                else
                {
                    ViewBag.Amount = 0;
                }



                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DetailCustomerManager - CustomerManagerController: " + ex);
                return PartialView();
            }
        }

        public async Task<IActionResult> InsertLogCache(CustomerManagerViewSearchModel searchModel)
        {

            int status = (int)ResponseType.FAILED;
            string msg = "Lỗi kỹ thuật vui lòng liên hệ bộ phận IT";
            try
            {

                var Insert = await _logCacheFilterMongoService.InsertLogCache(searchModel);
                if (Insert > 0)
                {
                    status = (int)ResponseType.SUCCESS;
                    msg = "Lưu thành công thành công";
                }
                else
                {
                    status = (int)ResponseType.SUCCESS;
                    msg = "Lưu không công thành công";
                }


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertLogCache - CustomerManagerController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Lỗi kỹ thuật vui lòng liên hệ bộ phận IT";
            }
            return Ok(new
            {
                status = status,
                msg = msg,

            });
        }
        public async Task<string> GetSuggestionUserCache(string txt_search)
        {
            try
            {

                var data = _logCacheFilterMongoService.GetListLogCache(txt_search, null);
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSuggestionUserCache - CustomerManagerController: " + ex);
                return null;
            }
        }
        public async Task<IActionResult> DetailUserAgent(int user_Id, long client)
        {

            try
            {
                ViewBag.client = client;
                if (user_Id != 0)
                {
                    var model = _userAgentRepository.UserAgentByClient(0, user_Id);
                    if (model != null && model.Count > 0)
                        return PartialView(model[0]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DetailUserAgent - PaymentAccountController: " + ex);
            }
            return PartialView();
        }
        public IActionResult UpdatalUserAgent(int id, int userId, long clientId)
        {

            try
            {

                var create_id = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var model = _userAgentRepository.UpdataUserAgent(id, userId, create_id, clientId);
                if (model > 0)
                {
                    return Ok(new
                    {
                        stt_code = (int)ResponseType.SUCCESS,
                        msg = "Đổi nhân viên thành công",

                    });
                }
                else
                {

                    return Ok(new
                    {
                        stt_code = (int)ResponseType.FAILED,
                        msg = "Đổi nhân viên không thành công",

                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatalUserAgent - PaymentAccountController: " + ex);
            }
            return Ok(new
            {
                stt_code = (int)ResponseType.FAILED,
                msg = "Đổi nhân viên không thành công",

            });
        }
        public async Task<IActionResult> UserSuggestion(string txt_search, int service_type = 0)
        {

            try
            {
                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (txt_search == null) txt_search = "";
                var data = await _userESRepository.GetUserSuggesstion(txt_search);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = data,
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserSuggestion - CustomerManagerController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<CustomerESViewModel>()
                });
            }

        }
        public async Task<IActionResult> InsertComment(CommentClientMongoModel model)
        {

            try
            {
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
              var detail_user= await _userRepository.GetById(_UserId);
                model.FullName = detail_user.FullName;
                model.UserName = detail_user.UserName;
                model.UserId = _UserId;
                var data =await _commentClientMongoService.InsertCommentClient(model);
                if(data > 0)
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Thêm comment thành công",
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Comment - CustomerManagerController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Thêm comment không thành công",
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Thêm comment không thành công",
            });
        }
        public async Task<IActionResult> CommentClient(string Clientid)
        {

            try
            {
                ViewBag.ClientId = Clientid;
                var model = new CommentClientMongoModel();
                model.ClientId = Clientid;
                var data = _commentClientMongoService.GetListComment(model);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Comment - Comment: " + ex.ToString());

            }
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> ListClient(CustomerManagerViewSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<CustomerManagerViewModel>();

            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null)
                {
                    var i = 0;
                    if (searchModel.CacheName != null)
                    {
                        var data = _logCacheFilterMongoService.GetListLogCache(null, searchModel.CacheName);
                        if (data != null)
                        {
                            searchModel.MaKH = searchModel.MaKH == -1 ? data[0].MaKH : searchModel.MaKH;
                            searchModel.CreatedBy = searchModel.CreatedBy == -1 ? data[0].CreatedBy : searchModel.CreatedBy;
                            searchModel.UserId = searchModel.UserId == -1 ? data[0].UserId : searchModel.UserId;
                            searchModel.TenKH = searchModel.TenKH == null ? data[0].TenKH : searchModel.TenKH;
                            searchModel.Email = searchModel.Email == null ? data[0].Email : searchModel.Email;
                            searchModel.Phone = searchModel.Phone == null ? data[0].Phone : searchModel.Phone;
                            searchModel.AgencyType = searchModel.AgencyType == -1 ? data[0].AgencyType : searchModel.AgencyType;
                            searchModel.ClientType = searchModel.ClientType == -1 ? data[0].ClientType : searchModel.ClientType;
                            searchModel.PermissionType = searchModel.PermissionType == -1 ? data[0].PermissionType : searchModel.PermissionType;
                            searchModel.CreateDate = searchModel.CreateDate == null ? data[0].CreateDate : searchModel.CreateDate;
                            searchModel.EndDate = searchModel.EndDate == null ? data[0].EndDate : searchModel.EndDate;
                            searchModel.MinAmount = searchModel.MinAmount == -1 ? data[0].MinAmount : searchModel.MinAmount;
                            searchModel.MaxAmount = searchModel.MaxAmount == -1 ? data[0].MaxAmount : searchModel.MaxAmount;

                        }

                    }
                    if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                    {
                        var list = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                        foreach (var item in list)
                        {
                            //kiểm tra chức năng có đc phép sử dụng
                            var listPermissions = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)SortOrder.TRUY_CAP, (int)MenuId.QL_KHACH_HANG);
                            var listPermissions6 = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)SortOrder.VIEW_ALL, (int)MenuId.QL_KHACH_HANG);
                            if (listPermissions == true)
                            {
                                searchModel.SalerPermission = current_user.Id.ToString(); i++;
                            }
                            if (listPermissions6 == true)
                            {
                                searchModel.SalerPermission = current_user.UserUnderList;
                                i++;
                            }
                            if (item == (int)RoleType.Admin)
                            {
                                searchModel.SalerPermission = null;
                                i++;
                            }
                        }
                    }
                    if (i != 0)
                    {
                        model = await _customerManagerRepositories.GetPagingList(searchModel, currentPage, pageSize);
                    }

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListClient - CustomerManagerController: " + ex);
            }

            return PartialView(model);
        }
    }
}
