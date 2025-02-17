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
    public class ContractESRepository : ESRepository<ContractNoESViewModel>
    {
        private readonly string index_name = "deepseektravel_sp_getcontract";
        private readonly IConfiguration _configuration;
        public ContractESRepository(string Host, IConfiguration configuration) : base(Host)
        {

            _configuration = configuration;
            index_name = configuration["DataBaseConfig:Elastic:Index:contract"];
        }

        public async Task<List<ContractNoESViewModel>> GetContractNoSuggesstion(string txt_search, int? tenant_id = null)
        {
            List<ContractNoESViewModel> result = new List<ContractNoESViewModel>();
            try
            {
                int top = 30;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);
                if (tenant_id == null)
                {
                    var result_all = elasticClient.Search<ContractNoESViewModel>(s => s
                        .Index(index_name)
                        .Size(30)
                        .Query(b =>
                             b.Bool(p => p.Should(q =>
                                     q.QueryString(qs => qs
                                       .DefaultField(s => s.contractno)
                                       .Query("*" + txt_search.ToUpper() + "*")

                                   ))))
                         );
                    result = result_all.Documents as List<ContractNoESViewModel>;
                    return result;
                }
                var search_response = elasticClient.Search<ContractNoESViewModel>(s => s
                          .Index(index_name)
                          .Size(top)
                          .Query(b =>
                                 b.Bool(p => p
                                 .Must(mu => mu
                                   .Term(t => t
                                   .Field(f => f.tenantid)
                                   .Value(tenant_id)

                                   )
                                 )
                                 .Should(q =>
                                         q.QueryString(qs => qs
                                           .DefaultField(s => s.contractno)
                                           .Query("*" + txt_search.ToUpper() + "*")

                                        ))
                                  )        
                          ));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<ContractNoESViewModel>;
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
