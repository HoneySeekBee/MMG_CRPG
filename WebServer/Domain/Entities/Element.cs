namespace Domain.Entities
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
        private Element() { } // EF Core


        public Element(string key, string label, string colorHex, short sortOrder, int? iconId, string metaJson)
        {
            SetKey(key);
            SetLabel(label);
            SetColor(colorHex);
            SetSortOrder(sortOrder);
            IconId = iconId;
            SetMeta(metaJson);
        }

        public void Update(string label, string colorHex, short sortOrder, int? iconId, string metaJson)
        {
            SetLabel(label);
            SetColor(colorHex);
            SetSortOrder(sortOrder);
            IconId = iconId;
            SetMeta(metaJson);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
        public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

        void SetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !System.Text.RegularExpressions.Regex.IsMatch(key, "^[a-z0-9_][a-z0-9_-]*$"))
                throw new ArgumentException("Invalid element key format.", nameof(key));
            Key = key;
        }
        void SetLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Label required.", nameof(label));
            Label = label;
        }
        void SetColor(string hex)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(hex, "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$"))
                throw new ArgumentException("ColorHex must be #RRGGBB or #RRGGBBAA.", nameof(hex));
            ColorHex = hex;
        }
        void SetSortOrder(short order) => SortOrder = order;
        void SetMeta(string json) => Meta = string.IsNullOrWhiteSpace(json) ? "{}" : json;
    }
}
