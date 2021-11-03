using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Dto.Payment.MoMo.IPN
{
    public class MoMoIPNResponse
    {
        public string partnerCode { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; }
        public long responseTime { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
    }
}
