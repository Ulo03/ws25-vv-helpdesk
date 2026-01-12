namespace ServiceDesk.Data.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public required Ticket Ticket { get; set; }
    public required User Author { get; set; }
}
