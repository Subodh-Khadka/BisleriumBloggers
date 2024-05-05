using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Bislerium
{
    public class Blog
    {
        [Key]
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }
        public double? Popularity { get; set; }
        public int? UpVote { get; set; }
        public int? DownVote { get; set; }
        public int? CommentCount {  get; set; }
        public string? Image { get; set; }

        // Navigation property for comments
        public ICollection<Comment> Comments { get; set; }

    }
}
