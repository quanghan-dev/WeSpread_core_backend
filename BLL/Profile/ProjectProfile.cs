using BLL.Dto.Project;
using DAL.Model;

namespace BLL.Profile
{
    public class ProjectProfile : AutoMapper.Profile
    {
        public ProjectProfile()
        {
            CreateMap<ProjectRequest, Project>();

            CreateMap<Project, ProjectResponse>();

            CreateMap<ProjectResponse, Project>();
            
        }
    }
}
