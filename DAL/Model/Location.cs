using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class Location
    {
        public Location()
        {
            ItemLocations = new HashSet<ItemLocation>();
            RegistrationForms = new HashSet<RegistrationForm>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public virtual ICollection<ItemLocation> ItemLocations { get; set; }
        public virtual ICollection<RegistrationForm> RegistrationForms { get; set; }
    }
}
