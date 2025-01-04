using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Vinpearl
{


    public class AvailableGuaranteeMethod
    {
        [JsonProperty("reservationId")]
        public string reservationId { get; set; }

        [JsonProperty("guaranteeDate")]
        public DateTime guaranteeDate { get; set; }

        [JsonProperty("guaranteeMethods")]
        public List<GuaranteeMethod> guaranteeMethods { get; set; }

        [JsonProperty("guaranteeAmount")]
        public GuaranteeAmount guaranteeAmount { get; set; }

        [JsonProperty("dueDate")]
        public DateTime dueDate { get; set; }
    }

    public class VinpearlGetGuaranteeMethodsResponseData
    {
        [JsonProperty("reservationId")]
        public string reservationId { get; set; }

        [JsonProperty("guaranteeDate")]
        public DateTime guaranteeDate { get; set; }

        [JsonProperty("guaranteeMethods")]
        public List<GuaranteeMethod> guaranteeMethods { get; set; }

        [JsonProperty("guaranteeAmount")]
        public GuaranteeAmount guaranteeAmount { get; set; }

        [JsonProperty("dueDate")]
        public DateTime dueDate { get; set; }
    }

    public class VinpearlGetGuaranteeMethodsResponseDetail
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("depositType")]
        public string depositType { get; set; }

        [JsonProperty("depositAmount")]
        public int depositAmount { get; set; }

        [JsonProperty("daysBeforeArrival")]
        public int daysBeforeArrival { get; set; }

        [JsonProperty("weekdays")]
        public string weekdays { get; set; }

        [JsonProperty("weekdaysEnum")]
        public List<string> weekdaysEnum { get; set; }

        [JsonProperty("from")]
        public DateTime from { get; set; }

        [JsonProperty("to")]
        public DateTime to { get; set; }

        [JsonProperty("weekdaysChecksum")]
        public int weekdaysChecksum { get; set; }

        [JsonProperty("order")]
        public int order { get; set; }

        [JsonProperty("parentId")]
        public string parentId { get; set; }

        [JsonProperty("createdAt")]
        public DateTime createdAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime updatedAt { get; set; }

        [JsonProperty("timingType")]
        public string timingType { get; set; }

        [JsonProperty("timingThreshold")]
        public int timingThreshold { get; set; }

        [JsonProperty("timingValue")]
        public int timingValue { get; set; }

        [JsonProperty("timingTypeEnum")]
        public string timingTypeEnum { get; set; }
    }

    public class GuaranteeAmount
    {
        [JsonProperty("amount")]
        public GuaranteeAmountAmount amount { get; set; }
    }
    public class GuaranteeAmountAmount
    {
        [JsonProperty("amount")]
        public double amount { get; set; }

        public string currencyCode { get; set; }
    }

    public class GuaranteeMethod
    {
        [JsonProperty("detail")]
        public VinpearlGetGuaranteeMethodsResponseDetail detail { get; set; }

        [JsonProperty("depositType")]
        public string depositType { get; set; }

        [JsonProperty("daysBeforeArrival")]
        public int daysBeforeArrival { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("reservationId")]
        public string reservationId { get; set; }

        [JsonProperty("ratePlanInfoId")]
        public string ratePlanInfoId { get; set; }

        [JsonProperty("amount")]
        public GuaranteeAmount amount { get; set; }

        [JsonProperty("stayDate")]
        public DateTime stayDate { get; set; }
    }

    public class VinpearlGetGuaranteeMethodsResponseModel
    {
        [JsonProperty("availableGuaranteeMethods")]
        public List<AvailableGuaranteeMethod> availableGuaranteeMethods { get; set; }

        [JsonProperty("isSuccess")]
        public bool isSuccess { get; set; }

        [JsonProperty("data")]
        public VinpearlGetGuaranteeMethodsResponseData data { get; set; }

        [JsonProperty("requestId")]
        public string requestId { get; set; }

        [JsonProperty("permissionCode")]
        public string permissionCode { get; set; }

        [JsonProperty("traceId")]
        public string traceId { get; set; }

        [JsonProperty("version")]
        public string version { get; set; }
    }




}