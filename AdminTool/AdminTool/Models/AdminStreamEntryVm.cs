namespace AdminTool.Models
{
    public sealed class AdminStreamEntryVm
    {
        public required string Id { get; init; }
        public required Dictionary<string, string> Fields { get; init; }
        public required string TimestampLocal { get; init; }
    }
}
