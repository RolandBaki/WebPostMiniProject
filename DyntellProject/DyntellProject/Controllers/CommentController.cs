using DyntellProject.Core.DTOs;
using DyntellProject.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DyntellProject.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentController> _logger;

    public CommentController(ICommentService commentService, ILogger<CommentController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    [HttpPost(Name = "PostComment")]
    [Authorize] // csak bejelentkezett user hozhat létre kommentet
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto createDto)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Comment creation failed - Invalid model state. User: {Username}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}", 
                username, createDto.BlogPostId, DateTime.UtcNow);
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Comment creation failed - User ID not found in token. Username: {Username}, Timestamp: {Timestamp}", 
                username, DateTime.UtcNow);
            return Unauthorized("User ID not found in token.");
        }

        var created = await _commentService.CreateCommentAsync(createDto, userId);
        if (created == null)
        {
            _logger.LogWarning("Comment creation failed - Invalid blog post or parent. User: {Username}, BlogPostId: {BlogPostId}, ParentCommentId: {ParentCommentId}, Timestamp: {Timestamp}", 
                username, createDto.BlogPostId, createDto.ParentCommentId, DateTime.UtcNow);
            return BadRequest("Comment could not be created (invalid blog post or parent).");
        }

        _logger.LogInformation("Comment created successfully - User: {Username}, CommentId: {CommentId}, BlogPostId: {BlogPostId}, ParentCommentId: {ParentCommentId}, Timestamp: {Timestamp}", 
            username, created.Id, createDto.BlogPostId, createDto.ParentCommentId, DateTime.UtcNow);
        
        return Ok(created);
    }

    [HttpDelete("{id}", Name ="DeleteComment")]
    [Authorize] // csak a tulajdonos törölhet (a service ezt ellenőrzi)
    public async Task<IActionResult> DeleteComment(int id)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Comment delete failed - User ID not found in token. Username: {Username}, CommentId: {CommentId}, Timestamp: {Timestamp}", 
                username, id, DateTime.UtcNow);
            return Unauthorized("User ID not found in token.");
        }

        var roleClaim = User.FindFirst("Role")?.Value;
        var isAdmin = string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);

        var deleted = await _commentService.DeleteCommentAsync(id, userId, isAdmin);
        if (!deleted)
        {
            _logger.LogWarning("Comment delete failed - Comment not found or permission denied. User: {Username}, CommentId: {CommentId}, IsAdmin: {IsAdmin}, Timestamp: {Timestamp}", 
                username, id, isAdmin, DateTime.UtcNow);
            return NotFound("Comment not found or you do not have permission to delete it.");
        }

        _logger.LogInformation("Comment deleted successfully - User: {Username}, CommentId: {CommentId}, IsAdmin: {IsAdmin}, Timestamp: {Timestamp}", 
            username, id, isAdmin, DateTime.UtcNow);
        
        return NoContent();
    }
}
