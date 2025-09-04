using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Portraits
{
    public sealed class CreatePortraitCommand
    {
        public string Key { get; init; } = default!;
    }

    public sealed class UpdatePortraitCommand
    {
        public int Id { get; init; }              // 경로 {id}와 일치
        public string? Key { get; init; }         // 변경 시만 값 제공
        public string? Atlas { get; init; }
        public int? X { get; init; }
        public int? Y { get; init; }
        public int? W { get; init; }
        public int? H { get; init; }
        public int? Version { get; init; }
    }

    public sealed class UploadPortraitCommand
    {
        public string Key { get; init; } = default!;
        public Stream Content { get; init; } = default!;
        public string ContentType { get; init; } = "image/png";
    }

    // API 응답용 DTO
    public sealed class PortraitDto
    {
        public int PortraitId { get; init; }
        public string Key { get; init; } = default!;
        public int Version { get; init; }
        public string Url { get; init; } = default!;
    }
}
