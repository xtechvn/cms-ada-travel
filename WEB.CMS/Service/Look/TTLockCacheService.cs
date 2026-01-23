using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Caching.RedisWorker;
using Entities.ViewModels.Hotel;
using Entities.ViewModels.Lock;
using Utilities.Contants;

public class TTLockCacheService
{
    private readonly IConfiguration _configuration;
    private readonly RedisConn _redis;
    private const int DB_INDEX = 0;

    public TTLockCacheService(IConfiguration configuration, RedisConn redis)
    {
        _configuration = configuration;
        _redis = redis;
    }

    public async Task<TTLockSimpleResponse> ChangeAdminKeyboardPwdAsync(long lockId, string password, int changeType = 2)
    {
        using var httpClient = new HttpClient();

        // ✅ date = unix ms (TTLock thường yêu cầu ms)
        var dateMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        var clientId = _configuration["DataBaseConfig:TTLock:ClientId"];
        var accessToken = _configuration["DataBaseConfig:TTLock:AccessToken"];

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("clientId", clientId),
            new KeyValuePair<string,string>("accessToken", accessToken),
            new KeyValuePair<string,string>("lockId", lockId.ToString()),
            new KeyValuePair<string,string>("password", password),
            new KeyValuePair<string,string>("changeType", changeType.ToString()),
            new KeyValuePair<string,string>("date", dateMs),
        });

        var url = "https://euapi.ttlock.com/v3/lock/changeAdminKeyboardPwd";
        var res = await httpClient.PostAsync(url, form);

        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            return new TTLockSimpleResponse
            {
                errcode = -1,
                errmsg = $"HTTP {(int)res.StatusCode}",
                description = body
            };
        }

        return JsonConvert.DeserializeObject<TTLockSimpleResponse>(body) ?? new TTLockSimpleResponse
        {
            errcode = -1,
            errmsg = "Deserialize failed",
            description = body
        };
    }

    public async Task<List<TTLockGatewayItem>> GetGatewayListAsync(bool forceRefresh = false)
    {
        var cacheKey = CacheName.TTLOCK_GATEWAY_LIST;

        if (!forceRefresh)
        {
            var cached = _redis.Get(cacheKey, DB_INDEX);
            if (!string.IsNullOrEmpty(cached))
                return JsonConvert.DeserializeObject<List<TTLockGatewayItem>>(cached) ?? new List<TTLockGatewayItem>();
        }

        var clientId = _configuration["DataBaseConfig:TTLock:ClientId"];
        var accessToken = _configuration["DataBaseConfig:TTLock:AccessToken"];
        var pageNo = 1;
        var pageSize = 100;
        var date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var url = $"https://euapi.ttlock.com/v3/gateway/list?clientId={clientId}&accessToken={accessToken}&pageNo={pageNo}&pageSize={pageSize}&date={date}";

        using var http = new HttpClient();
        var json = await http.GetStringAsync(url);

        var resp = JsonConvert.DeserializeObject<TTLockGatewayListResponse>(json);
        var list = resp?.list ?? new List<TTLockGatewayItem>();

        _redis.Set(cacheKey, JsonConvert.SerializeObject(list), DB_INDEX);
        return list;
    }

    public async Task<List<TTLockLockItem>> GetLocksByGatewayAsync(long gatewayId, bool forceRefresh = false)
    {
        var cacheKey = $"{CacheName.TTLOCK_GATEWAY_LOCKS}:{gatewayId}";

        if (!forceRefresh)
        {
            var cached = _redis.Get(cacheKey, DB_INDEX);
            if (!string.IsNullOrEmpty(cached))
                return JsonConvert.DeserializeObject<List<TTLockLockItem>>(cached) ?? new List<TTLockLockItem>();
        }

        var clientId = _configuration["DataBaseConfig:TTLock:ClientId"];
        var accessToken = _configuration["DataBaseConfig:TTLock:AccessToken"];
        var date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var url = $"https://euapi.ttlock.com/v3/gateway/listLock?clientId={clientId}&accessToken={accessToken}&gatewayId={gatewayId}&date={date}";

        using var http = new HttpClient();
        var json = await http.GetStringAsync(url);

        var resp = JsonConvert.DeserializeObject<TTLockLockListResponse>(json);
        var list = resp?.list ?? new List<TTLockLockItem>();

        _redis.Set(cacheKey, JsonConvert.SerializeObject(list), DB_INDEX);
        return list;
    }

    // ✅ trả list lock dropdown cho 1 hotel (tổng hợp từ gateway list + locks)
    // IMPORTANT: nếu DB có mapping gateway_devices.HotelId thì hãy filter gateway theo hotelId (bên Controller/Repo)
    public async Task<List<LockDropdownModel>> BuildHotelLocksDropdownAsync(
        List<TTLockGatewayItem> gateways,
        bool forceRefresh = false)
    {
        var result = new List<LockDropdownModel>();

        foreach (var gw in gateways)
        {
            var locks = await GetLocksByGatewayAsync(gw.gatewayId, forceRefresh);

            foreach (var l in locks)
            {
                result.Add(new LockDropdownModel
                {
                    //GatewayId = gw.gatewayId,
                    //GatewayName = gw.gatewayName,
                    LockId = l.lockId,
                    LockName = l.lockName,
                    LockMac = l.lockMac
                });
            }
        }

        return result;
    }
}
