using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.ApiService.Mapping;
using ServiceDesk.ApiService.Validation;
using ServiceDesk.Contracts;
using ServiceDesk.Data;
using ServiceDesk.Data.Entities;

namespace ServiceDesk.ApiService.Endpoints;

public static class KnowledgeEndpoints
{
    public static RouteGroupBuilder MapKnowledgeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/knowledge").WithTags("Knowledge");

        group.MapGet("/", GetArticlesAsync);
        group.MapGet("/{id:guid}", GetArticleByIdAsync);
        group.MapPost("/", CreateArticleAsync);
        group.MapPut("/{id:guid}", UpdateArticleAsync);
        group.MapDelete("/{id:guid}", DeleteArticleAsync);

        return group;
    }

    static async Task<IResult> GetArticlesAsync(ServiceDeskDbContext db, CancellationToken ct)
    {
        var articles = await db.Articles
            .AsNoTracking()
            .Include(a => a.Author)
            .ToListAsync(ct);

        var items = articles
            .OrderByDescending(GetArticleUpdatedAt)
            .Select(MapArticleSummary)
            .ToList();

        return Results.Ok(items);
    }

    static async Task<IResult> GetArticleByIdAsync(Guid id, ServiceDeskDbContext db, CancellationToken ct)
    {
        var article = await db.Articles
            .AsNoTracking()
            .Include(a => a.Author)
            .SingleOrDefaultAsync(a => a.Id == id, ct);

        if (article is null)
        {
            return Results.NotFound();
        }

        var dto = new ArticleDetailDto(
            article.Id,
            article.Title,
            article.Content,
            DateTimeMapping.ToDateTimeOffset(article.CreatedAt),
            DateTimeMapping.ToDateTimeOffset(article.UpdatedAt),
            article.Author.Username);

        return Results.Ok(dto);
    }

    static async Task<IResult> CreateArticleAsync(
        HttpContext httpContext,
        [FromBody] CreateArticleDto body,
        ServiceDeskDbContext db,
        CancellationToken ct)
    {
        var errors = ValidateCreateArticle(body);
        if (ValidationErrors.HasErrors(errors))
        {
            return Results.ValidationProblem(errors);
        }

        var author = await TryResolveCurrentUserAsync(httpContext, db, ct);
        if (author is null)
        {
            return Results.Problem(
                detail: "No user exists in database. Seed a default admin user.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var now = DateTime.UtcNow;

        var entity = new Article
        {
            Id = Guid.NewGuid(),
            Title = body.Title.Trim(),
            Content = body.Content.Trim(),
            Author = author,
            AuthorId = author.Id,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Articles.Add(entity);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/knowledge/{entity.Id}", new CreateResultDto(entity.Id));
    }

    static async Task<IResult> UpdateArticleAsync(
        Guid id,
        [FromBody] UpdateArticleDto body,
        ServiceDeskDbContext db,
        CancellationToken ct)
    {
        var errors = ValidateUpdateArticle(body);
        if (ValidationErrors.HasErrors(errors))
        {
            return Results.ValidationProblem(errors);
        }

        var entity = await db.Articles.SingleOrDefaultAsync(a => a.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }

        entity.Title = body.Title.Trim();
        entity.Content = body.Content.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    static async Task<IResult> DeleteArticleAsync(Guid id, ServiceDeskDbContext db, CancellationToken ct)
    {
        var entity = await db.Articles.SingleOrDefaultAsync(a => a.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }

        db.Articles.Remove(entity);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    static ArticleSummaryDto MapArticleSummary(Article a)
        => new(
            a.Id,
            a.Title,
            PreviewMapping.CreatePreview(a.Content),
            DateTimeMapping.ToDateTimeOffset(a.UpdatedAt),
            a.Author.Username);

    static DateTime GetArticleUpdatedAt(Article a) => a.UpdatedAt;

    static Dictionary<string, string[]> ValidateCreateArticle(CreateArticleDto dto)
    {
        var errors = ValidationErrors.Create();

        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Trim().Length < 3)
        {
            ValidationErrors.Add(errors, nameof(dto.Title), "Title must be at least 3 characters long.");
        }

        if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Trim().Length < 10)
        {
            ValidationErrors.Add(errors, nameof(dto.Content), "Content must be at least 10 characters long.");
        }

        return errors;
    }

    static Dictionary<string, string[]> ValidateUpdateArticle(UpdateArticleDto dto)
        => ValidateCreateArticle(new CreateArticleDto(dto.Title, dto.Content));

    static async Task<User?> TryResolveCurrentUserAsync(HttpContext httpContext, ServiceDeskDbContext db, CancellationToken ct)
    {
        var username = httpContext.User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(username))
        {
            return await db.Users.SingleOrDefaultAsync(u => u.Username == username, ct);
        }

        var admin = await db.Users.SingleOrDefaultAsync(u => u.Username == "admin", ct);
        if (admin is not null)
        {
            return admin;
        }

        return await db.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync(ct);
    }
}
