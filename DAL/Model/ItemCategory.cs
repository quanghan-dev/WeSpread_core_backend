using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class ItemCategory
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string CategoryId { get; set; }
        public string OrgId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Organization Org { get; set; }
        public virtual Project Project { get; set; }
    }
}
