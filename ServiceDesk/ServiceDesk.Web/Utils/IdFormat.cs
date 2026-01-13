namespace ServiceDesk.Web.Utils;

public static class IdFormat
{
    public static string Short(Guid id)
    {
        // "N" = 32 hex chars without hyphen
        var s = id.ToString("N");
        return s.Length >= 8 ? s[..8] : s;
    }
}
