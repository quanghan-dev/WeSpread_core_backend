using BLL.Dto.Location;
using System;
using System.Collections.Generic;

namespace BLL.Dto.Project
{
    [Serializable]
    public class ProjectRequest
    {
        public string ProjectName { get; set; }
        public string EngName { get; set; }
        public string Description { get; set; }
        public string OrgId { get; set; }
        public string Config { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual ICollection<string> Category { get; set; }
        public virtual ICollection<LocationRequest> Location { get; set; }
    }
}
