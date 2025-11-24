using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interface
{
    public interface IServerStatusTracker
    {
        Task UpdateHeartbeatAsync(string serverId, ServerStatus status, CancellationToken ct = default);

        Task<List<ServerStatusInfo>> GetAllServersAsync(CancellationToken ct = default);

        Task<ServerStatusInfo?> GetServerStatusAsync(string serverId, CancellationToken ct = default);

        Task<List<string>> GetServerIdsAsync(CancellationToken ct = default);
    }
}
