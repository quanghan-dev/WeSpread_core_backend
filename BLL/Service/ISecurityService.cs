using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Service
{
    public interface ISecurityService
    {
        string SignHmacSHA256(string rawData, string secretKey);
        string GetRawDataSignature<T>(T obj, List<string> ignoreField);
    }
}
