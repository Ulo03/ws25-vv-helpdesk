using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.ApiService.Mapping;
using ServiceDesk.ApiService.Validation;
using ServiceDesk.Contracts;
using ServiceDesk.Data;
using ServiceDesk.Data.Entities;

namespace ServiceDesk.ApiService.Endpoints;

public static class TicketsEndpoints
{
    public static RouteGroupBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tickets").WithTags("Tickets");

        group.MapGet("/", GetTicketsAsync);
        group.MapGet("/{id:guid}", GetTicketByIdAsync);
        group.MapPost("/", CreateTicketAsync);
        group.MapPatch("/{id:guid}/status", UpdateTicketStatusAsync);
        group.MapPost("/{id:guid}/comments", CreateCommentAsync);

        return group;
    }

    static async Task<IResult> GetTicketsAsync(ServiceDeskDbContext db, CancellationToken ct)
    {
        var tickets = await db.Tickets
            .AsNoTracking()
            .ToListAsync(ct);

        var items = tickets
            .OrderByDescending(GetTicketUpdatedAt)
            .Select(MapTicketSummary)
            .ToList();

        return Results.Ok(items);
    }

    static async Task<IResult> GetTicketByIdAsync(Guid id, ServiceDeskDbContext db, CancellationToken ct)
    {
        // Include: CreatedBy, AssignedTo, Comments + Comment.Author
        var ticket = await db.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .SingleOrDefaultAsync(t => t.Id == id, ct);

        if (ticket is null)
        {
            return Results.NotFound();
        }

        var dto = new TicketDetailDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            TicketStatusMapping.ParseEntityStatus(ticket.Status),
            DateTimeMapping.ToDateTimeOffset(ticket.CreatedAt),
            DateTimeMapping.ToDateTimeOffset(ticket.UpdatedAt),
            ticket.CreatedBy.Username,
            ticket.AssignedTo?.Username,
            ticket.Comments
                .OrderBy(GetCommentCreatedAt)
                .Select(MapTicketComment)
                .ToList());

        return Results.Ok(dto);
    }

    static async Task<IResult> CreateTicketAsync(
        HttpContext httpContext,
        [FromBody] CreateTicketDto body,
        ServiceDeskDbContext db,
        CancellationToken ct)
    {
        var errors = ValidateCreateTicket(body);
        if (ValidationErrors.HasErrors(errors))
        {
            return Results.ValidationProblem(errors);
        }

        var currentUser = await TryResolveCurrentUserAsync(httpContext, db, ct);
        if (currentUser is null)
        {
            return Results.Problem(
                detail: "No user exists in database. Seed a default admin user.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var now = DateTime.UtcNow;

        var entity = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = body.Title.Trim(),
            Description = NormalizeOptional(body.Description),
            Status = TicketStatusMapping.ToEntityStatus(TicketStatus.Open),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedById = currentUser.Id,
            CreatedBy = currentUser
        };

        db.Tickets.Add(entity);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/tickets/{entity.Id}", new CreateResultDto(entity.Id));
    }

    static async Task<IResult> UpdateTicketStatusAsync(
        Guid id,
        [FromBody] UpdateTicketStatusDto body,
        ServiceDeskDbContext db,
        CancellationToken ct)
    {
        // Enum validated on the JSON side; body can still be null/empty -> Minimal API usually binds 400 itself.
        var ticket = await db.Tickets.SingleOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null)
        {
            return Results.NotFound();
        }

        ticket.Status = TicketStatusMapping.ToEntityStatus(body.Status);
        ticket.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    static async Task<IResult> CreateCommentAsync(
        Guid id,
        HttpContext httpContext,
        [FromBody] CreateCommentDto body,
        ServiceDeskDbContext db,
        CancellationToken ct)
    {
        var errors = ValidateCreateComment(body);
        if (ValidationErrors.HasErrors(errors))
        {
            return Results.ValidationProblem(errors);
        }

        var ticket = await db.Tickets.SingleOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null)
        {
            return Results.NotFound();
        }

        var currentUser = await TryResolveCurrentUserAsync(httpContext, db, ct);
        if (currentUser is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["user"] = ["No valid user found. Seed an 'admin' user or enable auth and send a valid identity."]
            });
        }

        var now = DateTime.UtcNow;

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Ticket = ticket,
            Author = currentUser,
            AuthorId = currentUser.Id,
            Content = body.Content.Trim(),
            CreatedAt = now
        };

        db.Comments.Add(comment);

        ticket.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        // allowed 201/204 – we return 201 + ID
        return Results.Created($"/api/tickets/{ticket.Id}", new CreateResultDto(comment.Id));
    }

    static TicketSummaryDto MapTicketSummary(Ticket t)
        => new(
            t.Id,
            t.Title,
            TicketStatusMapping.ParseEntityStatus(t.Status),
            DateTimeMapping.ToDateTimeOffset(t.UpdatedAt));

    static TicketCommentDto MapTicketComment(Comment c)
        => new(
            c.Id,
            c.Author.Username,
            c.Content,
            DateTimeMapping.ToDateTimeOffset(c.CreatedAt));

    static DateTime GetTicketUpdatedAt(Ticket t) => t.UpdatedAt;
    static DateTime GetCommentCreatedAt(Comment c) => c.CreatedAt;

    static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    static Dictionary<string, string[]> ValidateCreateTicket(CreateTicketDto dto)
    {
        var errors = ValidationErrors.Create();

        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Trim().Length < 3)
        {
            ValidationErrors.Add(errors, nameof(dto.Title), "Title must be at least 3 characters long.");
        }

        if (dto.Description is not null && dto.Description.Trim().Length is > 0 and < 3)
        {
            ValidationErrors.Add(errors, nameof(dto.Description), "Description must be at least 3 characters long or null/empty.");
        }

        return errors;
    }

    static Dictionary<string, string[]> ValidateCreateComment(CreateCommentDto dto)
    {
        var errors = ValidationErrors.Create();

        if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Trim().Length < 2)
        {
            ValidationErrors.Add(errors, nameof(dto.Content), "Comment must be at least 2 characters long.");
        }

        return errors;
    }

    static async Task<User?> TryResolveCurrentUserAsync(HttpContext httpContext, ServiceDeskDbContext db, CancellationToken ct)
    {
        var username = httpContext.User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(username))
        {
            return await db.Users.SingleOrDefaultAsync(u => u.Username == username, ct);
        }

        // Fallback for Dev: admin if available, otherwise first user
        var admin = await db.Users.SingleOrDefaultAsync(u => u.Username == "admin", ct);
        if (admin is not null)
        {
            return admin;
        }

        return await db.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync(ct);
    }
}
