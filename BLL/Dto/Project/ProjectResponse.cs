using BLL.Dto.Location;
using BLL.Dto.Organization;
using System;
using System.Collections.Generic;

namespace BLL.Dto.Project
{
    [Serializable]
    public class ProjectResponse : ProjectDto
    {
        public OrganizationResponse Org { get; set; }

        public IEnumerable<LocationResponse> Location { get; set; }
    }
}
