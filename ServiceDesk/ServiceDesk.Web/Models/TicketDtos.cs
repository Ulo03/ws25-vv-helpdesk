namespace ServiceDesk.Web.Models;

public sealed record TicketSummaryDto(
    int Id,
    string Title,
    TicketStatus Status,
    DateTimeOffset UpdatedAt);

public sealed record TicketCommentDto(
    int Id,
    string Author,
    string Message,
    DateTimeOffset CreatedAt);

public sealed record TicketDetailDto(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TicketCommentDto> Comments);

public sealed record NewTicketDto(string Title, string Description);

public sealed record UpdateTicketStatusDto(TicketStatus Status);

public sealed record NewCommentDto(string Message);
