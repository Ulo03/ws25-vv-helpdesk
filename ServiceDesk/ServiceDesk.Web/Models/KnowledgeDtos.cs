namespace ServiceDesk.Web.Models;

public sealed record KnowledgeArticleSummaryDto(
    Guid Id,
    string Title,
    string Preview,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> Tags);

public sealed record KnowledgeArticleDetailDto(
    Guid Id,
    string Title,
    string Body,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> Tags);

public sealed record NewKnowledgeArticleDto(string Title, string Body, IReadOnlyList<string> Tags);

public sealed record UpdateKnowledgeArticleDto(string Title, string Body, IReadOnlyList<string> Tags);
