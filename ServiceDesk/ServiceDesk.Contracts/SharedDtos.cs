namespace ServiceDesk.Contracts;

public enum TicketStatus { Open, InProgress, Done }

public sealed record TicketSummaryDto(Guid Id, string Title, TicketStatus Status, DateTimeOffset UpdatedAt);

public sealed record TicketCommentDto(Guid Id, string AuthorUsername, string Content, DateTimeOffset CreatedAt);

public sealed record TicketDetailDto(
    Guid Id,
    string Title,
    string? Description,
    TicketStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string CreatedByUsername,
    string? AssignedToUsername,
    IReadOnlyList<TicketCommentDto> Comments);

public sealed record CreateTicketDto(string Title, string? Description);

public sealed record UpdateTicketStatusDto(TicketStatus Status);

public sealed record CreateCommentDto(string Content);

public sealed record ArticleSummaryDto(
    Guid Id,
    string Title,
    string Preview,
    DateTimeOffset UpdatedAt,
    string AuthorUsername);

public sealed record ArticleDetailDto(
    Guid Id,
    string Title,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string AuthorUsername);

public sealed record CreateArticleDto(string Title, string Content);

public sealed record UpdateArticleDto(string Title, string Content);

public sealed record CreateResultDto(Guid Id);
