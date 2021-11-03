using BLL.Dto;
using BLL.Dto.Location;
using BLL.Dto.Organization;
using DAL.Model;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BLL.Service
{
    public interface IOrganizationService
    {
        BaseResponse<OrganizationResponse> GetById(string id);

        BaseResponse<IEnumerable<OrganizationResponse>> GetByCategory(string cateId);

        BaseResponse<IEnumerable<OrganizationResponse>> GetByLocation(string locationId);

        BaseResponse<IEnumerable<OrganizationResponse>> GetAll();

        BaseResponse<OrganizationResponse> Create(OrganizationRequest organizationCreate, string token, 
            IFormFile logo, List<IFormFile> cover);

        BaseResponse<OrganizationResponse> UpdateById(string id, OrganizationRequest organizationUpdate, string token, 
            IFormFile logo, List<IFormFile> cover);

        BaseResponse<OrganizationResponse> DeleteById(string id, string token);

        void CreateItemCategory(ICollection<string> categoryIds, string orgId);

        void CreateItemLocation(ICollection<LocationResponse> locations, string orgId);

        void UpdateItemCategory(ICollection<string> categoryIds, string orgId);

        void UpdateItemLocation(ICollection<LocationResponse> locations, string orgId);

        void SetRoleOfOrg(string userId, string orgId, string roleId);

        bool CheckUserRoleByOrgId(string userId, string orgId, string roleId);

        OrganizationResponse GetByIdAndUser(string userId, string orgId, string roleId);
    }
}
