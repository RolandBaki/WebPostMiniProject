using DyntellProject.Core.Enums;
using System.ComponentModel.DataAnnotations;


namespace DyntellProject.Core.DTOs;

public class UpdateBlogPostDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public PostType PostType { get; set; }
}
