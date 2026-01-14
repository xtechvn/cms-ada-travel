
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels.Tour;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities.Contants;
using WEB.CMS.Customize;
using Utilities;
using Nest;

namespace WEB.CMS.Controllers.FlashSale
{
    [CustomAuthorize]

    public class FlashSaleController : Controller
    {

        private readonly IConfiguration _configuration;

        private readonly RedisConn _redisConn;
        private readonly ITourRepository _TourRepository;





            private readonly IAllCodeRepository _allCodeRepository;
        private readonly IFlashSaleRepository _flashSaleRepository;
           private readonly IFlashSaleProductRepository _flashSaleProductRepository;
        public FlashSaleController(IConfiguration configuration, RedisConn redisConn,  IFlashSaleRepository flashSaleRepository, ITourRepository tourRepository, IFlashSaleProductRepository flashSaleProductRepository
            , IAllCodeRepository allCodeRepository
         )
        {

            _redisConn = redisConn;
            _redisConn.Connect();

            _TourRepository = tourRepository;
            _configuration = configuration;
            _flashSaleProductRepository = flashSaleProductRepository;
            _flashSaleRepository = flashSaleRepository;
            _allCodeRepository = allCodeRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Search(DateTime? fromdate, DateTime? todate, int status, int page_index = 1, int page_size = 10)
        {
            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";

            if (fromdate >= todate)
            {
                fromdate = null;
                todate = null;
            }
            if (fromdate == DateTime.MinValue) fromdate = null;
            if (todate == DateTime.MinValue) todate = null;
            page_index = page_index < 1 ? 1 : page_index;
            page_size = page_size < 1 ? 10 : page_size;
            status = status < 0 ? 0 : status;
            // Gọi SP thông qua repository
            var flashSales = await _flashSaleRepository.GetList(fromdate, todate, status, page_index, page_size);
            // Trả về View với dữ liệu đã lấy được
            return View(flashSales);
        }
        public async Task<IActionResult> Detail(int id = -1)
        {
            ViewBag.Detail = new Entities.Models.FlashSale();
            ViewBag.Products = new List<Entities.Models.FlashSaleProduct>();
           
            ViewBag.Supplier = new Supplier();
            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
            if (id > 0)
            {
                var detail = await _flashSaleRepository.GetByID(id);
                if (detail != null && detail.Id > 0)
                {
                    ViewBag.Detail = detail;
                    var product = await _flashSaleProductRepository.GetByFlashSaleID(id);
                    if (product != null && product.Count > 0)
                    {
                        product = product.OrderBy(x => x.Position).ToList();
                        ViewBag.Products = product;
                        // ProductId = tourId
                        // ProductId = tourId (string) → parse sang long
                        var tourIds = product
                         .Select(x => x.ProductId)
                         .Where(x => x.HasValue)
                         .Select(x => x.Value)
                         .Distinct()
                         .ToList();   // => List<long>



                        // ✅ gọi DB lấy danh sách tour theo ids
                        ViewBag.ProductsDetail = await _flashSaleProductRepository.GetToursByIds(tourIds);
                       
                    }
                }
               
            }
            return View();
        }
        [HttpPost]
        public IActionResult ProductFlashSale(int id)
        {
            ViewBag.TourTypes = _allCodeRepository.GetListByType("TOUR_TYPE");
            ViewBag.FlashSaleId = id;
            return View();
        }
        [HttpPost]
        public IActionResult ProductFlashSaleSearch(TourProductSearchModel searchModel)
        {
            var model = new GenericViewModel<TourProductGridModel>();
            try
            {
                searchModel.IsSelfDesign = false;

                var datas = _TourRepository.GetPagingTourProduct(searchModel);
                model.CurrentPage = searchModel.PageIndex;
                model.ListData = datas?.ToList() ?? new List<TourProductGridModel>();
                model.PageSize = searchModel.PageSize;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ProductFlashSaleSearch (Tour) - " + ex);
            }

            // partial list tour (tbody)
            return View( model);
        }

        [HttpPost]
        public async Task<IActionResult> Summit(
    Entities.Models.FlashSale flashsale,
    List<FlashSaleProduct> flashsale_product
)
        {
            try
            {
                if (flashsale == null)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại"
                    });
                }

                // ===== USER LOGIN =====
                var userLogin = 0;
                var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null) userLogin = int.Parse(claim.Value);

                flashsale.UserUpdateId = userLogin;
                flashsale.UpdateLast = DateTime.Now;

                // ====== RULE 1: CHỈ 1 FLASHSALE ĐƯỢC BẬT ======
                // Chỉ check khi user đang bật campaign này
                if (flashsale.Status == 1)
                {
                    // Tìm campaign khác đang bật
                    // Bạn có thể implement repo như gợi ý bên dưới
                    var active = await _flashSaleRepository.GetActiveFlashSaleExceptId(flashsale.Id);

                    if (active != null && active.Id > 0)
                    {
                        return Ok(new
                        {
                            is_success = false,
                            msg = $"Hiện đang có chiến dịch FlashSale đang bật: \"{active.Name}\" (ID: {active.Id}). " +
                                  $"Vui lòng tắt chiến dịch đó trước khi bật chiến dịch này."
                        });
                    }
                }

