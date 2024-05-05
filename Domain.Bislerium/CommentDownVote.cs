using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Bislerium
{
    public class CommentDownVote
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime DownVoteDate { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Comment")]
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; }
    }
}
