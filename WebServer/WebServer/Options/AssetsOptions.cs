namespace WebServer.Options
{
    public sealed class AssetsOptions
    {
        public string ImageUrl { get; init; } = default!;

        public string IconsSubdir { get; init; } = "icons";
        public string PortraitsSubdir { get; init; } = "portraits";
    }

}
