using Caching.RedisWorker;
using Entities.ConfigModels;
using Entities.ViewModels.Lock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Service.Look;

namespace WEB.CMS.Controllers.LockManager
{
    public class LockController : Controller
    {

     
        private readonly LockService _lockService;

        private readonly IAllCodeRepository _AllCodeRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly string _UrlStaticImage;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;

        public LockController(RedisConn redisService, IAllCodeRepository allCodeRepository,  IConfiguration configuration ,  LockService lockService)
        {
  
            _lockService = lockService;


            _AllCodeRepository = allCodeRepository;
            
            _configuration = configuration;
            _redisService = redisService;

        }
        public async Task<IActionResult> Index(bool forceRefresh = false)
        {
            var model = new DeviceHierarchyMapViewModel();

            try
            {
                var db = 0;

                // ===== 1) Gateway list cache =====
                var gwCacheKey = CacheName.TTLOCK_GATEWAY_LIST;
                List<TTLockGatewayItem> gateways = null;

                if (!forceRefresh)
                {
                    var cachedGw = _redisService.Get(gwCacheKey, db);
                    if (!string.IsNullOrEmpty(cachedGw))
                        gateways = JsonConvert.DeserializeObject<List<TTLockGatewayItem>>(cachedGw);
                }

                if (gateways == null)
                {
                    gateways = await _lockService.GetGatewaysAsync(pageNo: 1, pageSize: 20);
                    _redisService.Set(gwCacheKey, JsonConvert.SerializeObject(gateways ?? new List<TTLockGatewayItem>()), db);
                }

                gateways ??= new List<TTLockGatewayItem>();

                // (tuỳ UI) lấy 3 gateway đầu cho đúng layout 3 cột
                var topGateways = gateways.Take(3).ToList();

                // ===== 2) Locks per gateway cache =====
                var result = new List<GatewayWithLocksViewModel>();

                foreach (var gw in topGateways)
                {
                    var locksCacheKey = $"{CacheName.TTLOCK_GATEWAY_LOCKS}:{gw.gatewayId}";
                    List<TTLockLockItem> locks = null;

                    if (!forceRefresh)
                    {
                        var cachedLocks = _redisService.Get(locksCacheKey, db);
                        if (!string.IsNullOrEmpty(cachedLocks))
                            locks = JsonConvert.DeserializeObject<List<TTLockLockItem>>(cachedLocks);
                    }

                    if (locks == null)
                    {
                        locks = await _lockService.GetLocksByGatewayAsync(gw.gatewayId);
                        _redisService.Set(locksCacheKey, JsonConvert.SerializeObject(locks ?? new List<TTLockLockItem>()), db);
                    }

                    result.Add(new GatewayWithLocksViewModel
                    {
                        Gateway = gw,
                        Locks = locks ?? new List<TTLockLockItem>()
                    });
                }

                model.Gateways = result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"Lock/Index Error: {ex}");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Refresh()
        {
            try
            {
                var db = 0;

                // clear gateway list
                _redisService.clear(CacheName.TTLOCK_GATEWAY_LIST, db);

                // clear locks của 20 gateway đầu (hoặc 3 gateway) bằng cách đọc list gateway từ cache trước khi clear
                // -> vì clear gateway list rồi không còn biết key, nên làm theo cách:
                // 1) cố get gateway list từ cache trước (nếu có) để clear locks
                // 2) hoặc dùng pattern key-prefix (nếu RedisConn hỗ trợ), hiện chưa thấy hỗ trợ.

                // Cách an toàn: clear luôn 3 keys locks theo top gateway (nếu bạn muốn).
                // Nếu bạn muốn clear hết theo list, ta làm: get cached gateways trước rồi clear.
                // Ở đây: thử lấy cached trước khi clear (nhưng ta đã clear ở trên). => chuyển thứ tự cho đúng:
            }
            catch { }

            // redirect index force refresh (sẽ gọi API và set cache lại)
            return RedirectToAction("Index", new { forceRefresh = true });
        }

        // ✅ Refresh “chuẩn”: lấy list gateway từ redis rồi clear locks từng gateway, sau đó clear gateway list
        [HttpGet]
        public IActionResult RefreshHard()
        {
            try
            {
                var db = 0;
                var cachedGw = _redisService.Get(CacheName.TTLOCK_GATEWAY_LIST, db);

                if (!string.IsNullOrEmpty(cachedGw))
                {
                    var gateways = JsonConvert.DeserializeObject<List<TTLockGatewayItem>>(cachedGw) ?? new();
                    foreach (var gw in gateways.Take(20))
                    {
                        _redisService.clear($"{CacheName.TTLOCK_GATEWAY_LOCKS}:{gw.gatewayId}", db);
                    }
                }

                _redisService.clear(CacheName.TTLOCK_GATEWAY_LIST, db);
            }
            catch { }

            return RedirectToAction("Index", new { forceRefresh = true });
        }

    }

}

