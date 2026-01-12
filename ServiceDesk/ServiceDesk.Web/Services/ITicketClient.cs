using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

public interface ITicketClient
{
    Task<IReadOnlyList<TicketSummaryDto>> GetTicketsAsync(CancellationToken cancellationToken);

    Task<TicketDetailDto?> GetTicketAsync(int id, CancellationToken cancellationToken);

    Task AddCommentAsync(int ticketId, NewCommentDto comment, CancellationToken cancellationToken);
}
