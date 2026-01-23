using Entities.ViewModels.Lock;
using System.Collections.Generic;

public class TTLockGatewayListResponse
{
    public int total { get; set; }
    public int pages { get; set; }
    public int pageNo { get; set; }
    public int pageSize { get; set; }
    public List<TTLockGatewayItem> list { get; set; } = new();
}

public class TTLockGatewayItem
{
    public int deviceNum { get; set; }
    public string gatewayMac { get; set; }
    public string serialNumber { get; set; }
    public int lockNum { get; set; }
    public string gatewayName { get; set; }
    public int electricMeterCount { get; set; }
    public string networkName { get; set; }
    public int waterMeterCount { get; set; }
    public int isOnline { get; set; }   // 1 online, 0 offline
    public int gatewayVersion { get; set; }
    public long gatewayId { get; set; }
    public string networkMac { get; set; }
}
public class TTLockLockListResponse
{
    public List<TTLockLockItem> list { get; set; } = new();
}

