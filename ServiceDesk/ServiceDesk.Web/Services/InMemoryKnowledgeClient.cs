using ServiceDesk.Web.Models;
using System.Collections.Concurrent;

namespace ServiceDesk.Web.Services;

// TODO: Patrick: Added for Testing and Designing, change later to API call!!

public sealed class InMemoryKnowledgeClient : IKnowledgeClient
{
    readonly ConcurrentDictionary<int, KnowledgeArticleDetailDto> store = new();
    int nextId;

    public InMemoryKnowledgeClient()
    {
        store[1] = new KnowledgeArticleDetailDto(
            1,
            "How to reset your password",
            "Open the login page and click “Forgot password”.\nThen follow the steps…",
            DateTimeOffset.Now.AddDays(-2),
            new[] { "account", "login" });

        store[2] = new KnowledgeArticleDetailDto(
            2,
            "VPN troubleshooting",
            "1) Check internet\n2) Verify server address\n3) Update client\n4) Retry…",
            DateTimeOffset.Now.AddDays(-7),
            new[] { "network", "vpn" });

        nextId = store.Keys.Max();
    }

    public Task<IReadOnlyList<KnowledgeArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var list = store.Values
            .Select(a => new KnowledgeArticleSummaryDto(
                a.Id,
                a.Title,
                CreatePreview(a.Body),
                a.UpdatedAt,
                a.Tags))
            .OrderByDescending(a => a.UpdatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<KnowledgeArticleSummaryDto>>(list);
    }

    public Task<KnowledgeArticleDetailDto?> GetArticleAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(store.TryGetValue(id, out var a) ? a : null);
    }

    public Task<int> CreateArticleAsync(NewKnowledgeArticleDto article, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(article.Title))
            throw new ArgumentException("Title must not be empty.", nameof(article));

        var id = Interlocked.Increment(ref nextId);

        store[id] = new KnowledgeArticleDetailDto(
            id,
            article.Title.Trim(),
            (article.Body ?? string.Empty).Trim(),
            DateTimeOffset.Now,
            NormalizeTags(article.Tags));

        return Task.FromResult(id);
    }

    public Task UpdateArticleAsync(int id, UpdateKnowledgeArticleDto article, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        store.AddOrUpdate(
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

    public Task DeleteArticleAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!store.TryRemove(id, out _))
            throw new InvalidOperationException($"Article {id} not found.");

        return Task.CompletedTask;
    }

    static string CreatePreview(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        const int max = 300;
        var trimmed = body.Trim();
        return trimmed.Length <= max ? trimmed : trimmed.Substring(0, max) + "…";
    }

    static IReadOnlyList<string> NormalizeTags(IReadOnlyList<string> tags)
    {
        var normalized = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalized;
    }
}
