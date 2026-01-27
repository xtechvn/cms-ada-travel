
using APP_CHECKOUT.RabitMQ;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Funding;
using Entities.ViewModels.Hotel;
using Entities.ViewModels.SupplierConfig;
using ENTITIES.ViewModels.ElasticSearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.Adavigo.CMS.Controllers
{
    [CustomAuthorize]
    public class HotelController : Controller
    {

        private readonly IPolicyRepository _PolicyRepository;
        private readonly IAllCodeRepository _AllCodeRepository;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly IHotelBookingRepositories _hotelBookingRepositories;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IHotelRepository _HotelRepository;
        private readonly ProgramsESRepository _programsESRepository;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;
        private readonly WorkQueueClient _workQueueClient;
        private readonly TTLockCacheService _ttlockService;
        public HotelController(IPolicyRepository policyRepository, IAllCodeRepository allCodeRepository,
        ISupplierRepository supplierRepository, ICampaignRepository campaignRepository, IBrandRepository brandRepository, TTLockCacheService ttlockService,
        IHotelBookingRepositories hotelBookingRepositories, ICommonRepository commonRepository, IHotelRepository hotelRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _PolicyRepository = policyRepository;
            _AllCodeRepository = allCodeRepository;
            _CampaignRepository = campaignRepository;
            _supplierRepository = supplierRepository;
            _hotelBookingRepositories = hotelBookingRepositories;
            _commonRepository = commonRepository;
            _brandRepository = brandRepository;
            _HotelRepository = hotelRepository;
            _ttlockService = ttlockService;
            _programsESRepository = new ProgramsESRepository(configuration["DataBaseConfig:Elastic:Host"]);
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            _workQueueClient = new WorkQueueClient(configuration);
        }

        #region hotel management
        public async Task<IActionResult> Index()
        {
            ViewBag.Provinces = await _commonRepository.GetProvinceList();
            ViewBag.Brands = await _brandRepository.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult Search(HotelFilterModel searchModel)
        {
            var model = new GenericViewModel<HotelGridModel>();
            try
            {
                ViewBag.TypeFilter = searchModel.TypeFilter;
                var datas = _HotelRepository.GetHotelPagingList(searchModel);
                model.CurrentPage = searchModel.PageIndex;
                model.ListData = datas.ToList();
                model.PageSize = searchModel.PageSize;
                model.TotalRecord = datas != null && datas.Any() ? datas.First().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - HotelController: " + ex);
            }
            return PartialView(model);
        }


        public async Task<IActionResult> AddOrUpdate(int id)
        {
            var model = new HotelUpsertViewModel()
            {
                IsDisplayWebsite = false
            };

            if (id > 0)
            {
                var hotelPosition =  _HotelRepository.GetListHotelPosition(id);
                
                var hotePositionB2B = hotelPosition != null && hotelPosition.Count > 0 ? hotelPosition.Where(S => S.PositionType == Utilities.Contants.PositionType.B2B).ToList() : null;
                var hotePositionB2C = hotelPosition != null && hotelPosition.Count > 0 ? hotelPosition.Where(S => S.PositionType == Utilities.Contants.PositionType.B2C).ToList() : null;
                var PositionBanChay = hotelPosition != null && hotelPosition.Count > 0 ? hotelPosition.Where(S => S.PositionType == Utilities.Contants.PositionType.BANCHAY).ToList() : null;
                var hotel = _HotelRepository.GetHotelById(id);
                //var HotelPosition = _HotelRepository.GetListHotelPosition(id);

                if (hotel != null)
                {
                    model = new HotelUpsertViewModel()
                    {
                        Id = hotel.Id,
                        Name = hotel.Name,
                        ShortName = hotel.ShortName,
                        Description = hotel.Description,
                        SupplierId = hotel.SupplierId,
                        RatingStar = hotel.RatingStar,
                        VerifyDate = hotel.VerifyDate,
                        ProvinceId = hotel.ProvinceId,
                        Street = hotel.Street,
                        ChainBrands = hotel.ChainBrands,
                        Email = hotel.Email,
                        EstablishedYear = hotel.EstablishedYear,
                        Telephone = hotel.Telephone,
                        TaxCode = hotel.TaxCode,
                        HotelId = hotel.HotelId,
                        HotelType = hotel.HotelType,
                        SalerId = hotel.SalerId,
                        IsDisplayWebsite = hotel.IsDisplayWebsite ?? false,
                        ImageThumb = hotel.ImageThumb,
                        City = hotel.City,
                        IsCommitFund = hotel.IsCommitFund,
                        IsApartment = hotel.IsApartment,
                        IsFlashSale = hotel.IsFlashSale,


                        Star = hotel.Star,
                        PositionB2B = hotePositionB2B != null && hotePositionB2B.Count > 0 ? (int)hotePositionB2B[0].Position : 0,
                        PositionB2C = hotePositionB2C != null && hotePositionB2C.Count > 0 ? (int)hotePositionB2C[0].Position : 0,
                        PositionBanChay = PositionBanChay != null && PositionBanChay.Count > 0 ? (int)PositionBanChay[0].Position : 0,
                    };
                }
            }


            if (model.SupplierId != null && model.SupplierId > 0)
            {
                var supplier_model = await _commonRepository.GetSupplierById((int)model.SupplierId);
                if (supplier_model != null) model.SupplierName = supplier_model.FullName;
            }
            if (model.ProvinceId == null && model.ProvinceId > 0)
            {
                ViewBag.District = await _commonRepository.GetDistrictList(model.ProvinceId.ToString());
            }
            ViewBag.Provinces = await _commonRepository.GetProvinceList();
            ViewBag.Brands = await _brandRepository.GetAll();
            ViewBag.VerifyTimeList = _AllCodeRepository.GetListByType("EXPIRE_TIME_VERIFY_TYPE");

            return PartialView(model);
        }

        [HttpPost]
        public IActionResult Save(HotelUpsertViewModel model)
        {
            try
            {
                var result = _HotelRepository.SaveHotel(model);
                int db_index = Convert.ToInt32(_configuration["Redis:Database:db_search_result"]);
               
                if(model.PositionBanChay > 0)
                {
                    string cache_name_Ban_Chay = CacheName.HotelExclusiveList_BAN_CHAY_POSITION;
                    _redisService.DeleteCacheByKeyword(cache_name_Ban_Chay, db_index);
                }
                if (result > 0)
                {
                   
                    string cache_name = CacheName.HotelExclusiveListB2C_POSITION + model.City.Trim();
                    _redisService.DeleteCacheByKeyword(cache_name, db_index);
                    _workQueueClient.SyncES(result, _configuration["DataBaseConfig:Elastic:SP:sp_GetHotel"], _configuration["DataBaseConfig:Elastic:Index:Hotel"], ProjectType.ADAVIGO_CMS, "SetUpHotel HotelController");

                        return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật khách sạn thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật khách sạn thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region hotel detail
        public IActionResult Detail(int id)
        {
            var hotel = _HotelRepository.GetHotelDetailById(id);

            ViewBag.HotelUtilities = _AllCodeRepository.GetListByType("HOTEL_UTILITIES");
            return View(hotel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSurchargeNote(string body, int hotel_id)
        {
            try
            {
                if (body == null)
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lưu thông tin thất bại"
                    });
                var result = await _HotelRepository.UpdateHotelSurchargeNote(body, hotel_id);
                return new JsonResult(new
                {
                    isSuccess = result,
                    message = result ? "Lưu thông tin thành công" : "Lưu thông tin thất bại"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateSurchargeNote: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #region BankingAccount

        [HttpPost]
        public IActionResult BankingAccountListing(int hotel_id)
        {
            var model = new GenericViewModel<HotelBankingAccountGridModel>();
            try
            {
                var datas = _HotelRepository.GetHotelBankingAccountList(hotel_id);
                model.CurrentPage = 1;
                model.ListData = datas.ToList();
                model.PageSize = 20;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / 20);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - BankingAccountListing: " + ex);
            }
            return PartialView(model);
        }

        public IActionResult BankingAccountUpsert(int id, int hotel_id)
        {
            var model = new HotelBankingAccount()
            {
                HotelId = hotel_id
            };

            try
            {
                if (id > 0) model = _HotelRepository.GetHotelBankingAccountById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - PaymentUpsert: " + ex);
            }
            return PartialView(model);
        }

        [HttpPost]
        public IActionResult BankingAccountUpsert(HotelBankingAccount model)
        {
            try
            {
                var result = _HotelRepository.UpsertHotelBankingAccount(model);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Lưu thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lưu thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PaymentUpsert - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult BankingAccountDelete(int id)
        {
            try
            {
                var result = _HotelRepository.DeleteHotelBankingAccount(id);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PaymentDelete - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion
        #region ShareHolder

        [HttpPost]
        public IActionResult ShareHolderListing(int hotel_id)
        {
            var model = new GenericViewModel<HotelShareHolderGridModel>();
            try
            {
                var datas = _HotelRepository.GetHotelShareHolderList(hotel_id);
                model.CurrentPage = 1;
                model.ListData = datas.ToList();
                model.PageSize = 20;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / 20);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - ShareHolderListing: " + ex);
            }
            return PartialView(model);
        }

        public IActionResult ShareHolderUpsert(int id, int hotel_id)
        {
            var model = new HotelShareHolder()
            {
                HotelId = hotel_id
            };

            try
            {
                if (id > 0)
                    model = _HotelRepository.GetHotelShareHolderById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - ShareHolderUpsert: " + ex);
            }

            return PartialView(model);
        }

        [HttpPost]
        public IActionResult ShareHolderUpsert(HotelShareHolder model)
        {
            try
            {
                var result = _HotelRepository.UpsertHotelShareHolder(model);

                if (result > 0)
                {
                    return Json(new
                    {
                        isSuccess = true,
                        message = "Lưu cổ đông thành công"
                    });
                }
                else
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Lưu cổ đông thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ShareHolderUpsert - SupplierController: " + ex.Message);
                return Json(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult ShareHolderDelete(int id)
        {
            try
            {
                var result = _HotelRepository.DeleteHotelShareHolder(id);

                if (result > 0)
                {
                    return Json(new
                    {
                        isSuccess = true,
                        message = "Xóa cổ đông thành công"
                    });
                }
                else
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Xóa cổ đông thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ShareHolderDelete - SupplierController: " + ex.Message);
                return Json(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        #endregion


        #region Contact
        [HttpPost]
        public IActionResult ContactListing(int hotel_id)
        {
            var model = new GenericViewModel<HotelContactGridModel>();
            try
            {
                var datas = _HotelRepository.GetHotelContactList(hotel_id);
                model.CurrentPage = 1;
                model.ListData = datas.ToList();
                model.PageSize = 20;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / 20);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - ContactListing: " + ex);
            }
            return PartialView(model);
        }

        public IActionResult ContactUpsert(int id, int hotel_id)
        {
            var model = new HotelContact()
            {
                HotelId = hotel_id
            };

            try
            {
                if (id > 0) model = _HotelRepository.GetHotelContactById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - ContactUpsert: " + ex);
            }
            return PartialView(model);
        }

        [HttpPost]
        public IActionResult ContactUpsert(HotelContact model)
        {
            try
            {
                var result = _HotelRepository.UpsertHotelContact(model);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Lưu thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lưu thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ContactUpsert - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult ContactDelete(int id)
        {
            try
            {
                var result = _HotelRepository.DeleteHotelContact(id);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ContactDelete - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region Surcharge
        [HttpPost]
        public IActionResult SurchargeListing(int hotel_id, int page_index, int page_size)
        {
            var model = new GenericViewModel<HotelSurchargeGridModel>();
            try
            {
                var datas = _HotelRepository.GetHotelSurchargeList(hotel_id, page_index, page_size);
                model.CurrentPage = page_index;
                model.ListData = datas.ToList();
                model.PageSize = page_size;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SurchargeListing: " + ex);
            }
            return PartialView(model);
        }

        public IActionResult SurchargeUpsert(int id, int hotel_id)
        {
            var model = new HotelSurcharge()
            {
                HotelId = hotel_id,
                Status = 1
            };

            try
            {
                if (id > 0) model = _HotelRepository.GetHotelSurchargeById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SurchargeUpsert: " + ex);
            }
            return PartialView(model);
        }

        [HttpPost]
        public IActionResult SurchargeUpsert(HotelSurcharge model)
        {
            try
            {
                var result = _HotelRepository.UpsertHotelSurcharge(model);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Lưu thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lưu thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SurchargeUpsert: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult SurchargeDelete(int id)
        {
            try
            {
                var result = _HotelRepository.DeleteHotelSurcharge(id);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SurchargeDelete: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region Room

        [HttpPost]
        public IActionResult RoomListing(int hotel_id, int page_index, int page_size)
        {
            var model = new GenericViewModel<HotelRoomGridModel>();
            try
            {
                var datas = _HotelRepository.GetHotelRoomList(hotel_id, page_index, page_size);
                model.CurrentPage = page_index;
                model.ListData = datas.ToList();
                model.PageSize = page_size;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SurchargeListing: " + ex);
            }
            return PartialView(model);
        }
        // ✅ refresh lock cache cho 1 hotel (clear redis)
        //[HttpGet]
        //public IActionResult RefreshHotelLocks(int hotel_id)
        //{
        //    try
        //    {
        //        // nếu bạn có cache key theo hotel thì clear ở đây
        //        var cacheKey = $"{CacheName.TTLOCK_HOTEL_LOCKS}:{hotel_id}";
        //        _redisService.clear(cacheKey, 0);
        //    }
        //    catch { }

        //    return new JsonResult(new { isSuccess = true });
        //}
        [HttpPost]
        public async Task<IActionResult> ResetRoomLockAdminPass(int roomId, long lockId, string newPassword)
        {
            try
            {
                if (lockId <= 0) return Json(new { isSuccess = false, message = "LockId không hợp lệ" });
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
                    return Json(new { isSuccess = false, message = "Password không hợp lệ" });

                // 1) call TTLock API
                var apiRes = await _ttlockService.ChangeAdminKeyboardPwdAsync(lockId, newPassword, changeType: 2);

                if (apiRes == null || apiRes.errcode != 0)
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = $"TTLock lỗi: {apiRes?.errmsg ?? "unknown"}"
                    });
                }

                // 2) success -> encrypt + save DB
                //var enc = _lockSecretProtector.Protect(newPassword);
                var affected = _HotelRepository.UpdateRoomLockAdminPwd(roomId, lockId, newPassword);

                if (affected <= 0)
                {
                    // tuỳ bạn: báo lỗi hoặc coi như OK nhưng chưa sync record gateway_locks
                    return Json(new
                    {
                        isSuccess = true,
                        message = "Reset pass thành công (TTLock OK). Nhưng chưa tìm thấy record trong gateway_locks để lưu."
                    });
                }

                return Json(new { isSuccess = true, message = "Reset pass thành công" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetLockAdminPass: " + ex);
                return Json(new { isSuccess = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestCheckoutReset(int roomId)
        {
            try
            {
                var room = _HotelRepository.GetHotelRoomById(roomId);
                if (room == null) return Json(new { isSuccess = false, message = "Không tìm thấy phòng" });

                if (!room.LockId.HasValue || room.LockId.Value <= 0)
                    return Json(new { isSuccess = false, message = "Phòng chưa gán khóa" });

                // 1) Lấy booking checkout gần nhất của khách sạn
                var booking = _HotelRepository.GetLatestCheckedOutBooking(room.HotelId);
                if (booking == null)
                    return Json(new { isSuccess = false, message = "Không tìm thấy booking để kiểm tra checkout" });

                // 2) Check thời điểm checkout
                if (DateTime.Now <= booking.CheckoutDateTime)
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = $"Chưa tới thời điểm checkout. Checkout lúc {booking.CheckoutDateTime:dd/MM/yyyy HH:mm}"
                    });
                }

                // 3) Check đã reset chưa (để tránh bấm nhiều lần)
                var checkoutDate = booking.DepartureDate.Date;
                var done = _HotelRepository.IsLockResetDoneForCheckout(room.HotelId, room.LockId.Value, checkoutDate);
                if (done)
                {
                    return Json(new
                    {
                        isSuccess = true,
                        message = "Khóa đã được reset theo checkout hôm nay rồi."
                    });
                }

                // 4) Generate pass mới (demo: 6 số)
                var newPwd = PasswordHelper.GenerateNumeric(6);

                // 5) Call TTLock API reset
                var apiRes = await _ttlockService.ChangeAdminKeyboardPwdAsync(room.LockId.Value, newPwd, changeType: 2);
                if (apiRes == null || apiRes.errcode != 0)
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = $"TTLock lỗi: {apiRes?.errmsg ?? "unknown"}"
                    });
                }

                
                _HotelRepository.UpdateRoomLockAdminPwd(roomId, room.LockId.Value, newPwd);

                // 7) Insert history log + đánh dấu đã gửi (tele/email tuỳ bạn)
                _HotelRepository.InsertLockResetHistory(
                    hotelId: room.HotelId,
                    roomId: roomId,
                    lockId: room.LockId.Value,
                    bookingId: booking.BookingId,
                    resetType: 2,
                    passwordEnc: newPwd,
                    sentTele: false,
                    sentEmail: false
                );

                // 8) TODO: bắn n8n/tele/email (demo trước có thể return password cho leader nhìn)
                return Json(new
                {
                    isSuccess = true,
                    message = $"Checkout reset OK. Mật khẩu mới: {newPwd}",
                    password = newPwd
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TestCheckoutReset: " + ex);
                return Json(new { isSuccess = false, message = "Có lỗi xảy ra" });
            }
        }

        public async Task<IActionResult> RoomUpsert(int id, int hotel_id, bool is_copy = false, bool forceLockRefresh = false)
        {
            var model = new HotelRoomUpsertModel()
            {
                HotelId = hotel_id,
                IsActive = true,
                IsDisplayWebsite = true
            };

            try
            {
                if (id > 0)
                {
                    var room = _HotelRepository.GetHotelRoomById(id);

                    model = new HotelRoomUpsertModel
                    {
                        Avatar = room.Avatar,
                        BedRoomType = room.BedRoomType,
                        Code = room.Code,
                        HotelId = room.HotelId,
                        Name = room.Name,
                        Description = room.Description,
                        TypeOfRoom = room.TypeOfRoom,
                        Extends = room.Extends,
                        IsActive = room.IsActive,
                        IsDisplayWebsite = room.IsDisplayWebsite,
                        NumberOfAdult = room.NumberOfAdult,
                        NumberOfBedRoom = room.NumberOfBedRoom,
                        NumberOfChild = room.NumberOfChild,
                        NumberOfRoom = room.NumberOfRoom,
                        RoomAvatar = room.RoomAvatar,
                        RoomArea = room.RoomArea,

                        LockId = room.LockId, // mapping lock vào phòng
                    };

                    if (is_copy)
                    {
                        model.Id = 0;
                        model.RoomAvatar = string.Empty;
                        model.Avatar = string.Empty;
                        model.IsActive = true;
                        model.IsDisplayWebsite = false;
                        model.LockId = null; // copy không copy LockId
                    }
                    else
                    {
                        model.Id = room.Id;
                    }
                }

                // ====== LOAD DROPDOWN LOCKS (Redis realtime + DB mapping) ======
                // 1) lấy danh sách gateway thuộc hotel từ DB để filter (khuyến nghị)
                // Nếu chưa có hàm này thì làm tạm: lấy all gateway từ Redis/API
                var gatewayIds = _HotelRepository.GetGatewayIdsByHotel(hotel_id); // List<long>
                List<TTLockGatewayItem> gateways;

                if (gatewayIds != null && gatewayIds.Any())
                {
                    // lấy toàn bộ gateway từ redis/api rồi filter theo gatewayIds
                    var allGw = await _ttlockService.GetGatewayListAsync(forceRefresh: forceLockRefresh);
                    gateways = allGw.Where(x => gatewayIds.Contains(x.gatewayId)).ToList();
                }
                else
                {
                    gateways = await _ttlockService.GetGatewayListAsync(forceRefresh: forceLockRefresh);
                }

                // 2) build lock list dropdown từ Redis/API (theo gateway list đã filter)
                var lockList = await _ttlockService.BuildHotelLocksDropdownAsync(gateways, forceRefresh: forceLockRefresh);

                // 3) lấy lockId đã gán phòng trong DB để loại ra
                var assignedLockIds = _HotelRepository.GetAssignedLockIdsByHotel(hotel_id) ?? new List<long>();

                // 4) nếu đang edit mà có LockId => cho phép hiện lock hiện tại
                if (model.LockId.HasValue)
                {
                    assignedLockIds = assignedLockIds.Where(x => x != model.LockId.Value).ToList();
                }

                // 5) filter locks chưa gán
                var availableLocks = lockList
                    .Where(x => !assignedLockIds.Contains(x.LockId))
                    .OrderBy(x => x.LockName)
                    .ToList();

                // 6) nếu edit và lock hiện tại không còn trong Redis list (hiếm) => add vào để vẫn select được
                if (model.LockId.HasValue && !availableLocks.Any(x => x.LockId == model.LockId.Value))
                {
                    var current = lockList.FirstOrDefault(x => x.LockId == model.LockId.Value);
                    if (current != null) availableLocks.Insert(0, current);
                }

                ViewBag.AvailableLocks = availableLocks;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RoomUpsert(GET): " + ex);
            }

            ViewBag.TypeOfRooms = _AllCodeRepository.GetListByType("TYPE_OF_ROOM");
            ViewBag.RoomBedTypes = _AllCodeRepository.GetListByType("BedRoomType");
            ViewBag.RoomPackageTypes = _AllCodeRepository.GetListByType("HOTELROOM_UTILITIES");

            return PartialView(model);
        }


        // ✅ POST Upsert: validate lockId có trong Redis list (optional) rồi lưu DB
        [HttpPost]
        public async Task<IActionResult> RoomUpsert(HotelRoomUpsertModel model)
        {
            try
            {
                // ========== VALIDATE LOCK tồn tại + chưa bị gán ==========
                if (model.LockId.HasValue)
                {
                    var gatewayIds = _HotelRepository.GetGatewayIdsByHotel(model.HotelId) ?? new List<long>();
                    var allGw = await _ttlockService.GetGatewayListAsync(forceRefresh: false);

                    var gws = gatewayIds.Any()
                        ? allGw.Where(x => gatewayIds.Contains(x.gatewayId)).ToList()
                        : allGw;

                    var lockList = await _ttlockService.BuildHotelLocksDropdownAsync(gws, forceRefresh: false);

                    if (!lockList.Any(x => x.LockId == model.LockId.Value))
                    {
                        return Json(new
                        {
                            isSuccess = false,
                            message = "Khóa không tồn tại (chưa sync từ TTLock). Vui lòng Refresh locks và thử lại."
                        });
                    }

                    // check lock đã gán cho phòng khác chưa (khi tạo mới)
                    var assigned = _HotelRepository.GetAssignedLockIdsByHotel(model.HotelId) ?? new List<long>();
                    if (assigned.Contains(model.LockId.Value) && model.Id == 0)
                    {
                        return Json(new { isSuccess = false, message = "Khóa này đã được gán cho phòng khác." });
                    }
                }

                // ========== XÁC ĐỊNH MAP LẦN ĐẦU ==========
                long? oldLockId = null;
                if (model.Id > 0)
                {
                    var oldRoom = _HotelRepository.GetHotelRoomById(model.Id);
                    if (oldRoom == null)
                        return Json(new { isSuccess = false, message = "Không tìm thấy phòng để cập nhật" });

                    oldLockId = oldRoom.LockId;
                }

                bool isFirstTimeMapping =
                    model.LockId.HasValue &&
                    (
                        model.Id == 0                 // tạo mới có lock
                        || !oldLockId.HasValue        // room cũ chưa có lock
                    );

                // ========== UPSERT ROOM ==========
                var result = _HotelRepository.UpsertHotelRoom(model);
                if (result <= 0)
                    return Json(new { isSuccess = false, message = "Lưu thông tin thất bại" });

                // Nếu repo trả về roomId mới khi insert thì dùng result, còn nếu Upsert trả số dòng ảnh hưởng
                // thì phải lấy lại roomId theo Code hoặc cách khác.
                var roomId = model.Id > 0 ? model.Id : result;

                // ========== MAP LẦN ĐẦU -> TẠO PASS RANDOM + SET TTLOCK + LƯU DB ==========
                if (isFirstTimeMapping)
                {
                    // random pass (ví dụ 6 số)
                    var newPwd = PasswordHelper.GenerateNumeric(6);

                    // khuyến nghị: set lên TTLock để đồng bộ
                    var apiRes = await _ttlockService.ChangeAdminKeyboardPwdAsync(model.LockId.Value, newPwd, changeType: 2);
                    if (apiRes == null || apiRes.errcode != 0)
                    {
                        // lúc này Room đã upsert xong nhưng set pass lock fail -> bạn quyết định rollback hay báo lỗi
                        return Json(new
                        {
                            isSuccess = false,
                            message = $"Lưu phòng OK nhưng set pass TTLock lỗi: {apiRes?.errmsg ?? "unknown"}"
                        });
                    }

                    // lưu DB (nên encrypt/protect)
                    // var enc = _lockSecretProtector.Protect(newPwd);
                    _HotelRepository.UpdateRoomLockAdminPwd(roomId, model.LockId.Value, newPwd);

                    // optional: log lịch sử
                    _HotelRepository.InsertLockResetHistory(
                        hotelId: model.HotelId,
                        roomId: roomId,
                        lockId: model.LockId.Value,
                        bookingId: 0,        // map lần đầu không có booking
                        resetType: 1,        // 1 = first map (tuỳ bạn định nghĩa)
                        passwordEnc: newPwd, // nên encrypt
                        sentTele: false,
                        sentEmail: false
                    );
                }

                return Json(new { isSuccess = true, message = "Lưu thông tin thành công" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RoomUpsert(POST): " + ex);
                return Json(new { isSuccess = false, message = "Có lỗi xảy ra" });
            }
        }


        [HttpPost]
        public IActionResult RoomDelete(int id)
        {
            try
            {
                var result = _HotelRepository.DeleteHotelRoom(id);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thông tin thành công"
                    });
                }
                else if (result == -2)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Phòng đã được cài đặt cho chương trình. Bạn không thể xóa dữ liệu."
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RoomDelete: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region Ultility

        [HttpPost]
        public async Task<IActionResult> UltilityUpsert(int hotel_id, string extends)
        {
            try
            {
                var result = await _HotelRepository.UpsertHotelUltilities(hotel_id, extends);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Lưu thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lưu thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SurchargeUpsert: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #endregion

        public IActionResult Suggest(string text, int size = 20)
        {
            var data = _HotelRepository.GetSuggestionHotelList(text, size);
            return new JsonResult(data.Select(s => new
            {
                id = s.Id,
                name = s.Name
            }));
        }

        public IActionResult UpsertHotel()
        {
            try
            {
                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertHotel - HotelController: " + ex);
                return PartialView();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportHotel(IFormFile file)
        {

            try
            {
                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                var data_list = new List<AddHotelViewModel>();
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.FirstOrDefault();

                    var endRow = ws.Cells.End.Row;
                    var startRow = 2;

                    for (int row = startRow; row <= endRow; row++)
                    {
                        var cellRange = ws.Cells[row, 1, row, 11];
                        var isRowEmpty = cellRange.All(c => c.Value == null);
                        if (isRowEmpty)
                        {
                            break;
                        }
                        var data_name = ws.Cells[row, 1].Value;
                        var data_Street = ws.Cells[row, 2].Value;
                        var account_number = ws.Cells[row, 3].Value;
                        var bannk_name = ws.Cells[row, 4].Value;
                        var bank_branch = ws.Cells[row, 5].Value;
                        var bank_account_name = ws.Cells[row, 6].Value;
                        var data = new AddHotelViewModel
                        {
                            Name = (data_name ?? string.Empty).ToString(),
                            Street = (data_Street ?? string.Empty).ToString(),
                            AccountNumber = (account_number ?? string.Empty).ToString(),
                            BankName = (bannk_name ?? string.Empty).ToString(),
                            Branch = (bank_branch ?? string.Empty).ToString(),
                            BankAccountName = (bank_account_name ?? string.Empty).ToString(),
                        };

                        data_list.Add(data);
                    }


                }

                ViewBag.CheckedAll = true;
                return PartialView(data_list);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ImportHotel - HotelController: " + ex.ToString());
                return PartialView();
            }
        }
        [HttpPost]
        public async Task<IActionResult> SetUpHotel(List<AddHotelViewModel> model)
        {
            try
            {
                if (model != null && model.Count > 0)
                {
                    var userId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    foreach (var item in model)
                    {
                        var Supplier = new SupplierConfigUpsertModel();
                        Supplier.FullName = item.Name;
                        Supplier.ContactName = "";
                        Supplier.CreatedBy = userId;
                        Supplier.IsDisplayWebsite = false;
                        Supplier.BankId = null;
                        Supplier.BankAccountName = item.BankAccountName ?? "";
                        Supplier.BankAccountNumber = item.AccountNumber ?? "";
                        Supplier.BankBranch = item.Branch;
                        Supplier.Phone = "";
                        Supplier.ContactPhone = "";
                        int createsupplier = 0;
                        try
                        {
                            createsupplier = _supplierRepository.Add(Supplier);
                        }
                        catch
                        {

                        }
                        if (createsupplier <= 0)
                        {
                            var suplier = _supplierRepository.GetByIDOrName(Supplier.SupplierId, Supplier.FullName);
                            createsupplier = suplier;
                        }
                        var Hotel = new Hotel();
                        Hotel.Name = item.Name;
                        Hotel.Street = item.Street;
                        Hotel.CreatedBy = userId;
                        Hotel.UpdatedBy = userId;
                        Hotel.CreatedDate = DateTime.Now;
                        Hotel.UpdatedDate = DateTime.Now;
                        Hotel.SupplierId = createsupplier;
                        Hotel.HotelId = "0";
                        int hotel_id = await _hotelBookingRepositories.CreateHotel(Hotel);
                        _workQueueClient.SyncES(hotel_id, _configuration["DataBaseConfig:Elastic:SP:sp_GetHotel"], _configuration["DataBaseConfig:Elastic:Index:Hotel"], ProjectType.ADAVIGO_CMS, "SetUpHotel HotelController");

                        if (hotel_id > 0)
                        {
                            var hotel_banking_account = new HotelBankingAccount()
                            {
                                AccountName = item.BankAccountName ?? "",
                                AccountNumber = item.AccountNumber ?? "",
                                BankId = item.BankName ?? "",
                                Branch = item.Branch ?? "",
                                CreatedBy = userId,
                                CreatedDate = DateTime.Now,
                                HotelId = hotel_id,
                                UpdatedBy = userId,
                                UpdatedDate = DateTime.Now,

                            };
                            var result = _HotelRepository.UpsertHotelBankingAccountByName(hotel_banking_account);
                        }
                    }

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Thêm khách sạn thành công"
                    });
                }
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Thêm khách sạn không thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpHotel - HotelController: " + ex.ToString());
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "xẩy ra lỗi vui lòng liên hệ IT"
                });
            }
        }
        public async Task<IActionResult> SuggestDistrict(string id)
        {
            var data = await _commonRepository.GetDistrictList(id);
            return new JsonResult(data);
        }

        public async Task<IActionResult> ProgramSuggestionByHotelid(string txt_search)
        {

            try
            {
                if (txt_search == null)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        data = new List<ProgramsViewModel>()
                    });
                }
                var data = await _programsESRepository.GetProgramsSuggesstionByHotelid(txt_search);
                if (data != null && data.Count > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = data
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        data = new List<ProgramsViewModel>()
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ProgramSuggestion - ProgramsController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    data = new List<ProgramsViewModel>()
                });
            }

        }
    }
}
