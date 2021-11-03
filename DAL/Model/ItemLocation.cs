using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class ItemLocation
    {
        public string Id { get; set; }
        public string LocationId { get; set; }
        public string OrgId { get; set; }
        public string ProjectId { get; set; }

        public virtual Location Location { get; set; }
        public virtual Organization Org { get; set; }
        public virtual Project Project { get; set; }
    }
}
