namespace AdminTool.Services
{
    public interface IAdminAssetUrlBuilder
    {
        string Icon(string key, int version);
        string Portrait(string key, int version);
    }

    public sealed class AdminAssetUrlBuilder : IAdminAssetUrlBuilder
    {
        private const string CdnBase = "https://d3nehzpoo6py80.cloudfront.net";

        public string Icon(string key, int version)
            => $"{CdnBase}/icons/{Uri.EscapeDataString(key)}.png?v={version}";

        public string Portrait(string key, int version)
            => $"{CdnBase}/portraits/{Uri.EscapeDataString(key)}.png?v={version}";
    }
}
