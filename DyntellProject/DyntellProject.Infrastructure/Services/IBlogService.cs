using DyntellProject.Core.DTOs;
using DyntellProject.Core.Enums;

namespace DyntellProject.Infrastructure.Services;

public interface IBlogService
{
    Task<List<BlogPostDto>> GetBlogPostsAsync(string? userId, UserRole userRole, AgeGroup? userAgeGroup);
    
    Task<BlogPostDto?> GetBlogPostByIdAsync(int id, UserRole userRole, AgeGroup? userAgeGroup);
    
    Task<BlogPostDto> CreateBlogPostAsync(CreateBlogPostDto createDto, string userId);
    
    Task<BlogPostDto?> UpdateBlogPostAsync(int id, UpdateBlogPostDto updateDto);
    
    Task<bool> DeleteBlogPostAsync(int id);
}
