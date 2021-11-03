using BLL.Dto;
using BLL.Dto.Location;
using BLL.Dto.Project;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BLL.Service
{
    public interface IProjectService
    {
        BaseResponse<ProjectResponse> CreateProject(ProjectRequest projectRequest, string token,
            IFormFile logo, List<IFormFile> cover);

        BaseResponse<ProjectResponse> UpdateProject(string id, ProjectRequest projectRequest, string token,
            IFormFile logo, List<IFormFile> cover);

        BaseResponse<ProjectResponse> DeleteProject(string id, string token);

        BaseResponse<ProjectResponse> GetProjectById(string id);

        BaseResponse<IEnumerable<ProjectResponse>> GetAllProject();

        BaseResponse<IEnumerable<ProjectResponse>> GetProjectByOrg(string orgId);

        BaseResponse<IEnumerable<ProjectResponse>> GetProjectByCategory(string cateId);

        BaseResponse<IEnumerable<ProjectResponse>> GetProjectByLocation(string locationId);

        Period GetProjectPeriod(string projectId);

        void CreateItemLocation(ICollection<LocationResponse> locations, string projectId);

        void CreateItemCategory(ICollection<string> categoryIds, string projectId);

        void UpdateItemCategory(ICollection<string> categoryIds, string projectId);

        void UpdateItemLocation(ICollection<LocationResponse> locations, string projectId);

        ProjectResponse CheckRoleInOrgByProIdAndUserId(string userId, string proId, string roleId);
    }
}
