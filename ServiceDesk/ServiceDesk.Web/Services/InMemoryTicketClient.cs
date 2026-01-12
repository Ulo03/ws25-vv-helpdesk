using System.Collections.Concurrent;
using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

public sealed class InMemoryTicketClient : ITicketClient
{
    readonly ConcurrentDictionary<Guid, TicketDetailDto> _store = new();

    public InMemoryTicketClient()
    {
        var t1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var t2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var t3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        _store[t1] = new TicketDetailDto(
            t1,
            "Cannot login",
            "User reports login fails with correct password.",
            TicketStatus.InProgress,
            DateTimeOffset.Now.AddHours(-1),
            new List<TicketCommentDto>
            {
                new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "admin", "We are looking into it.", DateTimeOffset.Now.AddMinutes(-50))
            });

        _store[t2] = new TicketDetailDto(
            t2,
            "Feature request: dark mode",
            "Please add dark mode to the UI.",
            TicketStatus.Open,
            DateTimeOffset.Now.AddDays(-1),
            Array.Empty<TicketCommentDto>());

        _store[t3] = new TicketDetailDto(
            t3,
            "VPN not connecting",
            "Client cannot connect to VPN from home network.",
            TicketStatus.Done,
            DateTimeOffset.Now.AddDays(-3),
            new List<TicketCommentDto>
            {
                new(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "admin", "Resolved by updating client.", DateTimeOffset.Now.AddDays(-2))
            });
    }

    public Task<IReadOnlyList<TicketSummaryDto>> GetTicketsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var list = _store.Values
            .Select(t => new TicketSummaryDto(t.Id, t.Title, t.Status, t.UpdatedAt))
            .OrderByDescending(t => t.UpdatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<TicketSummaryDto>>(list);
    }

    public Task<TicketDetailDto?> GetTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue(id, out var t) ? t : null);
    }

    public Task<Guid> CreateTicketAsync(NewTicketDto ticket, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(ticket.Title))
            throw new ArgumentException("Title must not be empty.", nameof(ticket));

        var id = Guid.NewGuid();

        _store[id] = new TicketDetailDto(
            id,
            ticket.Title.Trim(),
            (ticket.Description ?? string.Empty).Trim(),
            TicketStatus.Open,
            DateTimeOffset.Now,
            Array.Empty<TicketCommentDto>());

        return Task.FromResult(id);
    }

    public Task UpdateStatusAsync(Guid ticketId, UpdateTicketStatusDto status, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _store.AddOrUpdate(
            ticketId,
            _ => throw new InvalidOperationException($"Ticket {ticketId} not found."),
            (_, existing) => existing with { Status = status.Status, UpdatedAt = DateTimeOffset.Now });

        return Task.CompletedTask;
    }

    public Task AddCommentAsync(Guid ticketId, NewCommentDto comment, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(comment.Message))
            throw new ArgumentException("Message must not be empty.", nameof(comment));

        _store.AddOrUpdate(
            ticketId,
            _ => throw new InvalidOperationException($"Ticket {ticketId} not found."),
            (_, existing) =>
            {
                var newComments = existing.Comments.Concat(new[]
                {
                    new TicketCommentDto(Guid.NewGuid(), "user", comment.Message.Trim(), DateTimeOffset.Now)
                }).ToList();

                return existing with { UpdatedAt = DateTimeOffset.Now, Comments = newComments };
            });

        return Task.CompletedTask;
    }
}
