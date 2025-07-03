using Elasticsearch.Net;
using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Caching.Elasticsearch
{
    public class FlyBookingESRepository : ESRepository<FlyBookingESViewModel>
    {
        private readonly IConfiguration _configuration;

        private readonly string index_name = "deepseektravel_sp_getdetailflybookingdetail";

        public FlyBookingESRepository(string Host, IConfiguration configuration) : base(Host)
        {

            _configuration = configuration;
            index_name = configuration["DataBaseConfig:Elastic:Index:FlyBookingDetail"];
        }

        public async Task<List<FlyBookingESViewModel>> GetFlyBookingSuggesstion(string txt_search, int? tenant_id = null, string index_name = "fly_booking_detail_store")
        {
            List<FlyBookingESViewModel> result = new List<FlyBookingESViewModel>();
            try
            {
                int top = 30;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);
                var search_response = elasticClient.Search<object>(s => s
                           .Index(index_name)
                           .Size(top)
                           .Query(q =>
                                 q.Bool(
                               qb => qb.Must(
                                  q => q.Term("isdelete", false),
                                   q => q.QueryString(qs => qs
                                .Fields(new[] { "servicecode" })
                                .Query("*" + txt_search.ToUpper() + "*")
                                .Analyzer("standard")),
                                mu => mu.Term(t => t
                                                .Field("tenantid")
                                                .Value(tenant_id)
                                            )

                            )
                           
                            )));
                if (!search_response.IsValid)
                {
                    var debug = search_response.DebugInformation;
                    return result;
                }
                else
                {
                    result = JsonConvert.DeserializeObject<List<FlyBookingESViewModel>>(JsonConvert.SerializeObject(search_response.Documents));
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        
    }
}
