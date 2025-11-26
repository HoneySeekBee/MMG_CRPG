using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public record ServerStatus(
    long RequestCount,
    int OnlineUsers,
    string Version
);

    public record ServerStatusInfo(
        string ServerId,
        bool Alive,
        ServerStatus? Status,
        long? LastUpdated
    );
}
