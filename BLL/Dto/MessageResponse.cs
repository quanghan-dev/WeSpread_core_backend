using System;

namespace BLL.Dto
{
    [Serializable]
    public class MessageResponse
    {
        public MessageResponse(string to, string from, string message)
        {
            To = to;
            From = from;
            Message = message;
        }
        public string To { get; set; }
        public string From { get; set; }
        public string Message { get; set; }
        public string Sid { get; set; }
    }
}
