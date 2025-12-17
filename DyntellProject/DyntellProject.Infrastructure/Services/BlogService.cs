using DyntellProject.Core.DTOs;
using DyntellProject.Core.Entities;
using DyntellProject.Core.Enums;
using DyntellProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DyntellProject.Infrastructure.Services;

public class BlogService : IBlogService
{
    private readonly ApplicationDbContext _context;

    public BlogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BlogPostDto>> GetBlogPostsAsync(string? userId, UserRole userRole, AgeGroup? userAgeGroup)
    {
        // alap query
        var query = _context.BlogPosts
            .Include(b => b.CreatedByUser)  // ki hozta létre
            .Include(b => b.Comments)       // kommentek
                .ThenInclude(c => c.User)  // ki irta a kommentet
            .Include(b => b.Comments)       // valaszok
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.User)
            .AsQueryable();

        // admin mindent
        if (userRole != UserRole.Admin && userAgeGroup.HasValue)
        {
            // csak a felhasznalo szamara - SQL-re fordithato szures
            var userAgeGroupValue = userAgeGroup.Value;

            // PostType-hoz tartozo engedelyezett korosztalyok a helperrel
            query = query.Where(bp => bp.PostType.GetAllowedAgeGroups().Contains(userAgeGroupValue));
        }

        // datum szerint csokkeno sorrendben
        var blogPosts = await query
            .OrderByDescending(bp => bp.CreatedDate)
            .ToListAsync();

        // entity -> dao
        return blogPosts.Select(bp => MapToDto(bp)).ToList();
    }

    public async Task<BlogPostDto?> GetBlogPostByIdAsync(int id, UserRole userRole, AgeGroup? userAgeGroup)
    {
        // Poszt betoltese kapcsolodo adatokkal
        var blogPost = await _context.BlogPosts
            .Include(b => b.CreatedByUser)
            .Include(b => b.Comments)
                .ThenInclude(c => c.User)
            .Include(b => b.Comments)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(bp => bp.Id == id);

        if (blogPost == null)
        {
            return null;
        }

        // Hozzaferes ellenorzese: Admin mindent lat, Standard csak a sajat korosztalyanak megfelelot
        if (userRole != UserRole.Admin && userAgeGroup.HasValue)
        {
            var allowedAgeGroups = blogPost.PostType.GetAllowedAgeGroups();
            if (!allowedAgeGroups.Contains(userAgeGroup.Value))
            {
                return null; // Nincs hozzaferes
            }
        }

        return MapToDto(blogPost);
    }

    public async Task<BlogPostDto> CreateBlogPostAsync(CreateBlogPostDto createDto, string userId)
    {
        var blogPost = new BlogPost
        {
            Title = createDto.Title,
            Content = createDto.Content,
            PostType = createDto.PostType,
            CreatedDate = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();

        // Ujra betoltjuk a kapcsolodo adatokkal
        var createdPost = await _context.BlogPosts
            .Include(b => b.CreatedByUser)
            .Include(b => b.Comments)
            .FirstOrDefaultAsync(bp => bp.Id == blogPost.Id);

        return MapToDto(createdPost!);
    }

    public async Task<BlogPostDto?> UpdateBlogPostAsync(int id, UpdateBlogPostDto updateDto)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return null;
        }

        // Frissites
        blogPost.Title = updateDto.Title;
        blogPost.Content = updateDto.Content;
        blogPost.PostType = updateDto.PostType;

        await _context.SaveChangesAsync();

        // Ujra betoltjuk a kapcsolodo adatokkal
        var updatedPost = await _context.BlogPosts
            .Include(b => b.CreatedByUser)
            .Include(b => b.Comments)
                .ThenInclude(c => c.User)
            .Include(b => b.Comments)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(bp => bp.Id == id);

        return MapToDto(updatedPost!);
    }

    public async Task<bool> DeleteBlogPostAsync(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return false;
        }

        _context.BlogPosts.Remove(blogPost);
        await _context.SaveChangesAsync();

        return true;
    }

    // BlogPost entity -> BlogPostDto
    private BlogPostDto MapToDto(BlogPost blogPost)
    {
        return new BlogPostDto
        {
            Id = blogPost.Id,
            Title = blogPost.Title,
            Content = blogPost.Content,
            CreatedDate = blogPost.CreatedDate,
            PostType = blogPost.PostType,
            CreatedByUsername = blogPost.CreatedByUser?.UserName ?? "Unknown",
            Comments = blogPost.Comments
                .Where(c => c.ParentCommentId == null)  
                .Select(c => MapCommentToDto(c, 0))
                .ToList()
        };
    }

    // Comment entity -> CommentDto
    private CommentDto MapCommentToDto(Comment comment, int depth)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedDate = comment.CreatedDate,
            Username = comment.User?.UserName ?? "Unknown",
            ParentCommentId = comment.ParentCommentId,
            Depth = depth,
            Replies = comment.Replies
                .Select(r => MapCommentToDto(r, depth + 1)) // rekurziv valszok
                .ToList()
        };
    }
}
