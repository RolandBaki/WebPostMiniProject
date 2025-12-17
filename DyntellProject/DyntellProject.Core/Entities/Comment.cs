using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DyntellProject.Core.Entities;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public int BlogPostId { get; set; }
    public virtual BlogPost BlogPost { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;

    public int? ParentCommentId { get; set; }
    public virtual Comment? ParentComment { get; set; }

    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

    [NotMapped]
    public int Depth { get; set; }
}
