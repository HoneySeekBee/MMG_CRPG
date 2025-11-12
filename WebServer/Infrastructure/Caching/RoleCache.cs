using Application.Roles;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class RoleCache : IRoleCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly object _gate = new();

        private List<RoleDto> _all = new();
        private Dictionary<int, RoleDto> _byId = new();
        private Dictionary<string, RoleDto> _byKey = new(StringComparer.OrdinalIgnoreCase);

        public RoleCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        public IReadOnlyList<RoleDto> GetAll() => _all;
        public RoleDto? GetById(int id) => _byId.TryGetValue(id, out var v) ? v : null;
        public RoleDto? GetByKey(string key) => _byKey.TryGetValue(key, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            var entities = await db.Roles
                .AsNoTracking()
                .OrderBy(x => x.SortOrder).ThenBy(x => x.RoleId)
                .ToListAsync(ct);
            var list = entities.Select(RoleDto.From).ToList();
            var byId = list.ToDictionary(x => x.RoleId);
            var byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

            lock (_gate)
            {
                _all = list;
                _byId = byId;
                _byKey = byKey;
            }

            Console.WriteLine($"[RoleCache] loaded: {_all.Count}");
        }
    }
}
