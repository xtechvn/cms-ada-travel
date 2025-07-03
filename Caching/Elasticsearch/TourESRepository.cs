using Elasticsearch.Net;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Caching.Elasticsearch
{
   public class TourESRepository : ESRepository<HotelESViewModel>
    {
        private readonly IConfiguration _configuration;
        private readonly string index_name = "deepseektravel_sp_gettour";

        public TourESRepository(string Host, IConfiguration configuration) : base(Host)
        {

            _configuration = configuration;
            index_name = configuration["DataBaseConfig:Elastic:Index:TourBooking"];
        }
        public async Task<List<TourESViewModel>> GetListProduct(string txt_search)
        {
            List<TourESViewModel> result = new List<TourESViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);
                if (txt_search == null) txt_search = "";
                var search_response = elasticClient.Search<TourESViewModel>(s => s
                         .Index(index_name)
                          .Size(top)
                          .Query(q => q
                           .QueryString(qs => qs
                               .Fields(new[] { "tourname" })
                               .Query("*" + txt_search + "*")
                               .Analyzer("standard")
                           )
                          ));
                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<TourESViewModel>;
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
