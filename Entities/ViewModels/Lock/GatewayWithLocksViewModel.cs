using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities.ViewModels.Lock
{
    public class GatewayWithLocksViewModel
    {
        public TTLockGatewayItem Gateway { get; set; } = new TTLockGatewayItem();
        public List<TTLockLockItem> Locks { get; set; } = new List<TTLockLockItem>();

        public int LockCount => Locks?.Count ?? 0;

        public int AvgSignal
        {
            get
            {
                if (Locks == null || Locks.Count == 0) return 0;
                return (int)Math.Round(Locks.Average(x => x.rssi));
            }
        }
    }

    public class DeviceHierarchyMapViewModel
    {
        public string RootName { get; set; } = "xtech-smart-home-core";
        public string RootIp { get; set; } = "9fee893031724a50b067ce39d436cb59";

        public List<GatewayWithLocksViewModel> Gateways { get; set; } = new List<GatewayWithLocksViewModel>();

        public int TotalNodes => 1 + (Gateways?.Count ?? 0) + (Gateways?.Sum(g => g.LockCount) ?? 0);
        public int ActiveAlerts => Gateways?.Sum(g => (g.Locks?.Count(x => x.electricQuantity <= 20) ?? 0)) ?? 0;

        public int AvgSignal
        {
            get
            {
                var all = Gateways?.SelectMany(g => g.Locks ?? new List<TTLockLockItem>()).ToList()
                          ?? new List<TTLockLockItem>();
                if (all.Count == 0) return 0;
                return (int)Math.Round(all.Average(x => x.rssi));
            }
        }
    }
}
