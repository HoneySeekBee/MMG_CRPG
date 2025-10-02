namespace WebServer.Options
{
    public sealed class JwtOptions
    {
        public string Key { get; init; } = default!;
        public string? Issuer { get; init; }
        public string? Audience { get; init; }
    }
}
