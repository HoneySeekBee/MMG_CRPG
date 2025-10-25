using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.EquipSlots
{
    public interface IEquipSlotsService
    {
        Task<IReadOnlyList<EquipSlotDto>> GetAllAsync(CancellationToken ct);
    }
}
