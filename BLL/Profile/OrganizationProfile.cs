using BLL.Dto.Organization;
using DAL.Model;

namespace BLL.Profile
{
    public class OrganizationProfile : AutoMapper.Profile
    {
        public OrganizationProfile()
        {
            CreateMap<OrganizationRequest, Organization>();

            CreateMap<Organization, OrganizationResponse>();

            CreateMap<OrganizationResponse, Organization>();

        }
    }
}
