namespace AdminTool.Models
{
    public class Icon
    {
        public int IconId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Atlas { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? W { get; set; }
        public int? H { get; set; }
        public int Version { get; set; }
    }
}
