using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

public interface ITicketClient
{
    Task<IReadOnlyList<TicketSummaryDto>> GetTicketsAsync(CancellationToken cancellationToken);

    Task<TicketDetailDto?> GetTicketAsync(Guid id, CancellationToken cancellationToken);

    Task<Guid> CreateTicketAsync(NewTicketDto ticket, CancellationToken cancellationToken);

    Task UpdateStatusAsync(Guid ticketId, UpdateTicketStatusDto status, CancellationToken cancellationToken);

    Task AddCommentAsync(Guid ticketId, NewCommentDto comment, CancellationToken cancellationToken);
}
