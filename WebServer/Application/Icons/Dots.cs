using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Icons
{
    public sealed class CreateIconCommand
    {
        public string Key { get; init; } = default!;
    }
    public sealed class UpdateIconCommand
    {
        public int Id { get; init; }
        public string Key { get; init; } = default!;
        public string? Atlas { get; init; }
        public int? X { get; init; }
        public int? Y { get; init; }
        public int? W { get; init; }
        public int? H { get; init; }
        public int Version { get; init; }
    }

    public sealed class UploadIconCommand
    {
        public string Key { get; init; } = default!;
        public Stream Content { get; init; } = default!;
        public string ContentType { get; init; } = "image/png";
    }
    public sealed class IconDto   // API 응답용 Query DTO
    {
        public int IconId { get; init; }
        public string Key { get; init; } = default!;
        public int Version { get; init; }
        public string Url { get; init; } = default!;
    }
}
