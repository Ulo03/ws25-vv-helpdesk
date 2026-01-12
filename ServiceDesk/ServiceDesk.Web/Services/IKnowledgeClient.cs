using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

public interface IKnowledgeClient
{
    Task<IReadOnlyList<KnowledgeArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken);

    Task<KnowledgeArticleDetailDto?> GetArticleAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateArticleAsync(NewKnowledgeArticleDto article, CancellationToken cancellationToken);

    Task UpdateArticleAsync(int id, UpdateKnowledgeArticleDto article, CancellationToken cancellationToken);

    Task DeleteArticleAsync(int id, CancellationToken cancellationToken);
}
