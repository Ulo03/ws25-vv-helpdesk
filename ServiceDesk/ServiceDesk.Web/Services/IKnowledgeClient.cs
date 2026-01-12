using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

public interface IKnowledgeClient
{
    Task<IReadOnlyList<KnowledgeArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken);

    Task<KnowledgeArticleDetailDto?> GetArticleAsync(Guid id, CancellationToken cancellationToken);

    Task<Guid> CreateArticleAsync(NewKnowledgeArticleDto article, CancellationToken cancellationToken);

    Task UpdateArticleAsync(Guid id, UpdateKnowledgeArticleDto article, CancellationToken cancellationToken);

    Task DeleteArticleAsync(Guid id, CancellationToken cancellationToken);
}
