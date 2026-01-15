using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceDesk.Contracts;

namespace ServiceDesk.Web.Services;

public sealed class ApiTicketClient(HttpClient httpClient) : ITicketClient
{
    static readonly JsonSerializerOptions s_jsonOptions = CreateJsonOptions();

    public async Task<IReadOnlyList<TicketSummaryDto>> GetTicketsAsync(CancellationToken cancellationToken)
    {
        var items = await httpClient.GetFromJsonAsync<List<TicketSummaryDto>>(
            "/api/tickets",
            s_jsonOptions,
            cancellationToken);

        return items ?? [];
    }

    public async Task<TicketDetailDto?> GetTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/api/tickets/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<TicketDetailDto>(s_jsonOptions, cancellationToken);
    }

    public async Task<Guid> CreateTicketAsync(CreateTicketDto ticket, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/tickets", ticket, s_jsonOptions, cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw await CreateValidationExceptionAsync(response, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var created = await response.Content.ReadFromJsonAsync<CreateResultDto>(s_jsonOptions, cancellationToken);
        if (created is null)
            throw new InvalidOperationException("API returned 201 but no CreateResultDto body was present.");

        return created.Id;
    }

    public async Task UpdateStatusAsync(Guid ticketId, UpdateTicketStatusDto status, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/tickets/{ticketId}/status")
        {
            Content = JsonContent.Create(status, options: s_jsonOptions)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Ticket {ticketId} not found.");

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw await CreateValidationExceptionAsync(response, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task AddCommentAsync(Guid ticketId, CreateCommentDto comment, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"/api/tickets/{ticketId}/comments",
            comment,
            s_jsonOptions,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Ticket {ticketId} not found.");

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw await CreateValidationExceptionAsync(response, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"API request failed with {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}",
            inner: null,
            statusCode: response.StatusCode);
    }

    static async Task<Exception> CreateValidationExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ArgumentException($"Validation failed (400). Body: {body}");
    }

    static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
