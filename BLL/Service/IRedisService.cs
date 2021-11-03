using System.Collections.Generic;

namespace BLL.Service
{
    public interface IRedisService
    {
        void CreateConnection();

        void CloseConnection();

        IEnumerable<string> GetKeysByPattern(string pt);
    }
}
