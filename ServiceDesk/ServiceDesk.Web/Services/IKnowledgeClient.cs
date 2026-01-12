using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

public interface IKnowledgeClient
{
    Task<IReadOnlyList<KnowledgeArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken);
}
