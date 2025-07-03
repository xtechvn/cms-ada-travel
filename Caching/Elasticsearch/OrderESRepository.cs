using Elasticsearch.Net;
using Entities.ViewModels.ElasticSearch;
using ENTITIES.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Caching.Elasticsearch
{
   public class OrderESRepository :ESRepository<OrderElasticsearchViewModel>
    {
        private readonly IConfiguration _configuration;

        private readonly string index_name = "deepseektravel_sp_getorder";

        public OrderESRepository(string Host, IConfiguration configuration) : base(Host) {

            _configuration = configuration;
            index_name=configuration["DataBaseConfig:Elastic:Index:Order"];
        }
        public async Task<List<OrderElasticsearchViewModel>> GetOrderNoSuggesstion(string txt_search,int? tenant_id=null)
        {
            List<OrderElasticsearchViewModel> result = new List<OrderElasticsearchViewModel>();
            try
            {
                int top = 30;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<OrderElasticsearchViewModel>(s => s
                             .Index(index_name)
                             .Size(top)
                              .Query(q => q
                                    .Bool(b => b
                                        .Must(mu => mu
                                            .Term(t => t
                                                .Field(f => f.tenantid)
                                                .Value(tenant_id)
                                            )
                                        )
                                        .Should(sh =>
                                            sh.QueryString(qs => qs
                                          .Fields(new[] { "orderno" })
                                          .Query("*" + txt_search.ToUpper() + "*")
                                          .Analyzer("standard")
                                        )
                                    )
                                )
                             ));
                if (tenant_id == null)
                {
                    search_response = elasticClient.Search<OrderElasticsearchViewModel>(s => s
                            .Index(index_name)
                            .Size(top)
                             .Query(q => q
                                   .Bool(b => b
                                       .Should(sh =>
                                           sh.QueryString(qs => qs
                                         .Fields(new[] { "orderno" })
                                         .Query("*" + txt_search.ToUpper() + "*")
                                         .Analyzer("standard")
                                       )
                                   )
                               )
                            ));
                }

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<OrderElasticsearchViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderNoSuggesstion - OrderESRepository. " + ex);
                return null;
            }

        }
       
        public async Task<List<ESHotelBookingCodeViewModel>> GetHotelBookingCode(string txt_search, int Type)
        {
            List<ESHotelBookingCodeViewModel> result = new List<ESHotelBookingCodeViewModel>();
            try
            {
                int top = 30;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<ESHotelBookingCodeViewModel>(s => s
                          .Index(index_name)
                          .Size(top)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                  q => q.Term("hotel_type", Type),
                                   q => q.Term("isdelete", "false"),
                                   sh => sh.QueryString(qs => qs
                                   .Fields(new[] { "bookingcode" })
                                   .Query("*" + txt_search.ToUpper() + "*")
                                   .Analyzer("standard")

                            )
                           )
                          )));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<ESHotelBookingCodeViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderNoSuggesstion - OrderESRepository. " + ex);
                return null;
            }

        }
    }
}
