namespace ServiceDesk.Web.Models;

public sealed record KnowledgeArticleSummaryDto(
    int Id,
    string Title,
    string Preview,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> Tags);
