namespace ServiceDesk.Web.Models;

public sealed record KnowledgeArticleSummaryDto(
    int Id,
    string Title,
    string Preview,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> Tags);

public sealed record KnowledgeArticleDetailDto(
    int Id,
    string Title,
    string Body,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> Tags);

public sealed record NewKnowledgeArticleDto(
    string Title, 
    string Body, 
    IReadOnlyList<string> Tags);

public sealed record UpdateKnowledgeArticleDto(
    string Title, 
    string Body, 
    IReadOnlyList<string> Tags);
