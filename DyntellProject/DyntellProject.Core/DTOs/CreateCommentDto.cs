using System.ComponentModel.DataAnnotations;

namespace DyntellProject.Core.DTOs;

public class CreateCommentDto
{
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public int BlogPostId { get; set; }
    
    public int? ParentCommentId { get; set; }
}




