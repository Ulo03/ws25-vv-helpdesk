namespace ServiceDesk.ApiService.Mapping;

internal static class PreviewMapping
{
    internal static string CreatePreview(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var trimmed = content.Trim();
        const int max = 140;

        return trimmed.Length <= max ? trimmed : trimmed[..max] + "…";
    }
}
