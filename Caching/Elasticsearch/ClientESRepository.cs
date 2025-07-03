using Elasticsearch.Net;
using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Caching.Elasticsearch
{
    public class ClientESRepository : ESRepository<CustomerESViewModel>
    {
        private readonly string index_name = "deepseektravel_sp_getclient";
        private readonly IConfiguration _configuration;
        public ClientESRepository(string Host, IConfiguration configuration) : base(Host)
        {

            _configuration = configuration;
            index_name = configuration["DataBaseConfig:Elastic:Index:Client"];
        }


        public async Task<List<CustomerESViewModel>> GetClientSuggesstion(string txt_search, int? tenant_id = null)
        {
            List<CustomerESViewModel> result = new List<CustomerESViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);
                if (tenant_id == null)
                {
                    var result_all = elasticClient.Search<CustomerESViewModel>(s => s
                        .Index(index_name)
                        .Size(30)
                        .Query(q => q.Bool(qb => qb.Should(q => q.MatchAll())
                                      
                                )
                           ));
                    result = result_all.Documents as List<CustomerESViewModel>;
                    return result;
                }
                if (txt_search == null)
                {
                    var result_all = elasticClient.Search<CustomerESViewModel>(s => s
                          .Index(index_name)
                          .Size(30)
                          .Query(q =>
                                    q.Bool(qb => qb.Should(q => q.MatchAll())
                                       .Must(mu => mu
                                            .Term(t => t
                                                .Field(f => f.tenantid)
                                                .Value(tenant_id)
                                            )
                                        )
                                )
                           ));
                    result = result_all.Documents as List<CustomerESViewModel>;
                    return result;
                }
                var search_response = elasticClient.Search<CustomerESViewModel>(s => s
                          .Index(index_name)
                          .Size(top)
                          .Query(q =>
                            q.Bool(
                                qb => qb.Should(
                                    sh => sh.QueryString(m => m
                                    .DefaultField(f => f.phone)
                                    .Query("*" + txt_search + "*")),
                                    sh => sh.Match(m => m
                                    .Field(f => f.email)
                                    .Query("*" + txt_search + "*")),
                                    sh => sh.QueryString(m => m
                                    .DefaultField(f => f.clientname)
                                    .Query("*" + txt_search + "*")),
                                      sh => sh.QueryString(m => m
                                    .DefaultField(f => f.clientcode)
                                    .Query("*" + txt_search + "*"))

                                )
                                .Must(mu => mu
                                            .Term(t => t
                                                .Field(f => f.tenantid)
                                                .Value(tenant_id)
                                            )
                                        )
                                )
                           ));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<CustomerESViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public int UpSert(ClientESViewModel entity, string index_name = "deepseektravel_sp_getclient")

        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);
                var indexResponse = elasticClient.Index(new IndexRequest<ClientESViewModel>(entity, index_name + (_company_type.Trim() == "0" ? "" : "_" + _company_type.Trim())));

                if (!indexResponse.IsValid)
                {

                    return 0;
                }

                return 1;
            }
            catch
            {
                return 0;
            }
        }
    }
}
