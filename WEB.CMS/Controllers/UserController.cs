﻿using APP_CHECKOUT.RabitMQ;
using Caching.Elasticsearch;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IUserRepository _UserRepository;
        private readonly UserESRepository _userESRepository;
        private readonly IDepartmentRepository _DepartmentRepository;
        private readonly IRoleRepository _RoleRepository;
        private readonly IMFARepository _mFARepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ManagementUser _ManagementUser;
        private readonly APIService _aPIService;
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient _workQueueClient;

        public UserController(IUserRepository userRepository, IRoleRepository roleRepository,
            IWebHostEnvironment hostEnvironment, IMFARepository mFARepository,
            IDepartmentRepository departmentRepository, ManagementUser managementUser, IOrderRepository orderRepository, IConfiguration configuration)
        {
            _UserRepository = userRepository;
            _RoleRepository = roleRepository;
            _WebHostEnvironment = hostEnvironment;
            _mFARepository = mFARepository;
            _DepartmentRepository = departmentRepository;
            _ManagementUser = managementUser;
            _orderRepository = orderRepository;
            _configuration = configuration;
            _aPIService = new APIService(configuration, userRepository);
            _workQueueClient = new WorkQueueClient(configuration);
            _userESRepository = new UserESRepository(_configuration["DataBaseConfig:Elastic:Host"]);
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> GetUserSuggestionList(string name)
        {
            try
            {
                var userlist = await _userESRepository.GetUserSuggesstion(name);
                var suggestionlist = userlist.Select(s => new
                {
                    id = s.id,
                    name = s.username,
                    fullname = s.fullname,
                    email = s.email
                }).ToList();
                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserSuggestionList - UserController: " + ex);
                return null;
            }
        }

        [HttpPost]
        public IActionResult Search(string userName, int? status, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<UserGridModel>();
            try
            {
                if (status < 0 || status>2) status = null;
                if (userName !=null && userName.Trim()!="") userName = CommonHelper.RemoveUnicode(userName);
                model = _UserRepository.GetPagingList(userName, status, currentPage, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - UserController: " + ex);
            }
            return PartialView(model);
        }

        public async Task<IActionResult> AddOrUpdate(int Id, bool IsClone = false)
        {
            try
            {
                var model = new User();
                ViewBag.UserRoleList = null;
                ViewBag.CompanyType = "";
                if (Id != 0)
                {

                    model = await _UserRepository.FindById(Id);
                    if (IsClone)
                    {
                        model = new User
                        {
                            FullName = model.FullName,
                            UserName = model.UserName,
                            Email = model.Email,
                            Address = model.Address,
                            BirthDay = model.BirthDay,
                            Gender = model.Gender,
                            Status = model.Status,
                            Note = model.Note,
                            DepartmentId = model.DepartmentId,
                            Phone = model.Phone,
                        };
                    }
                    var list_role_active = await _UserRepository.GetUserActiveRoleList(model.Id);
                    if (list_role_active != null && list_role_active.Count > 0)
                    {
                        ViewBag.UserRoleList = list_role_active.Select(x => x.Id).ToList();
                    }
                    var user = await _aPIService.GetByUserDetail(model.Id, model.UserName, model.Email);
                    ViewBag.CompanyType = user != null && user.Id > 0 ? user.CompanyType : "";
                }
                else
                {
                    model.Gender = 1;
                }

                ViewBag.DepartmentList = await _DepartmentRepository.GetAll(String.Empty);
                ViewBag.RoleList = await _RoleRepository.GetAll();
                ViewBag.UserPosition = _UserRepository.GetUserPositions();
                return View(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdate - UserController: " + ex);
                return Content("");
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpSert(IFormFile imagefile, UserViewModel model)
        {
            try
            {
                string imageUrl = string.Empty;
                if (imagefile != null)
                {
                    string _FileName = Guid.NewGuid() + Path.GetExtension(imagefile.FileName);
                    string _UploadFolder = @"uploads/images";
                    string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                    if (!Directory.Exists(_UploadDirectory))
                    {
                        Directory.CreateDirectory(_UploadDirectory);
                    }

                    string filePath = Path.Combine(_UploadDirectory, _FileName);

                    if (!System.IO.File.Exists(filePath))
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagefile.CopyToAsync(fileStream);
                        }
                    }
                    model.Avata = "/" + _UploadFolder + "/" + _FileName;
                }

                int rs = 0;
                if (model.UserPositionId != null && model.UserPositionId > 0)
                {
                    var active_position = await _UserRepository.GetUserPositionsByID((int)model.UserPositionId);
                    if (active_position != null) model.Level = active_position.Rank;
                }
                if (model.CompanyType == null || model.CompanyType.Trim() == "")
                {
                    model.CompanyType = _configuration["CompanyType"];
                }
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                model.CreatedBy = _UserLogin;
                model.ModifiedBy = _UserLogin;
                if (model.Phone == null) model.Phone = "";
                if (model.Avata == null) model.Avata = "";
                if (model.Address == null) model.Address = "";
                if (model.NickName == null) model.NickName = "";
                //-- check exists:
                var exists_detail = await _aPIService.GetByUserDetail(model.Id <= 0 ? -1 : model.Id, model.UserName, model.Email);
                if (exists_detail != null && exists_detail.Id > 0)
                {
                    model.Id = exists_detail.Id;
                    var combine_company_type = (exists_detail.CompanyType + "," + model.CompanyType).Split(",").Where(x => x != null && x.Trim() != "").Distinct();
                    model.CompanyType = string.Join(",", combine_company_type);
                    model.CreatedBy = exists_detail.CreatedBy;
                    model.CreatedOn = exists_detail.CreatedOn;
                }
                //-- Update dbUser:
                var success = await _aPIService.UpdateUser(model);
                if (success > 0)
                {
                    var exists = await _UserRepository.GetById(model.Id);
                    if (exists == null || exists.Id <= 0)
                    {
                        rs = await _UserRepository.Create(model);

                    }
                    else
                    {
                        rs = await _UserRepository.Update(model);

                    }
                }
                else
                {
                    rs = -2;
                }
                /*
              if (model.Id != 0)
              {
                  rs = await _UserRepository.Update(model);
              }
              else
              {
                  rs = await _UserRepository.Create(model);
              }
             */
                if (rs > 0)
                {
                    _workQueueClient.SyncES(rs, _configuration["DataBaseConfig:Elastic:SP:sp_GetUser"], _configuration["DataBaseConfig:Elastic:Index:User"], ProjectType.ADAVIGO_CMS, "UpSert UserController");

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tên đăng nhập hoặc email đã tồn tại"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpSert - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDetail(int Id)
        {
            var model = new UserDataViewModel();
            try
            {
                model = await _UserRepository.GetUser(Id);
                var mfa_record = await _mFARepository.get_MFA_DetailByUserID(Id);
                ViewBag.RoleList = await _RoleRepository.GetAll();
                model.UserPositionName = model.UserPositionId != null && model.UserPositionId > 0 ? _UserRepository.GetUserPositionsByID((int)model.UserPositionId).Result.Name : "";
                ViewBag.IsMFAEnabled = (mfa_record != null && mfa_record.UserId == Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - UserController: " + ex);
                ViewBag.IsMFAEnabled = false;
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(int userId, int[] arrRole)
        {
            try
            {
                var rs = await _UserRepository.UpdateUserRole(userId, arrRole);

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateUserRole - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUserRole(int userId, int[] arrRole)
        {
            try
            {
                var rs = await _UserRepository.DeleteUserRole(userId, arrRole);

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RemoveUserRole - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserStatus(int id)
        {
            try
            {
                var rs = await _UserRepository.ChangeUserStatus(id);
                if (rs != -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        status = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeUserStatus - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordByUserId(int userId)
        {
            try
            {
                var rs = await _UserRepository.ResetPasswordByUserId(userId);
                var current_user = await _UserRepository.GetById(userId);
                var user = await _aPIService.GetByUserDetail(current_user.Id, current_user.UserName, current_user.Email);
                var result_2 = await _aPIService.ChangePassword(current_user.UserName, current_user.Password);
                if (result_2 > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        result = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetPasswordByUserId - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> UserProfile()
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                var model = await _UserRepository.GetUser(current_user.Id);
                ViewBag.RoleList = await _RoleRepository.GetRoleListByUserId(current_user.Id);
                ViewBag.UserPosition = _UserRepository.GetUserPositions();
                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserProfile - UserController: " + ex);
                return Content("");
            }

        }

        public IActionResult UserChangePass()
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                var model = new UserDataViewModel()
                {
                    Id = current_user.Id
                };
                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserChangePass - UserController: " + ex);
                return Content("");
            }

        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserPasswordModel model)
        {
            try
            {
                var rs = await _UserRepository.ChangePassword(model);
                var current_user = await _UserRepository.GetById(model.Id);
                var user = await _aPIService.GetByUserDetail(current_user.Id, current_user.UserName, current_user.Email);
                var NewPassword = EncodeHelpers.MD5Hash(model.NewPassword);
                var result_2 = _aPIService.ChangePassword(current_user.UserName, NewPassword);
                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        result = rs
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Mật khẩu hiện tại không chính xác",
                        result = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangePassword - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ResetMFA(long id)
        {
            try
            {
                var detail = await _mFARepository.get_MFA_DetailByUserID(id);
                detail.Status = 0;
                var rs = await _mFARepository.UpdateAsync(detail);
                var user_local = await _UserRepository.GetById(detail.UserId);
                var user = await _aPIService.GetByUserDetail(user_local.Id, user_local.UserName, user_local.Email);

                if (rs)
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                    });
                else
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại",
                    });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetMFA - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString(),
                });
            }

        }
        [HttpPost]
        public async Task<IActionResult> UpdateAllOrderDetail()
        {
            try
            {
                var order = _orderRepository.GetAllOrderIDs();
                if (order != null && order.Count > 0)
                {
                    var _UserLogin = 0;
                    if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                    {
                        _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    }

                    foreach (var id in order)
                    {
                        await _orderRepository.UpdateOrderDetail(id, _UserLogin);
                    }
                }
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    message = "Update Order Success",
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAllOrderDetail - UserController: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    message = ex.Message.ToString(),
                });
            }

        }
        public async Task<IActionResult> ViewConfirm(long id)
        {
            try
            {
                ViewBag.id = id;

                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ViewConfirm - UserController: " + ex);
                return PartialView();
            }

        }
        public async Task<IActionResult> ConfirmPassQr(long id, string pass)
        {
            try
            {

                var user = await _UserRepository.GetById(id);
                string passqr = user.UserName + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.ToString("dd");
                if (pass == passqr)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Mật khẩu chính xác"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "mật khẩu không đúng"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ConfirmPassQr - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Đã sẩy ra lỗi vui lòng liên hệ IT"
                });
            }

        }
        public async Task<IActionResult> QrCodeUser(long id)
        {
            try
            {
                var mfa_record = new Mfauser();
                var model = await _UserRepository.GetById(id);
                APIService apiService = new APIService(_configuration, _UserRepository);
                string enviroment = _configuration["Config:OTP_Enviroment"];
                if (enviroment == null) enviroment = "";

                mfa_record.Username = model.UserName;
                mfa_record.Id = model.Id;
                var data = await apiService.GenQrCode(model.UserName);
                if (data != null)
                {
                    ViewBag.manual_entry_key = data.manual_entry_key;
                    mfa_record.SecretKey = data.manual_entry_key;
                }
                ViewBag.key = MFAService.GenerateQRCode(mfa_record, enviroment);
                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QrCodeUser - UserController: " + ex);
                return Content("");
            }

        }

    }
}