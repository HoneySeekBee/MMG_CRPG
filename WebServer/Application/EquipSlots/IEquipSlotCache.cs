using Application.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.EquipSlots
{
    public interface IEquipSlotCache
    {
        IReadOnlyList<EquipSlotDto> GetAll();
        EquipSlotDto? GetById(int id);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
