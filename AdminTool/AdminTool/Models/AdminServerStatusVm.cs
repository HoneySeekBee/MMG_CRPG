using Domain.Common;
using static AdminTool.Controllers.AdminServerStatusController;

namespace AdminTool.Models
{
    public sealed class AdminServerStatusVm
    {
        public List<ServerStatusInfoDto> Servers { get; set; } = new();
    }
    public sealed class AdminServerInfoVm
    {
        public required string ServerId { get; init; }
        public bool Alive { get; init; }
        public long LastUpdated { get; init; }

        // 세부 상태
        public required string Version { get; init; } 
        public required int OnlineUsers { get; init; } 
    }
    public sealed class AdminServerDetailVm
    {
        public required string ServerId { get; init; }
        public bool Alive { get; init; }
        public long LastUpdated { get; init; }

        public required string Version { get; init; } 
        public required int OnlineUsers { get; init; } 
        public required int RequestCount { get; init; }
    }
}
