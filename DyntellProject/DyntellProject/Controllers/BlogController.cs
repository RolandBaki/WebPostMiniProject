using DyntellProject.Core.DTOs;
using DyntellProject.Core.Entities;
using DyntellProject.Core.Enums;
using DyntellProject.Infrastructure.Data;
using DyntellProject.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DyntellProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlogService _blogService;
        private readonly ILogger<BlogController> _logger;

        public BlogController(ApplicationDbContext context, IBlogService blogService, ILogger<BlogController> logger)
        {
            _context = context;
            _blogService = blogService;
            _logger = logger;
        }

        [HttpGet(Name = "GetAllBlogs")]
        [Authorize] // csak bejelentkezett
        public async Task<IActionResult> GetAllBlogs()
        {
            // JWT -> felhasznalo adatok
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst("Role")?.Value;
            var ageGroupClaim = User.FindFirst("AgeGroup")?.Value;

            // UserRole -> enum
            UserRole userRole = UserRole.Standard;
            if (Enum.TryParse<UserRole>(roleClaim, out var parsedRole))
            {
                userRole = parsedRole;
            }

            // age group -> enum
            AgeGroup? userAgeGroup = null;
            if (Enum.TryParse<AgeGroup>(ageGroupClaim, out var parsedAgeGroup))
            {
                userAgeGroup = parsedAgeGroup;
            }

            // Service -> blog posts
            var blogPosts = await _blogService.GetBlogPostsAsync(userId, userRole, userAgeGroup);
            
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            _logger.LogInformation("Blog posts retrieved successfully - User: {Username}, Role: {Role}, Count: {Count}, Timestamp: {Timestamp}", 
                username, userRole, blogPosts.Count, DateTime.UtcNow);
            
            return Ok(blogPosts);
        }

        [HttpGet("{id}", Name = "GetBlogById")]
        [Authorize] // csak bejelentkezett
        public async Task<IActionResult> GetBlogById(int id)
        {
            // JWT -> felhasznalo adatok
            var roleClaim = User.FindFirst("Role")?.Value;
            var ageGroupClaim = User.FindFirst("AgeGroup")?.Value;

            // UserRole -> enum
            UserRole userRole = UserRole.Standard;
            if (Enum.TryParse<UserRole>(roleClaim, out var parsedRole))
            {
                userRole = parsedRole;
            }

            // age group -> enum
            AgeGroup? userAgeGroup = null;
            if (Enum.TryParse<AgeGroup>(ageGroupClaim, out var parsedAgeGroup))
            {
                userAgeGroup = parsedAgeGroup;
            }

            // Service -> blog post ID alapjan
            var blogPost = await _blogService.GetBlogPostByIdAsync(id, userRole, userAgeGroup);
            
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            if (blogPost == null)
            {
                _logger.LogWarning("Blog post not found or access denied - User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}", 
                    username, userRole, id, DateTime.UtcNow);
                return NotFound("Blog post not found or you don't have access to it.");
            }

            _logger.LogInformation("Blog post retrieved successfully - User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}", 
                username, userRole, id, DateTime.UtcNow);
            return Ok(blogPost);
        }

        [HttpPost(Name = "PostBlogs")]
        [Authorize] // csak bejelentkezett
        public async Task<IActionResult> PostBlogs(CreateBlogPostDto createBlogPostDto)
        {
            // JWT -> felhasznalo adatok
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst("Role")?.Value;

            // UserRole -> enum
            UserRole userRole = UserRole.Standard;
            if (Enum.TryParse<UserRole>(roleClaim, out var parsedRole))
            {
                userRole = parsedRole;
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            
            // Csak Admin hozhat letre posztot
            if (userRole != UserRole.Admin)
            {
                _logger.LogWarning("Unauthorized create attempt - User: {Username}, Role: {Role}, Timestamp: {Timestamp}", 
                    username, userRole, DateTime.UtcNow);
                return StatusCode(StatusCodes.Status403Forbidden, "Only administrators can create blog posts.");
            }

            // Ha nincs userId, akkor hiba
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Create blog post failed - User ID not found in token. Username: {Username}, Timestamp: {Timestamp}", 
                    username, DateTime.UtcNow);
                return Unauthorized("User ID not found in token.");
            }

            // Service -> poszt letrehozasa
            var blogPost = await _blogService.CreateBlogPostAsync(createBlogPostDto, userId);
            
            _logger.LogInformation("Blog post created successfully - User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Title: {Title}, Timestamp: {Timestamp}", 
                username, userRole, blogPost.Id, blogPost.Title, DateTime.UtcNow);
            
            return Ok(blogPost);
        }

        [HttpPut("{id}", Name = "UpdateBlog")]
        [Authorize] // csak bejelentkezett
        public async Task<IActionResult> UpdateBlog(int id, UpdateBlogPostDto updateBlogPostDto)
        {
            // JWT -> felhasznalo adatok
            var roleClaim = User.FindFirst("Role")?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            // UserRole -> enum
            UserRole userRole = UserRole.Standard;
            if (Enum.TryParse<UserRole>(roleClaim, out var parsedRole))
            {
                userRole = parsedRole;
            }

            // Csak Admin szerkeszthet posztot
            if (userRole != UserRole.Admin)
            {
                // Logolás: Standard felhasználó próbált szerkeszteni
                _logger.LogWarning(
                    "Unauthorized edit attempt - User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}",
                    username, userRole, id, DateTime.UtcNow);
                
                return StatusCode(StatusCodes.Status403Forbidden, "Only administrators can update blog posts.");
            }

            // Service -> poszt frissitese
            var blogPost = await _blogService.UpdateBlogPostAsync(id, updateBlogPostDto);
            
            if (blogPost == null)
            {
                _logger.LogWarning("Blog post update failed - Blog post not found. User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}", 
                    username, userRole, id, DateTime.UtcNow);
                return NotFound("Blog post not found.");
            }

            _logger.LogInformation("Blog post updated successfully - User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Title: {Title}, Timestamp: {Timestamp}", 
                username, userRole, id, blogPost.Title, DateTime.UtcNow);
            
            return Ok(blogPost);
        }

        [HttpDelete("{id}", Name = "DeleteBlog")]
        [Authorize] // csak bejelentkezett
        public async Task<IActionResult> DeleteBlog(int id)
        {
            // JWT -> felhasznalo adatok
            var roleClaim = User.FindFirst("Role")?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            // UserRole -> enum
            UserRole userRole = UserRole.Standard;
            if (Enum.TryParse<UserRole>(roleClaim, out var parsedRole))
            {
                userRole = parsedRole;
            }

            // Csak Admin torolhet posztot
            if (userRole != UserRole.Admin)
            {
                // Logolás: Standard felhasználó próbált törölni
                _logger.LogWarning(
                    "Unauthorized delete attempt - User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}",
                    username, userRole, id, DateTime.UtcNow);
                
                return StatusCode(StatusCodes.Status403Forbidden, "Only administrators can delete blog posts.");
            }

            // Service -> poszt torlese
            var deleted = await _blogService.DeleteBlogPostAsync(id);
            
            if (!deleted)
            {
                _logger.LogWarning("Blog post delete failed - Blog post not found. User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}", 
                    username, userRole, id, DateTime.UtcNow);
                return NotFound("Blog post not found.");
            }

            _logger.LogInformation("Blog post deleted successfully - User: {Username}, Role: {Role}, BlogPostId: {BlogPostId}, Timestamp: {Timestamp}", 
                username, userRole, id, DateTime.UtcNow);
            
            return NoContent(); // 204 No Content
        }

    }
}
