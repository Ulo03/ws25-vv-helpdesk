using ServiceDesk.Contracts;

namespace ServiceDesk.Web.Services;

public interface IKnowledgeClient
{
    Task<IReadOnlyList<ArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken);

    Task<ArticleDetailDto?> GetArticleAsync(Guid id, CancellationToken cancellationToken);

    Task<Guid> CreateArticleAsync(CreateArticleDto article, CancellationToken cancellationToken);

    Task UpdateArticleAsync(Guid id, UpdateArticleDto article, CancellationToken cancellationToken);

    Task DeleteArticleAsync(Guid id, CancellationToken cancellationToken);
}
