using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class RedisService : IRedisService
    {
        private readonly string ConnectionString;
        private readonly IUtilService _utilService;
        private readonly IConfiguration _configuration;
        private static ConnectionMultiplexer connection;

        public RedisService(IConfiguration configuration, IUtilService utilService)
        {
            _configuration = configuration;
            ConnectionString = _configuration.GetValue<string>("CacheSettings:ConnectionString");
            _utilService = utilService;
        }

        public IEnumerable<string> GetKeysByPattern(string pt)
        {
            CreateConnection();

            EndPoint endPoint = connection.GetEndPoints().First();
            RedisKey[] redisKeys = connection.GetServer(endPoint).Keys(pattern: pt).ToArray();

            List<string> keys = new List<string>();

            if (!_utilService.IsNullOrEmpty(redisKeys))
                foreach (RedisKey key in redisKeys)
                {
                    keys.Add(key.ToString());
                }

            CloseConnection();

            return keys;
        }

        public void CreateConnection()
        {
            connection = ConnectionMultiplexer.Connect(ConnectionString);
        }

        public void CloseConnection()
        {
            connection.Close();
        }
    }
}
