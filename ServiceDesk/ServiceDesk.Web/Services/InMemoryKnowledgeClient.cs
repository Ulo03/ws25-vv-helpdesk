using ServiceDesk.Web.Models;

namespace ServiceDesk.Web.Services;

// TODO: Patrick: Added for Testing and Designing, change later to API call!!

public sealed class InMemoryKnowledgeClient : IKnowledgeClient
{
    static readonly IReadOnlyList<KnowledgeArticleSummaryDto> Articles =
    [
        new(1, "How to reset your password",
            "Open the login page and click “Forgot password”…",
            DateTimeOffset.Now.AddDays(-2),
            ["account", "login"]),

        new(2, "VPN troubleshooting",
            "Check your internet connection, then verify server address…",
            DateTimeOffset.Now.AddDays(-7),
            ["network", "vpn"]),
    ];

    public Task<IReadOnlyList<KnowledgeArticleSummaryDto>> GetArticlesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Articles);
    }
}
