using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Vinpearl
{
    public class VinpearlGetGuaranteeMethodsResponseModel
    {
        public List<AvailableGuaranteeMethod> availableGuaranteeMethods { get; set; }
        public bool isSuccess { get; set; }
        public VinpearlGetGuaranteeMethodData data { get; set; }
        public string requestId { get; set; }
        public string permissionCode { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class VinpearlGetGuaranteeMethodAmount
    {
        public Amount amount { get; set; }
        public string currencyCode { get; set; }
    }

    public class AvailableGuaranteeMethod
    {
        public string reservationId { get; set; }
        public DateTime guaranteeDate { get; set; }
        public List<GuaranteeMethod> guaranteeMethods { get; set; }
        public GuaranteeAmount guaranteeAmount { get; set; }
        public DateTime dueDate { get; set; }
    }

    public class VinpearlGetGuaranteeMethodData
    {
        public string reservationId { get; set; }
        public DateTime guaranteeDate { get; set; }
        public List<GuaranteeMethod> guaranteeMethods { get; set; }
        public GuaranteeAmount guaranteeAmount { get; set; }
        public DateTime dueDate { get; set; }
    }

    public class VinpearlGetGuaranteeMethodDetail
    {
        public string id { get; set; }
        public string type { get; set; }
        public string depositType { get; set; }
        public int depositAmount { get; set; }
        public string weekdays { get; set; }
        public List<string> weekdaysEnum { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public int weekdaysChecksum { get; set; }
        public int order { get; set; }
        public string parentId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int timingValue { get; set; }
        public string timingTypeEnum { get; set; }
    }

    public class GuaranteeAmount
    {
        public Amount amount { get; set; }
    }

    public class GuaranteeMethod
    {
        public VinpearlGetGuaranteeMethodDetail detail { get; set; }
        public string depositType { get; set; }
        public int daysBeforeArrival { get; set; }
        public string id { get; set; }
        public string reservationId { get; set; }
        public string ratePlanInfoId { get; set; }
        public Amount amount { get; set; }
        public DateTime stayDate { get; set; }
    }


}