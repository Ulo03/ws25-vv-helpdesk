namespace ServiceDesk.Web.Models;

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Done = 2
}

public sealed record TicketSummaryDto(Guid Id, string Title, TicketStatus Status, DateTimeOffset UpdatedAt);

public sealed record TicketCommentDto(Guid Id, string Author, string Message, DateTimeOffset CreatedAt);

public sealed record TicketDetailDto(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TicketCommentDto> Comments);

public sealed record NewTicketDto(string Title, string Description);

public sealed record UpdateTicketStatusDto(TicketStatus Status);

public sealed record NewCommentDto(string Message);
