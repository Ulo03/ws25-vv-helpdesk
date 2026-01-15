namespace ServiceDesk.ApiService.Validation;

internal static class ValidationErrors
{
    internal static Dictionary<string, string[]> Create()
        => new(StringComparer.Ordinal);

    internal static bool HasErrors(Dictionary<string, string[]> errors)
        => errors.Count > 0;

    internal static void Add(Dictionary<string, string[]> errors, string field, string message)
    {
        if (errors.TryGetValue(field, out var existing))
        {
            errors[field] = [.. existing, message];
            return;
        }

        errors[field] = [message];
    }
}
