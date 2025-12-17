using DyntellProject.Core.DTOs;
using DyntellProject.Core.Entities;
using DyntellProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DyntellProject.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;

    public CommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommentDto?> CreateCommentAsync(CreateCommentDto createDto, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        // BlogPost ellenorzes
        var blogExists = await _context.BlogPosts.AnyAsync(bp => bp.Id == createDto.BlogPostId);
        if (!blogExists)
        {
            return null;
        }

        // Szulo ellenorzese
        if (createDto.ParentCommentId.HasValue)
        {
            var parentInfo = await _context.Comments
                .Where(c => c.Id == createDto.ParentCommentId.Value)
                .Select(c => new { c.Id, c.BlogPostId })
                .FirstOrDefaultAsync();

            if (parentInfo == null || parentInfo.BlogPostId != createDto.BlogPostId)
            {
                return null;
            }
        }

        var comment = new Comment
        {
            Content = createDto.Content,
            CreatedDate = DateTime.UtcNow,
            BlogPostId = createDto.BlogPostId,
            UserId = userId,
            ParentCommentId = createDto.ParentCommentId
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Uj comment lekeres
        var stored = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        if (stored == null)
        {
            return null;
        }

        var depth = await CalculateDepthAsync(stored.ParentCommentId);
        return MapCommentToDto(stored, depth);
    }

    public async Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin)
    {
        
        var comment = await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == id);

        // Csak sajat commentet, vagy admin torolhet
        if (comment == null || (comment.UserId != userId && !isAdmin))
        {
            return false;
        }

        await RemoveCommentWithRepliesAsync(comment);
        await _context.SaveChangesAsync();

        return true;
    }

    private CommentDto MapCommentToDto(Comment comment, int depth)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedDate = comment.CreatedDate,
            Username = comment.User?.UserName ?? "Ismeretlen",
            ParentCommentId = comment.ParentCommentId,
            Depth = depth,
            Replies = comment.Replies
                .Select(r => MapCommentToDto(r, depth + 1))
                .ToList()
        };
    }

    private async Task<int> CalculateDepthAsync(int? parentCommentId)
    {
        var depth = 0;
        var currentParentId = parentCommentId;

        while (currentParentId.HasValue)
        {
            depth++;
            currentParentId = await _context.Comments
                .Where(c => c.Id == currentParentId.Value)
                .Select(c => c.ParentCommentId)
                .FirstOrDefaultAsync();
        }

        return depth;
    }

    private async Task RemoveCommentWithRepliesAsync(Comment comment)
    {
        // Torlese a komment kommentnek
        await _context.Entry(comment).Collection(c => c.Replies).LoadAsync();

        foreach (var reply in comment.Replies.ToList())
        {
            await RemoveCommentWithRepliesAsync(reply);
        }

        _context.Comments.Remove(comment);
    }
}
