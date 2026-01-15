using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceDesk.Contracts;

namespace ServiceDesk.Web.Services;

public sealed class ApiKnowledgeClient(HttpClient httpClient) : IKnowledgeClient
{
    static readonly JsonSerializerOptions s_jsonOptions = CreateJsonOptions();

    public async Task<IReadOnlyList<ArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken)
    {
        var items = await httpClient.GetFromJsonAsync<List<ArticleSummaryDto>>(
            "/api/knowledge",
            s_jsonOptions,
            cancellationToken);

        return items ?? [];
    }

    public async Task<ArticleDetailDto?> GetArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/api/knowledge/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ArticleDetailDto>(s_jsonOptions, cancellationToken);
    }

    public async Task<Guid> CreateArticleAsync(CreateArticleDto article, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/knowledge", article, s_jsonOptions, cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw await CreateValidationExceptionAsync(response, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var created = await response.Content.ReadFromJsonAsync<CreateResultDto>(s_jsonOptions, cancellationToken);
        if (created is null)
            throw new InvalidOperationException("API returned 201 but no CreateResultDto body was present.");

        return created.Id;
    }

    public async Task UpdateArticleAsync(Guid id, UpdateArticleDto article, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/knowledge/{id}", article, s_jsonOptions, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Article {id} not found.");

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw await CreateValidationExceptionAsync(response, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        using var response = await httpClient.DeleteAsync($"/api/knowledge/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Article {id} not found.");

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
