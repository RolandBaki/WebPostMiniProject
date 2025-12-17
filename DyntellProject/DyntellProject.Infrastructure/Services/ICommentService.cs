using DyntellProject.Core.DTOs;

namespace DyntellProject.Infrastructure.Services;

public interface ICommentService
{
    Task<CommentDto?> CreateCommentAsync(CreateCommentDto createDto, string userId);
    Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin);
}
