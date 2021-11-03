using BLL.Dto.Location;
using DAL.Model;

namespace BLL.Profile
{
    public class LocationProfile : AutoMapper.Profile
    {
        public LocationProfile()
        {
            CreateMap<LocationRequest, Location>();

            CreateMap<Location, LocationResponse>();
        }
    }
}
