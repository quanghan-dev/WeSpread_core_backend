using System;

namespace BLL.Dto.Donate
{
    [Serializable]
    public class DonateLinkResponse
    {
        public string PayUrl { get; set; }
        public string Deeplink { get; set; }
    }
}
