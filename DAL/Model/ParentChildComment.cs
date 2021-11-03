using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class ParentChildComment
    {
        public string ParentCommentId { get; set; }
        public string ChildCommentId { get; set; }

        public virtual Comment ChildComment { get; set; }
        public virtual Comment ParentComment { get; set; }
    }
}
