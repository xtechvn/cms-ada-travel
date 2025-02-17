using Entities.ViewModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using Telegram.Bot.Types;
using Utilities;
using Utilities.Contants;

namespace APP_CHECKOUT.RabitMQ
{
    public class WorkQueueClient
    {
        private readonly QueueSettingViewModel queue_setting;
        private readonly QueueSettingViewModel queue_synces_setting;
        private readonly ConnectionFactory factory;
        private readonly ConnectionFactory factory_sync_es;
        private readonly IConfiguration _configuration;

        public WorkQueueClient(IConfiguration configuration)
        {
            _configuration = configuration;
            queue_setting = new QueueSettingViewModel()
            {
                host = _configuration["Queue:Host"],
                port = Convert.ToInt32(_configuration["Queue:Port"]),
                v_host = _configuration["Queue:V_Host"],
                username = _configuration["Queue:Username"],
                password = _configuration["Queue:Password"],
            };
            queue_synces_setting = new QueueSettingViewModel()
            {
                host = _configuration["QueueSyncES:Host"],
                port = Convert.ToInt32(_configuration["QueueSyncES:Port"]),
                v_host = _configuration["QueueSyncES:V_Host"],
                username = _configuration["QueueSyncES:Username"],
                password = _configuration["QueueSyncES:Password"],
            };
            factory_sync_es = new ConnectionFactory()
            {
                HostName = queue_synces_setting.host,
                UserName = queue_synces_setting.username,
                Password = queue_synces_setting.password,
                VirtualHost = queue_synces_setting.v_host,
                Port = Protocols.DefaultProtocol.DefaultPort
            };factory = new ConnectionFactory()
            {
                HostName = queue_setting.host,
                UserName = queue_setting.username,
                Password = queue_setting.password,
                VirtualHost = queue_setting.v_host,
                Port = Protocols.DefaultProtocol.DefaultPort
            };
        }
        public bool SyncES(long id, string store_procedure, string index_es)
        {
            try
            {
                var j_param = new Dictionary<string, object>
                              {
                                { "store_name", store_procedure },
                                { "index_es",index_es },
                                { "project_type", Convert.ToInt16(ProjectType.DEEPSEEK_CMS) },
                                { "id", id }
                              };
                var message = JsonConvert.SerializeObject(j_param);

                using (var connection = factory_sync_es.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    try
                    {
                        channel.QueueDeclare(queue: _configuration["QueueSyncES:QueueSyncES"],
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                        arguments: null);

                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                                             routingKey: _configuration["QueueSyncES:QueueSyncES"],
                                             basicProperties: null,
                                             body: body);
                        return true;

                    }
                    catch (Exception ex)
                    {


                        return false;
                    }
                }

            }
            catch (Exception ex)
            {


            }
            return false;
        }
        public bool InsertQueueSimple(string message, string queueName)
        {            
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);
                    return true;

                }
                catch (Exception ex)
                {


                    return false;
                }
            }
        }
        public bool InsertQueueSimpleDurable( string message, string queueName)
        {
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);
                    return true;

                }
                catch (Exception ex)
                {
                    
                    return false;
                }
            }
        }
    }
}
