using Application.EquipSlots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IEquipSlotsRepository
    {
        Task<IReadOnlyList<EquipSlotDto>> GetAllAsync(CancellationToken ct);
    }
}
