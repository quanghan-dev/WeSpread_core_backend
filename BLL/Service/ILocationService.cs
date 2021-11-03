using BLL.Dto.Location;
using System.Collections.Generic;

namespace BLL.Service
{
    public interface ILocationService
    {
        ICollection<LocationResponse> CreateLocation(ICollection<LocationRequest> locationRequest);

        bool CheckLocationExist(string locationName);

        ICollection<LocationResponse> GetLocationByProjectId(string projectId);

        ICollection<LocationResponse> GetLocationByOrgId(string orgId);

    }
}
