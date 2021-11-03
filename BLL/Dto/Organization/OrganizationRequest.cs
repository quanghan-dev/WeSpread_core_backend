using BLL.Dto.Location;
using System;
using System.Collections.Generic;

namespace BLL.Dto.Organization
{
    [Serializable]
    public class OrganizationRequest
    {
        public string OrgName { get; set; }
        public string EngName { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public string Achievement { get; set; }
        public DateTime FoundingDate { get; set; }
        public string TaxCode { get; set; }

        public ICollection<string> Category { get; set; }
        public ICollection<LocationRequest> Location { get; set; }
    }
}