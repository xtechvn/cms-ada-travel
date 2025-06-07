using APP_CHECKOUT.RabitMQ;
using Entities.ViewModels.CustomerManager;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
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
    }
}
