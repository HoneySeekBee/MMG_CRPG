using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.StatTypes
{
    public sealed class StatTypeService : IStatTypeService
    {
        private readonly IStatTypeRepository _repo;
        public StatTypeService(IStatTypeRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<StatTypeDto>> ListAsync(CancellationToken ct)
            => (await _repo.ListAsync(ct))
               .OrderBy(x => x.Name)
               .Select(Map).ToList();

        public async Task<StatTypeDto?> GetAsync(short id, CancellationToken ct)
            => (await _repo.GetByIdAsync(id, ct)) is { } e ? Map(e) : null;

        public async Task<StatTypeDto> CreateAsync(CreateStatTypeRequest req, CancellationToken ct)
        {
            if (await _repo.GetByCodeAsync(req.Code, ct) is not null)
                throw new InvalidOperationException($"StatType code '{req.Code}' already exists.");

            var e = new StatType(req.Code, req.Name, req.IsPercent);
            await _repo.AddAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
            return Map(e);
        }

        public async Task<StatTypeDto> UpdateAsync(short id, UpdateStatTypeRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Not found");
            if (req.Code is { } code)
            {
                var dup = await _repo.GetByCodeAsync(code, ct);
                if (dup is not null && dup.Id != id)
                    throw new InvalidOperationException($"Code '{code}' already exists.");
                e.ChangeCode(code);
            }
            if (req.Name is { } name) e.Rename(name);
            if (req.IsPercent is { } p) e.SetPercent(p);

            await _repo.SaveChangesAsync(ct);
            return Map(e);
        }

        public async Task DeleteAsync(short id, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Not found");
            await _repo.RemoveAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }

        private static StatTypeDto Map(StatType e)
            => new(e.Id, e.Code, e.Name, e.IsPercent);
    }
}
