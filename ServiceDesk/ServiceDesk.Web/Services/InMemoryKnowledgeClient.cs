using System.Collections.Concurrent;
using ServiceDesk.Contracts;

namespace ServiceDesk.Web.Services;

public sealed class InMemoryKnowledgeClient : IKnowledgeClient
{
    readonly ConcurrentDictionary<Guid, ArticleDetailDto> _store = new();

    public InMemoryKnowledgeClient()
    {
        var a1 = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
        var a2 = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222");

        var now = DateTimeOffset.UtcNow;

        _store[a1] = new ArticleDetailDto(
            a1,
            "How to reset your password",
            "Open the login page and click “Forgot password”.\nThen follow the steps…",
            now.AddDays(-3),
            now.AddDays(-2),
            "admin");

        _store[a2] = new ArticleDetailDto(
            a2,
            "VPN troubleshooting",
            "1) Check internet\n2) Verify server address\n3) Update client\n4) Retry…",
            now.AddDays(-8),
            now.AddDays(-7),
            "admin");
    }

    public Task<IReadOnlyList<ArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var list = _store.Values
            .Select(a => new ArticleSummaryDto(a.Id, a.Title, CreatePreview(a.Content), a.UpdatedAt, a.AuthorUsername))
            .OrderByDescending(a => a.UpdatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<ArticleSummaryDto>>(list);
    }

    public Task<ArticleDetailDto?> GetArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue(id, out var a) ? a : null);
    }

    public Task<Guid> CreateArticleAsync(CreateArticleDto article, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(article.Title) || article.Title.Trim().Length < 3)
            throw new ArgumentException("Title must be at least 3 characters long.", nameof(article));

        if (string.IsNullOrWhiteSpace(article.Content) || article.Content.Trim().Length < 10)
            throw new ArgumentException("Content must be at least 10 characters long.", nameof(article));

        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        _store[id] = new ArticleDetailDto(
            id,
            article.Title.Trim(),
            article.Content.Trim(),
            now,
            now,
            "admin");

        return Task.FromResult(id);
    }

    public Task UpdateArticleAsync(Guid id, UpdateArticleDto article, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _store.AddOrUpdate(
            id,
            _ => throw new InvalidOperationException($"Article {id} not found."),
            (_, existing) => existing with
            {
                Title = article.Title.Trim(),
                Content = article.Content.Trim(),
                UpdatedAt = DateTimeOffset.UtcNow
            });

        return Task.CompletedTask;
    }

    public Task DeleteArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryRemove(id, out _))
            throw new InvalidOperationException($"Article {id} not found.");

        return Task.CompletedTask;
    }

    static string CreatePreview(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        const int max = 140;
        var trimmed = content.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max] + "…";
    }
}
