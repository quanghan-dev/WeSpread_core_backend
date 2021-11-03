using System;

namespace BLL.Dto.Location
{
    [Serializable]
    public class LocationRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