                // ====== FLASHSALE PRODUCTS INPUT ======
                flashsale_product ??= new List<FlashSaleProduct>();

                // loại trùng theo ProductId để tránh user tick/append nhiều lần
                flashsale_product = flashsale_product
                    .Where(x => x != null)
                    .GroupBy(x => x.ProductId)
                    .Select(g => g.First())
                    .ToList();

                // ====== RULE 2: TỐI ĐA 8 TOUR ======
                const int MAX_PRODUCTS = 8;
                if (flashsale_product.Count > MAX_PRODUCTS)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = $"Mỗi chương trình FlashSale chỉ được tối đa {MAX_PRODUCTS} tour. " +
                              $"Hiện bạn đang chọn {flashsale_product.Count} tour."
                    });
                }

                // ====== LOAD EXISTS (nếu update) ======
                List<FlashSaleProduct> exists_products = new List<FlashSaleProduct>();

                // ====== CREATE / UPDATE FLASHSALE ======
                if (flashsale.Id <= 0)
                {
                    flashsale.UserCreateId = userLogin;
                    flashsale.CreateDate = DateTime.Now;

                    _flashSaleRepository.CreateFlashSale(flashsale);

                    // IMPORTANT:
                    // - Nếu repo CreateFlashSale không tự set flashsale.Id (EF SaveChanges sets it) thì bạn phải sửa repo để trả về Id.
                    // - Nếu đang dùng StoredProcedure có OUT param Identity, cũng cần map lại về flashsale.Id
                }
                else
                {
                    var exists = await _flashSaleRepository.GetByID(flashsale.Id);
                    if (exists != null && exists.Id > 0)
                    {
                        flashsale.UserCreateId = exists.UserCreateId;
                        flashsale.CreateDate = exists.CreateDate;

                        exists_products = await _flashSaleProductRepository.GetByFlashSaleID(flashsale.Id);

                        _flashSaleRepository.UpdateFlashSale(flashsale);
                    }
                    else
                    {
                        // không tìm thấy thì coi như tạo mới
                        flashsale.UserCreateId = userLogin;
                        flashsale.CreateDate = DateTime.Now;

                        _flashSaleRepository.CreateFlashSale(flashsale);
                    }
                }

                // Nếu create/update mà vẫn không có Id => lỗi
                if (flashsale.Id <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Lỗi trong quá trình xử lý, vui lòng thử lại / liên hệ hỗ trợ kỹ thuật"
                    });
                }

                // ====== FLASHSALE PRODUCTS (TOUR) ======

                // 1) mark deleted (những record cũ không còn trong list gửi lên)
                if (exists_products != null && exists_products.Count > 0)
                {
                    var newIds = flashsale_product.Select(x => x.Id).ToHashSet();
                    var deleted = exists_products.Where(x => !newIds.Contains(x.Id)).ToList();

                    foreach (var del in deleted)
                    {
                        del.CampaignId *= -1; // theo logic cũ của bạn
                        _flashSaleProductRepository.UpdateFlashSaleProduct(del);
                    }
                }

                // 2) upsert list mới
                foreach (var p in flashsale_product)
                {
                    p.CampaignId = flashsale.Id;

                    // set default
                    if (p.Position == null || p.Position <= 0) p.Position = 1;
                    if (p.Status == null) p.Status = 1;
                    if (p.SuperSale == null) p.SuperSale = true;

                    if (p.Id <= 0)
                        _flashSaleProductRepository.CreateFlashSaleProduct(p);
                    else
                        _flashSaleProductRepository.UpdateFlashSaleProduct(p);
                }

                return Ok(new
                {
                    is_success = true,
                    msg = "Thêm mới / Cập nhật chiến dịch Flashsale thành công",
                    data = flashsale.Id,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Summit - FlashSaleController: " + ex);

                // Nếu bạn có làm UNIQUE INDEX ở DB để chỉ 1 campaign active,
                // có thể check SqlException Number 2601/2627 ở đây để trả msg đẹp hơn.

                return Ok(new
                {
                    is_success = false,
                    msg = "Thêm mới / Cập nhật sản phẩm thất bại, vui lòng liên hệ bộ phận IT",
                    err = ex.ToString(),
                });
            }
        }




        //}
        //[HttpPost]
        //public async Task<IActionResult> GetActiveFlashSaleCampaignBySupplier(int supplier_id = -1, int flash_sale_id = 0)
        //{
        //    try
        //    {
        //        if (supplier_id <= 0)
        //        {
        //            return Ok(new
        //            {
        //                is_success = false,
        //                msg = "NCC không tồn tại",
        //                exists = false
        //            });
        //        }

        //        var exists_active_flashsale = await _flashSaleESRepository.GetActiveFlashsaleBySupplierID(supplier_id);
        //        if (exists_active_flashsale == null || exists_active_flashsale.Count <= 0)
        //        {
        //            return Ok(new
        //            {
        //                is_success = true,
        //                msg = "Không có chương trình nào hoạt động, được phép thêm chương trình cho NCC này",
        //                exists = false
        //            });
        //        }
        //        else
        //        {
        //            exists_active_flashsale = exists_active_flashsale.Where(x => x.flashsale_id != flash_sale_id).ToList();
        //            if (exists_active_flashsale != null && exists_active_flashsale.Count > 0)
        //            {
        //                return Ok(new
        //                {
        //                    is_success = true,
        //                    msg = "Nhà cung cấp này đã có chương trình FlashSale khác đang hoạt động, vui lòng chọn nhà cung cấp khác",
        //                    exists = true
        //                });
        //            }

        //        }
        //        return Ok(new
        //        {
        //            is_success = true,
        //            msg = "Không có chương trình nào hoạt động, được phép thêm chương trình cho NCC này",
        //            exists = false
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetActiveFlashSaleCampaignBySupplier - FlashSaleController: " + ex.ToString());
        //        return Ok(new
        //        {
        //            is_success = false,
        //            msg = "Thêm mới / Cập nhật sản phẩm thất bại, vui lòng liên hệ bộ phận IT",
        //            err = ex.ToString(),
        //        });
        //    }
        //}
        //[HttpPost]

        //public async Task<IActionResult> SyncES()
        //{
        //    try
        //    {
        //        var fsl = await _flashSaleRepository.GetAll();
        //        if (fsl != null && fsl.Count > 0)
        //        {
        //            List<FlashSaleESModel> all_fsl_es = fsl.Select(x => new FlashSaleESModel()
        //            {
        //                id = _flashSaleESRepository.GenerateId(),
        //                flashsale_id = x.Id,
        //                fromdate = x.FromDate,
        //                status = x.Status,
        //                supplierid = x.SupplierId,
        //                todate = x.ToDate,
        //                name = x.Name,
        //                banner = x.Banner,
        //                created_date = x.CreateDate,
        //                supplier_name = _supplierRepository.GetSuplierById((int)x.SupplierId).FullName,
        //            }).ToList();
        //            await _flashSaleESRepository.DeleteByIds(all_fsl_es.Select(x => x.flashsale_id).ToList());
        //            await _flashSaleESRepository.IndexMany(all_fsl_es);
        //        }
        //        var fspl = await _flashSaleProductRepository.GetAll();
        //        await _flashSaleProductESRepository.DeleteAll();
        //        if (fspl != null && fspl.Count > 0)
        //        {
        //            List<FlashSaleProductESModel> all_fspl_es = fspl.Select(x => new FlashSaleProductESModel()
        //            {
        //                id = _flashSaleProductESRepository.GenerateId(),
        //                valuetype = x.ValueType,
        //                discountvalue = x.DiscountValue,
        //                flashsale_id = x.CampaignId,
        //                flashsale_productid = x.Id,
        //                position = x.Position,
        //                productid = x.ProductId,
        //                status = x.Status,
        //                supersale = x.SuperSale

        //            }).ToList();

        //            if (all_fspl_es.Count > 0)
        //            {
        //                foreach (var product in all_fspl_es)
        //                {
        //                    var product_mongo = await _productV2DetailMongoAccess.GetByID(product.productid);
        //                    if (fsl != null)
        //                    {
        //                        var flashsale = fsl.FirstOrDefault(x => x.Id == product.flashsale_id && x.Status == 1);
        //                        var flashsale_product = fspl.FirstOrDefault(x => x.Id == product.flashsale_productid && x.Status == 1);
        //                        if (flashsale != null && flashsale_product != null)
        //                        {
        //                            await _productV2DetailMongoAccess.UpdateProductFlashsale(product_mongo, flashsale, flashsale_product);
        //                            var list_sub = await _productV2DetailMongoAccess.SubListing(product.productid);
        //                            if (list_sub != null && list_sub.Count > 0)
        //                            {
        //                                foreach (var sub in list_sub)
        //                                {
        //                                    await _productV2DetailMongoAccess.UpdateProductFlashsale(sub, flashsale, flashsale_product);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            await _productV2DetailMongoAccess.UpdateProductFlashsale(product_mongo, null, null);
        //                            var list_sub = await _productV2DetailMongoAccess.SubListing(product.productid);
        //                            if (list_sub != null && list_sub.Count > 0)
        //                            {
        //                                foreach (var sub in list_sub)
        //                                {
        //                                    await _productV2DetailMongoAccess.UpdateProductFlashsale(sub, null, null);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    product.group_id = (product_mongo == null || product_mongo.group_product_id == null) ? "" : product_mongo.group_product_id;

        //                }
        //            }
        //            // await _flashSaleProductESRepository.DeleteByIds(all_fspl_es.Select(x => x.flashsale_productid).ToList());
        //            await _flashSaleProductESRepository.IndexMany(all_fspl_es);
        //            string cache_name = "PRODUCT_LISTING_";
        //            await _redisConn.DeleteCacheByKeyword(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("SyncES - ProductController: " + ex.ToString());
        //        return Ok(new
        //        {
        //            is_success = false
        //        });
        //    }
        //    return Ok(new
        //    {
        //        is_success = true
        //    });
        //}

    }
}
