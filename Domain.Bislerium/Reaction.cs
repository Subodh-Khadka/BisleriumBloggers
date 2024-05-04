using Domain.Bislerium;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Reaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("UserId")]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    [ForeignKey("BlogId")]
    public Guid? BlogId { get; set; }
    public Blog Blog { get; set; }

    [ForeignKey("CommentId")]
    public int? CommentId { get; set; }
    public Comment Comment { get; set; }
}
