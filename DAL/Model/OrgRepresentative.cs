using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class OrgRepresentative
    {
        public string OrgId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string IdentityCard { get; set; }
        public DateTime Birthday { get; set; }
        public string PermanentAddress { get; set; }

        public virtual Organization Org { get; set; }
    }
}
