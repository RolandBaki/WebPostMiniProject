using DyntellProject.Core.Enums;
using System.ComponentModel.DataAnnotations;


namespace DyntellProject.Core.DTOs;

public class BlogPostDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public PostType PostType { get; set; }

    public string? CreatedByUsername { get; set; }

    public List<CommentDto> Comments { get; set; } = new();
}
