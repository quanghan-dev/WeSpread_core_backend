using System;

namespace BLL.Dto.Location
{
    [Serializable]
    public class LocationResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
