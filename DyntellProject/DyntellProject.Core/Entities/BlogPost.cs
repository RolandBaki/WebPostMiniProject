using DyntellProject.Core.Enums;
using System.ComponentModel.DataAnnotations;


namespace DyntellProject.Core.Entities;

public class BlogPost
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public PostType PostType { get; set; }

    public string? CreatedByUserId { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
