using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Bislerium.ViewModels
{
    public class DownVote
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime DownVoteDate { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Blog")]
        public Guid BlogId { get; set; }
        public Blog? Blog { get; set; }
    }
}
