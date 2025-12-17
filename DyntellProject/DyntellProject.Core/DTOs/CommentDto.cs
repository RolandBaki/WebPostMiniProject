

namespace DyntellProject.Core.DTOs;

public class CommentDto
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public string Username { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }

    public int Depth { get; set; }

    public List<CommentDto> Replies { get; set; } = new();
}
