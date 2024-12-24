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
   public class HotelBookingESRepository : ESRepository<HotelBookingESViewModel>
    {
        private readonly IConfiguration _configuration;
        private readonly string index_name = "adavigo_sp_gethotelbooking";
        public HotelBookingESRepository(string Host, IConfiguration configuration) : base(Host)
        {

            _configuration = configuration;
            index_name = configuration["DataBaseConfig:Elastic:Index:HotelBooking"];
        }

        public async Task<List<HotelBookingESViewModel>> GetListProduct(string txt_search)
        {
            List<HotelBookingESViewModel> result = new List<HotelBookingESViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<HotelBookingESViewModel>(s => s
                          .Index(index_name)
                          .Size(top)
                          .Query(q => q
                           .QueryString(qs => qs
                               .Fields(new[] { "servicecode"})
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
                    result = search_response.Documents as List<HotelBookingESViewModel>;
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
