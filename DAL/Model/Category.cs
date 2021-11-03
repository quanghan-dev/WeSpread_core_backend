using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class Category
    {
        public Category()
        {
            InverseParent = new HashSet<Category>();
            ItemCategories = new HashSet<ItemCategory>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public bool IsActive { get; set; }
        public string IconUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> InverseParent { get; set; }
        public virtual ICollection<ItemCategory> ItemCategories { get; set; }
    }
}
