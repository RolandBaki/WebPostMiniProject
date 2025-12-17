using DyntellProject.Core.DTOs;
using DyntellProject.Core.Entities;
using DyntellProject.Core.Enums;
using DyntellProject.Infrastructure.Data;
using DyntellProject.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DyntellProject.Test;

// BlogService tesztek:
public class BlogServiceTests
{
    // InMemory EF context gyari, egyedi adatbazis nevvel, hogy a tesztek izolaltak legyenek
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetBlogPosts_Filters_ByAllowedAgeGroup_ForNonAdmin()
    {
        using var context = CreateContext(nameof(GetBlogPosts_Filters_ByAllowedAgeGroup_ForNonAdmin));

        // Seed kulonbozo PostType ertekekkel, hogy lathassuk a szurt eredmenyt
        context.BlogPosts.AddRange(
            new BlogPost { Id = 1, Title = "Gyerek poszt", Content = "c", CreatedDate = DateTime.UtcNow, PostType = PostType.Gyermek },
            new BlogPost { Id = 2, Title = "Kozelet poszt", Content = "c", CreatedDate = DateTime.UtcNow.AddMinutes(-1), PostType = PostType.Kozelet },
            new BlogPost { Id = 3, Title = "Sport poszt", Content = "c", CreatedDate = DateTime.UtcNow.AddMinutes(-2), PostType = PostType.Sport }
        );
        await context.SaveChangesAsync();

        var service = new BlogService(context);

        var result = await service.GetBlogPostsAsync(
            userId: "u1",
            userRole: UserRole.Standard,
            userAgeGroup: AgeGroup.Gyerek);

        // Gyerek korosztaly csak a Gyermek tipust lathatja
        Assert.Single(result);
        Assert.Equal(PostType.Gyermek, result[0].PostType);
    }

    [Fact]
    public async Task GetBlogPostById_Blocks_WhenAgeGroupNotAllowed()
    {
        using var context = CreateContext(nameof(GetBlogPostById_Blocks_WhenAgeGroupNotAllowed));

        // Sport posztot csak felnott/idos lat, gyerek nem
        context.BlogPosts.Add(new BlogPost
        {
            Id = 5,
            Title = "Sport poszt",
            Content = "c",
            CreatedDate = DateTime.UtcNow,
            PostType = PostType.Sport
        });
        await context.SaveChangesAsync();

        var service = new BlogService(context);

        var result = await service.GetBlogPostByIdAsync(
            id: 5,
            userRole: UserRole.Standard,
            userAgeGroup: AgeGroup.Gyerek); // gyereknek tiltott

        Assert.Null(result);
    }
}

