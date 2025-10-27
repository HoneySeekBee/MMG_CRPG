using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCharacterEquips
{
    public interface ICharacterEquipmentService
    {
        Task<UserCharacterEquipSnapshotDto> GetAsync(int userId, int characterId, CancellationToken ct);
        Task<SetEquipmentResultDto> SetAsync(SetEquipmentCommand cmd, CancellationToken ct);
    }

}
