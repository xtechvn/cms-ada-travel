using Elasticsearch.Net;
using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Caching.Elasticsearch
{
   public class HotelBookingCodeESRepository : ESRepository<HotelBookingCodeESViewModel>
    {
        private readonly IConfiguration _configuration;

        private readonly string index_name = "deepseektravel_sp_gethotelbookingcode";

        public HotelBookingCodeESRepository(string Host, IConfiguration configuration) : base(Host)
        {

            _configuration = configuration;
            index_name = configuration["DataBaseConfig:Elastic:Index:HotelBookingCode"];
        }
        public async Task<List<HotelBookingCodeESViewModel>> BoongKingCodeSuggesstion(string txt_search, int? tenant_id = null)
        {
            List<HotelBookingCodeESViewModel> result = new List<HotelBookingCodeESViewModel>();
            try
            {
                int top = 30;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);
              
                var search_response = elasticClient.Search<HotelBookingCodeESViewModel>(s => s
                         .Index(index_name)
                          .Size(top)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                  q => q.Term("isdelete", false),
                                   q => q.QueryString(qs => qs
                                   .Fields(new[] { "bookingcode" })
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
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<HotelBookingCodeESViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BoongKingCodeSuggesstion - BoongKingCodeESRepository. " + ex);
                return null;
            }

        }
    }
}
