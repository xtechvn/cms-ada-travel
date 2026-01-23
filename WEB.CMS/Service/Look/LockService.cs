using Telegram.Bot.Types;
using Utilities.Contants;
using Utilities;
using Newtonsoft.Json;
using WEB.CMS.Models;
using Entities.ViewModels;
using Entities.ViewModels.Lock;
namespace WEB.CMS.Service.Look
{
    public class LockService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        public LockService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _context = httpContextAccessor;
        }
        public async Task<List<TTLockGatewayItem>> GetGatewaysAsync(int pageNo = 1, int pageSize = 20)
        {
            try
            {
                var baseUrl = _configuration["DataBaseConfig:TTLock:BaseUrl"] ?? "https://euapi.ttlock.com";
                var clientId = _configuration["DataBaseConfig:TTLock:ClientId"];
                var accessToken = _configuration["DataBaseConfig:TTLock:AccessToken"];

                // date = unix milliseconds (giống Postman Date.now())
                var date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var url =
                    $"{baseUrl}/v3/gateway/list" +
                    $"?clientId={Uri.EscapeDataString(clientId ?? "")}" +
                    $"&accessToken={Uri.EscapeDataString(accessToken ?? "")}" +
                    $"&pageNo={pageNo}" +
                    $"&pageSize={pageSize}" +
                    $"&date={date}";

                using var httpClient = new HttpClient();
                var resp = await httpClient.GetAsync(url);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TTLockGatewayListResponse>(json);
                    return data?.list ?? new List<TTLockGatewayItem>();
                }

                return new List<TTLockGatewayItem>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"GetGatewaysAsync Error: {ex}");
                return new List<TTLockGatewayItem>();
            }
        }

        public async Task<List<TTLockLockItem>> GetLocksByGatewayAsync(long gatewayId)
        {
            try
            {
                var baseUrl = _configuration["DataBaseConfig:TTLock:BaseUrl"] ?? "https://euapi.ttlock.com";
                var clientId = _configuration["DataBaseConfig:TTLock:ClientId"];
                var accessToken = _configuration["DataBaseConfig:TTLock:AccessToken"];

                // date = unix milliseconds (giống Postman Date.now())
                var date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var url =
                    $"{baseUrl}/v3/gateway/listLock" +
                    $"?clientId={Uri.EscapeDataString(clientId ?? "")}" +
                    $"&accessToken={Uri.EscapeDataString(accessToken ?? "")}" +
                    $"&gatewayId={gatewayId}" +
                    $"&date={date}";

                using var httpClient = new HttpClient();
                var resp = await httpClient.GetAsync(url);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TTLockListLockResponse>(json);
                    return data?.list ?? new List<TTLockLockItem>();
                }

                return new List<TTLockLockItem>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"GetLocksByGatewayAsync Error: {ex}");
                return new List<TTLockLockItem>();
            }
        }
    }
}
