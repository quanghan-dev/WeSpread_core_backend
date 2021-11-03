using Newtonsoft.Json;
using System;

namespace BLL.Dto
{
    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BaseResponse<T> where T : class
    {
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public T Data { get; set; }
    }
}
