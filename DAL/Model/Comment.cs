using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class Comment
    {
        public Comment()
        {
            ParentChildCommentChildComments = new HashSet<ParentChildComment>();
            ParentChildCommentParentComments = new HashSet<ParentChildComment>();
        }

        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CommentatorId { get; set; }
        public string ProjectId { get; set; }

        public virtual AppUser Commentator { get; set; }
        public virtual Organization CommentatorNavigation { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<ParentChildComment> ParentChildCommentChildComments { get; set; }
        public virtual ICollection<ParentChildComment> ParentChildCommentParentComments { get; set; }
    }
}
