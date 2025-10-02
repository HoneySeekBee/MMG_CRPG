namespace WebServer.Options
{
    public sealed class AssetsOptions
    {
        public string ImageUrl { get; init; } = default!;     // ex) https://localhost:7110 or CDN
        public string IconsSubdir { get; init; } = "icons";
        public string PortraitsSubdir { get; init; } = "portraits";
    }

}
