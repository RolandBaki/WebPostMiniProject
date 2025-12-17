using DyntellProject.Core.Entities;
using DyntellProject.Core.Enums;
using DyntellProject.Infrastructure.Data;
using DyntellProject.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DyntellProject.Test;

// CommentService tesztek: letrehozas es torles jogosultsaggal
public class CommentServiceTests
{
    // InMemory EF context gyari, hogy izolalt teszteket kapjunk
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateComment_ReturnsNull_WhenBlogNotFound()
    {
        using var context = CreateContext(nameof(CreateComment_ReturnsNull_WhenBlogNotFound));
        var service = new CommentService(context);

        // Nem letezo blogra probal kommentet letrehozni -> null
        var result = await service.CreateCommentAsync(
            new Core.DTOs.CreateCommentDto { BlogPostId = 999, Content = "hi" },
            userId: "user1");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteComment_AllowsAdmin_ForForeignComment()
    {
        using var context = CreateContext(nameof(DeleteComment_AllowsAdmin_ForForeignComment));

        // Seed: egy blog es egy komment user-1 tulajdonnal
        var blog = new BlogPost { Id = 1, Title = "t", Content = "c", CreatedDate = DateTime.UtcNow, PostType = PostType.Gyermek };
        context.BlogPosts.Add(blog);
        context.Comments.Add(new Comment
        {
            Id = 10,
            BlogPostId = 1,
            Content = "orig",
            CreatedDate = DateTime.UtcNow,
            UserId = "user-1"
        });
        await context.SaveChangesAsync();

        var service = new CommentService(context);

        // Admin (isAdmin: true) torolhet mas kommentjet is
        var deleted = await service.DeleteCommentAsync(10, userId: "other-user", isAdmin: true);

        Assert.True(deleted);
        Assert.False(await context.Comments.AnyAsync(c => c.Id == 10));
    }
}