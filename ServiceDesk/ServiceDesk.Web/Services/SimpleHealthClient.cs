namespace ServiceDesk.Web.Services;

// TODO: Patrick: Added for Testing and Designing, change later to API call!!

public sealed class SimpleHealthClient : IHealthClient
{
    public Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(true);
    }
}
