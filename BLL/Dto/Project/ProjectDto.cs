using System;

namespace BLL.Dto.Project
{
    [Serializable]
    public class ProjectDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EngName { get; set; }
        public string Description { get; set; }
        public string OrgId { get; set; }
        public string Logo { get; set; }
        public string Cover { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Config { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
