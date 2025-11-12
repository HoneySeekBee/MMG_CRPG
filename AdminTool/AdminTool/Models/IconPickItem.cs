namespace AdminTool.Models
{
    public sealed class IconPickItem
    {
        public int IconId { get; set; }
        public string Key { get; set; } = "";
        public int Version { get; set; }
        public string Url { get; set; } = ""; // cdn/base/icons/{key}.png?v={version}
    }
}
