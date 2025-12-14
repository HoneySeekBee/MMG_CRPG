namespace AdminTool.Services
{
    public interface IAdminAssetUrlBuilder
    {
        string Icon(string key, int version);
        string Portrait(string key, int version);
    }

    public sealed class AdminAssetUrlBuilder : IAdminAssetUrlBuilder
    {
        public string Icon(string key, int version)
            => $"/admin/assets/icons/{Uri.EscapeDataString(key)}?v={version}";

        public string Portrait(string key, int version)
            => $"/admin/assets/portraits/{Uri.EscapeDataString(key)}?v={version}";
    }
}
