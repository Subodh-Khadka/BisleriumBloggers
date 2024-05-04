    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Domain.Bislerium
    {
        public class Comment
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            [StringLength(1000)]
            public string Content { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? UpdateDate { get; set; }

            [ForeignKey("Blog")]
            public Guid BlogId { get; set; }

            public Blog? Blog { get; set; }

            [ForeignKey("User")]
            public string? UserId { get; set; }

            public ApplicationUser? User { get; set; }

            public double? Popularity { get; set; }
            public int? UpVote { get; set; }
            public int? DownVote { get; set; }
        }
    }
