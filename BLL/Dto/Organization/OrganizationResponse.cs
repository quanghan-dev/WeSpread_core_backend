using BLL.Dto.Location;
using System;
using System.Collections.Generic;

namespace BLL.Dto.Organization
{
    [Serializable]
    public class OrganizationResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EngName { get; set; }
        public string Logo { get; set; }
        public string Cover { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public string Achievement { get; set; }
        public bool IsActive { get; set; }
        public DateTime FoundingDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Config { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public string TaxCode { get; set; }
        public ICollection<LocationResponse> Location { get; set; }

    }
}