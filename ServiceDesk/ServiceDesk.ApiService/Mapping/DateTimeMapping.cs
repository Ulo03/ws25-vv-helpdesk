namespace ServiceDesk.ApiService.Mapping;

internal static class DateTimeMapping
{
    internal static DateTimeOffset ToDateTimeOffset(DateTime value)
    {
        // SQL Server datetime via GETDATE() typically returns ‘unspecified’.
        // Here, unspecified is interpreted as UTC because we write UTC in the endpoints.
        if (value.Kind == DateTimeKind.Unspecified)
        {
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        return new DateTimeOffset(value);
    }
}
