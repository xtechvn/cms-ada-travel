using APP_CHECKOUT.RabitMQ;
using Caching.Elasticsearch;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.CustomerManager;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Mongo;
using Entities.ViewModels.Vinpearl;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
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
        private IDepartmentRepository _departmentRepository;
        private readonly WorkQueueClient _workQueueClient;
        public CustomerManagerManualController(IConfiguration configuration, ManagementUser managementUser, IAllCodeRepository allCodeRepository, IUserRepository userRepository,
            ICustomerManagerRepository customerManagerRepositories, IClientRepository clientRepository, IUserAgentRepository userAgentRepository, IDepartmentRepository departmentRepository)
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
            _departmentRepository = departmentRepository;
            _workQueueClient = new WorkQueueClient(configuration);
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
                            var listPermissions = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Utilities.Contants.SortOrder.THEM, (int)MenuId.QL_KHACH_HANG);
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
        public async Task<IActionResult> CustomerManagerDetail(int id)
        {
            try
            {
                var key_token_api = _configuration["DataBaseConfig:key_api:api_manual"];
                var AgencyType = _allCodeRepository.GetListByType("AGENCY_TYPE");
                var PermisionType = _allCodeRepository.GetListByType("PERMISION_TYPE");
                var ClientType = _allCodeRepository.GetListByType("CLIENT_TYPE");
                var UtmSource = _allCodeRepository.GetListByType(AllCodeType.CLIENT_SOURCE);
                ViewBag.AgencyType = AgencyType;
                ViewBag.PermisionType = PermisionType;
                ViewBag.ClientType = ClientType;
                ViewBag.UtmSource = UtmSource;

                if (id != 0)
                {
                    var model = await _clientRepository.GetClientDetailByClientId(id);
                    ViewBag.dataModel = model;
                    return PartialView();
                }
                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CustomerManagerDetail - CustomerManagerController: " + ex);
                return PartialView();
            }

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
                var detail_user = await _userRepository.GetById(_UserId);
                model.FullName = detail_user.FullName;
                model.UserName = detail_user.UserName;
                model.UserId = _UserId;
                var data = await _commentClientMongoService.InsertCommentClient(model);
                if (data > 0)
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
                            var listPermissions = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Utilities.Contants.SortOrder.TRUY_CAP, (int)MenuId.QL_KHACH_HANG);
                            var listPermissions6 = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Utilities.Contants.SortOrder.VIEW_ALL, (int)MenuId.QL_KHACH_HANG);
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
        public async Task<IActionResult> PopStatusClient(string Clientid)
        {
            try
            {
                ViewBag.ClientId = Clientid;
                var ClientType = _allCodeRepository.GetListByType(AllCodeType.CLIENT_STATUS);
                ViewBag.ClientStatus = ClientType;
                var data = await _customerManagerRepositories.GetDetailClient(Convert.ToInt64(Clientid));
                if (data != null)
                {
                    ViewBag.Status = data.Status;
                }
                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PopStatusClient - Comment: " + ex.ToString());

            }
            return PartialView();
        }
        public async Task<IActionResult> SetUpStatusClient(int Clientid, int Status, string Note)
        {
            try
            {
                var ClientType = _allCodeRepository.GetListByType(AllCodeType.CLIENT_STATUS);
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var model = new CommentClientMongoModel();
                var detail_user = await _userRepository.GetById(_UserId);
                model.FullName = detail_user.FullName;
                model.UserName = detail_user.UserName;
                model.UserId = _UserId;
                model.ClientId = Clientid.ToString();
                model.Note = "Cập nhật trạng thái khách hàng :" + ClientType.FirstOrDefault(s => s.CodeValue == Status).Description + ".Mô tả :" + Note;
                var InsertComment = await _commentClientMongoService.InsertCommentClient(model);
                var UpdateStatus = await _customerManagerRepositories.UpdateStatusClient(Status, Clientid);
                if (UpdateStatus > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Cập nhật trạng thái khách hàng thành công",
                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Cập nhật trạng thái khách hàng không thành công",
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PopStatusClient - Comment: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Cập nhật trạng thái khách hàng không thành công",
                });
            }
        }
        public IActionResult Source()
        {
            var data = _allCodeRepository.GetAll();
            ViewBag.data = data.GroupBy(s => s.Type).Select(i => i.First()).ToList();
            return View();
        }
        public async Task<IActionResult> AddOrUpdateSource(int id)
        {
            ViewBag.id = id;
            if (id != 0)
            {
                var data = await _allCodeRepository.GetById(id);
                return View(data);
            }
            return View();
        }
        public async Task<IActionResult> SearchSource(string type)
        {
            var data_model = new List<SumContractPayByUtmSource>();
            var data = _allCodeRepository.GetListByType(type);
            var sum = await _customerManagerRepositories.GetSumContractPayByUtmSource();
            if (data != null && data.Count > 0)
            {
                foreach (var item in data)
                {
                    var data_detail = new SumContractPayByUtmSource();
                    data_detail.CodeValue = item.CodeValue;
                    data_detail.Description = item.Description;
                    var sum_detai = sum.FirstOrDefault(s => s.UtmSource == item.CodeValue);
                    data_detail.TotalAmount = sum_detai != null ? sum_detai.TotalAmount : 0;
                    data_model.Add(data_detail);
                }
            }

            return View(data_model);
        }
        public IActionResult ListClientSource(int Id)
        {
            ViewBag.id = Id;
            var CLIENT_SOURCE = _allCodeRepository.GetListByType(AllCodeType.CLIENT_SOURCE);
            var detail = CLIENT_SOURCE.FirstOrDefault(s => s.CodeValue == Id);
            ViewBag.Description = detail.Description;
            return View();
        }
        public async Task<IActionResult> ListClientBySource(CustomerManagerViewSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<CustomerManagerViewModel>();

            try
            {
                currentPage = searchModel.PageIndex;
                pageSize = searchModel.PageSize;
                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null)
                {
                    var i = 0;
                    if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                    {
                        var list = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                        foreach (var item in list)
                        {
                            //kiểm tra chức năng có đc phép sử dụng
                            var listPermissions = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Utilities.Contants.SortOrder.TRUY_CAP, (int)MenuId.XEP_HANG);
                            var listPermissions6 = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Utilities.Contants.SortOrder.VIEW_ALL, (int)MenuId.XEP_HANG);
                            if (listPermissions == true)
                            {
                                searchModel.SalerPermission = current_user.UserUnderList + "," + current_user.Id.ToString(); i++;
                            }
                            if (listPermissions6 == true || item == (int)RoleType.Admin)
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
        public IActionResult ImportWSExcel(int type)
        {
            ViewBag.type = type;
            return PartialView();

        }
        [HttpPost]
        public async Task<IActionResult> ImportWSExcelListing(IFormFile file)
        {

            try
            {
                ViewBag.DepartmentList = await _departmentRepository.GetAll(String.Empty);
                ViewBag.data = _allCodeRepository.GetListByType(AllCodeType.CLIENT_SOURCE);
                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                var data_list = new List<ClientExcelImportModel>();

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.FirstOrDefault();

                    var endRow = ws.Cells.End.Row;
                    var startRow = 2;

                    for (int row = startRow; row <= endRow; row++)
                    {
                        var cellRange = ws.Cells[row, 1, row, 13];
                        var isRowEmpty = cellRange.All(c => c.Value == null);
                        if (isRowEmpty)
                        {
                            break;
                        }
                        var ClientName = ws.Cells[row, 1].Value;
                        var Email = ws.Cells[row, 2].Value;
                        var Phone = ws.Cells[row, 3].Value;
                        var Note = ws.Cells[row, 4].Value;

                        var data = new ClientExcelImportModel
                        {
                            Client_name = (ClientName == null ? "" : ClientName.ToString()),
                            email = (Email == null ? "" : Email.ToString()),
                            phone = (Phone == null ? "" : Phone.ToString()),
                            Note = (Note == null ? "" : Note.ToString()),

                        };
                        data_list.Add(data);
                    }


                }

                ViewBag.CheckedAll = true;
                return PartialView(data_list);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ImportWSExcelListing - OrderController: " + ex.ToString());
                return PartialView();
            }
        }
        [HttpPost]
        public async Task<IActionResult> ImportWSExcelUpload(string model)
        {

            try
            {
                if (model != null && model != "")
                {
                    List<ClientExcelImportModel> success_model = new List<ClientExcelImportModel>();
                    List<ClientExcelImportModel> summit_model = new List<ClientExcelImportModel>();
                    List<ClientExcelImportModel> err_model = new List<ClientExcelImportModel>();
                    try
                    {
                        success_model = JsonConvert.DeserializeObject<List<ClientExcelImportModel>>(model);
                    }
                    catch
                    {
                        return PartialView(success_model);
                    }
                    foreach (var item in success_model)
                    {
                        var list_client = await _customerManagerRepositories.GetClientByPhone(item.phone);
                        if (item.phone == null || item.phone == "" || (list_client != null && list_client.Count > 0))
                        {
                            err_model.Add(item);
                            continue;
                        }
                        APIService apiService = new APIService(_configuration, _userRepository);
                        var ClientCode = await apiService.buildClientCode(item.id_ClientType.ToString());
                        var model_client = new CustomerManagerView();
                        model_client.Id = 0;
                        model_client.UserId = item.UserId;
                        model_client.AgencyType = item.AgencyType;
                        model_client.phone = item.phone;
                        model_client.email = item.email;
                        model_client.Client_name = item.Client_name;
                        model_client.PermisionType = Convert.ToInt32(item.id_nhomkhach);
                        model_client.id_loaikhach = item.id_loaikhach;
                        model_client.id_nhomkhach = item.id_nhomkhach;
                        model_client.id_ClientType = item.id_ClientType;
                        model_client.ClientCode = ClientCode;
                        model_client.UtmSource = item.UtmSource;
                        model_client.JoinDate = DateTime.Now;

                        var Result = await _customerManagerRepositories.CreateClient(model_client);

                        if (Result > 0)
                        {
                            summit_model.Add(item);
                            var clientdetail = await _clientRepository.GetClientByClientCode(item.ClientCode);
                            _workQueueClient.SyncES(clientdetail.Id, _configuration["DataBaseConfig:Elastic:SP:sp_GetClient"], _configuration["DataBaseConfig:Elastic:Index:Client"], ProjectType.ADAVIGO_CMS, "Setup CustomerManager");

                        }

                    }
                    ViewBag.success_count = summit_model.Count;
                    return PartialView(err_model);
                }
                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ImportWSExcelUpload - OrderController: " + ex.ToString());
                return PartialView();
            }
        }
        public async Task<IActionResult> GetListUserByDepartmentId(int DepartmentId)
        {
            try
            {
                List<int?> deparments = new List<int?>();
                deparments.Add(DepartmentId);
                var list_user_ids = await _userRepository.GetListUserDepartById(deparments);
                list_user_ids = list_user_ids.Where(s => s.UserPositionId == UserPositionType.TN && s.Status == (int)StatusType.BINH_THUONG).ToList();
                if (list_user_ids == null || list_user_ids.Count == 0)
                {

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = list_user_ids,

                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = list_user_ids,

                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListUserByDepartmentId - CustomerManagerManualController: " + ex.ToString());
            }
            return Ok(new
            {
                status = (int)ResponseType.SUCCESS,
                data = new List<User>(),

            });

        }
    }
}
