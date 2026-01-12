
namespace ServiceDesk.Web.Services;

public class ApiHealthClient(HttpClient httpClient) : IHealthClient
{
    public async Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken)
    {
        using var respone = await httpClient.GetAsync("/health", cancellationToken);

        return respone.IsSuccessStatusCode;
    }
}
