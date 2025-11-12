using Application.EquipSlots;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class EquipSlotsRepository : IEquipSlotsRepository
    {
        private readonly GameDBContext _db;
        public EquipSlotsRepository(GameDBContext db) => _db = db;

        public async Task<IReadOnlyList<EquipSlotDto>> GetAllAsync(CancellationToken ct)
        {
            return await _db.Set<EquipSlot>()
                .AsNoTracking()
                .OrderBy(e => e.SortOrder).ThenBy(e => e.Name)
                .Select(e => new EquipSlotDto(e.Id, e.Code, e.Name, e.SortOrder, e.IconId, e.UpdatedAt))
                .ToListAsync(ct);
        }
    }
}
