using System.Collections.Concurrent;
using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

public sealed class InMemoryKnowledgeClient : IKnowledgeClient
{
    readonly ConcurrentDictionary<Guid, KnowledgeArticleDetailDto> _store = new();

    public InMemoryKnowledgeClient()
    {
        var a1 = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
        var a2 = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222");

        _store[a1] = new KnowledgeArticleDetailDto(
            a1,
            "How to reset your password",
            "Open the login page and click “Forgot password”.\nThen follow the steps…",
            DateTimeOffset.Now.AddDays(-2),
            new[] { "account", "login" });

        _store[a2] = new KnowledgeArticleDetailDto(
            a2,
            "VPN troubleshooting",
            "1) Check internet\n2) Verify server address\n3) Update client\n4) Retry…",
            DateTimeOffset.Now.AddDays(-7),
            new[] { "network", "vpn" });
    }

    public Task<IReadOnlyList<KnowledgeArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var list = _store.Values
            .Select(a => new KnowledgeArticleSummaryDto(a.Id, a.Title, CreatePreview(a.Body), a.UpdatedAt, a.Tags))
            .OrderByDescending(a => a.UpdatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<KnowledgeArticleSummaryDto>>(list);
    }

    public Task<KnowledgeArticleDetailDto?> GetArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue(id, out var a) ? a : null);
    }

    public Task<Guid> CreateArticleAsync(NewKnowledgeArticleDto article, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(article.Title))
            throw new ArgumentException("Title must not be empty.", nameof(article));

        var id = Guid.NewGuid();

        _store[id] = new KnowledgeArticleDetailDto(
            id,
            article.Title.Trim(),
            (article.Body ?? string.Empty).Trim(),
            DateTimeOffset.Now,
            NormalizeTags(article.Tags));

        return Task.FromResult(id);
    }

    public Task UpdateArticleAsync(Guid id, UpdateKnowledgeArticleDto article, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _store.AddOrUpdate(
            id,
            _ => throw new InvalidOperationException($"Article {id} not found."),
            (_, existing) => existing with
            {
                Title = article.Title.Trim(),
                Body = (article.Body ?? string.Empty).Trim(),
                Tags = NormalizeTags(article.Tags),
                UpdatedAt = DateTimeOffset.Now
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

    static string CreatePreview(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        const int max = 300;
        var trimmed = body.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max] + "…";
    }

    static IReadOnlyList<string> NormalizeTags(IReadOnlyList<string> tags) =>
        tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
