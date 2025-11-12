using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.EquipSlots
{
    public class EquipSlotsService : IEquipSlotsService
    {
        private readonly IEquipSlotsRepository _repo;
        public EquipSlotsService(IEquipSlotsRepository repo) => _repo = repo;

        public Task<IReadOnlyList<EquipSlotDto>> GetAllAsync(CancellationToken ct)
            => _repo.GetAllAsync(ct);
    }
}
