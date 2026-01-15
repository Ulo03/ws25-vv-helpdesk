using ServiceDesk.Contracts;

namespace ServiceDesk.ApiService.Mapping;

internal static class TicketStatusMapping
{
    internal static TicketStatus ParseEntityStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TicketStatus.Open;
        }

        // to be "safe": accept different spellings
        return value.Trim() switch
        {
            "Open" or "open" => TicketStatus.Open,
            "InProgress" or "In_Progress" or "in_progress" or "inprogress" or "In Progress" => TicketStatus.InProgress,
            "Done" or "done" => TicketStatus.Done,
            _ => TicketStatus.Open // TODO: maybe better to throw new InvalidOperationException(...) ?
        };
    }

    internal static string ToEntityStatus(TicketStatus status)
        => status switch
        {
            TicketStatus.Open => "Open",
            TicketStatus.InProgress => "In Progress",
            TicketStatus.Done => "Done",
            _ => "Open"
        };
}
