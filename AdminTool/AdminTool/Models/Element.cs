namespace AdminTool.Models
{
    public class Element
    {
        public int ElementId { get; private set; }
        public string Key { get; private set; } = default!;
        public string Label { get; private set; } = default!;
        public int? IconId { get; private set; }
        public string ColorHex { get; private set; } = "#FFFFFF";
        public short SortOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public string Meta { get; private set; } = "{}"; // jsonb 대응
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    }
}
