namespace ServiceDesk.Web.Services;

public interface IHealthClient
{
    Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken);
}
