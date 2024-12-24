﻿using APP_CHECKOUT.RabitMQ;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ElasticController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient work_queue;
        public ElasticController(IConfiguration configuration)
        {
            work_queue = new WorkQueueClient(configuration);
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult PushToQueue([FromBody] QueueRequest request)
        {
            try
            {
                var j_param = new Dictionary<string, object>
                {
                    { "store_name", request.StoreName },
                    { "index_es", "adavigo_" + request.StoreName.ToLower() },
                    { "project_type", Convert.ToInt16(ProjectType.ADAVIGO_CMS) },
                    { "id", request.Id }
                };

                var _data_push = JsonConvert.SerializeObject(j_param);
                var response_queue = work_queue.InsertQueueSimple(_data_push, "SYNC_DATA_TO_ES");

                if (response_queue)
                {
                    return new JsonResult(new { isSuccess = true, message = "Push queue thành công" });
                }
                else
                {
                    return new JsonResult(new { isSuccess = false, message = "Push queue thất bại" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { isSuccess = false, message = ex.Message });
            }
        }

    }
}
