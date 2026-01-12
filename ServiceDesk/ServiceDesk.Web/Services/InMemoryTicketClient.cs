using System.Collections.Concurrent;
using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

// TODO: Patrick: Added for Testing and Designing, change later to API call!!

public sealed class InMemoryTicketClient : ITicketClient
{
    readonly ConcurrentDictionary<int, TicketDetailDto> store = new();

    public InMemoryTicketClient()
    {
        store[1] = new TicketDetailDto(
            1, "Cannot login", TicketStatus.InProgress, DateTimeOffset.Now.AddHours(-1),
            new List<TicketCommentDto>
            {
                new(1, "admin", "We are looking into it.", DateTimeOffset.Now.AddMinutes(-50))
            });

        store[2] = new TicketDetailDto(
            2, "Feature request: dark mode", TicketStatus.Open, DateTimeOffset.Now.AddDays(-1),
            Array.Empty<TicketCommentDto>());

        store[3] = new TicketDetailDto(
            3, "VPN not connecting", TicketStatus.Done, DateTimeOffset.Now.AddDays(-3),
            new List<TicketCommentDto>
            {
                new(2, "admin", "Resolved by updating client.", DateTimeOffset.Now.AddDays(-2))
            });
    }

    public Task<IReadOnlyList<TicketSummaryDto>> GetTicketsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var list = store.Values
            .Select(t => new TicketSummaryDto(t.Id, t.Title, t.Status, t.UpdatedAt))
            .OrderByDescending(t => t.UpdatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<TicketSummaryDto>>(list);
    }

    public Task<TicketDetailDto?> GetTicketAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(store.TryGetValue(id, out var t) ? t : null);
    }

    public Task AddCommentAsync(int ticketId, NewCommentDto comment, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(comment.Message))
            throw new ArgumentException("Message must not be empty.", nameof(comment));

        store.AddOrUpdate(
            ticketId,
            _ => throw new InvalidOperationException($"Ticket {ticketId} not found."),
            (_, existing) =>
            {
                var nextId = existing.Comments.Count == 0 ? 1 : existing.Comments.Max(c => c.Id) + 1;
                var newComments = existing.Comments.Concat(new[]
                {
                    new TicketCommentDto(nextId, "user", comment.Message.Trim(), DateTimeOffset.Now)
                }).ToList();

                return existing with
                {
                    UpdatedAt = DateTimeOffset.Now,
                    Comments = newComments
                };
            });

        return Task.CompletedTask;
    }
}
