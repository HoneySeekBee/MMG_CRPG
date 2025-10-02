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
        private readonly GameDBContext _db;
        private Dictionary<int, RoleDto> _byId = new();
        private Dictionary<string, RoleDto> _byKey = new();
        private List<RoleDto> _all = new();

        public RoleCache(GameDBContext db)
        {
            _db = db;
        }


        public IReadOnlyList<RoleDto> GetAll() => _all;
        public RoleDto? GetById(int id) => _byId.TryGetValue(id, out var dto) ? dto : null;
        public RoleDto? GetByKey(string key) => _byKey.TryGetValue(key, out var dto) ? dto : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            var entities = await _db.Roles.AsNoTracking().ToListAsync(ct);

            var list = entities.Select(RoleDto.From).ToList();

            _all = list;
            _byId = list.ToDictionary(x => x.RoleId);
            _byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        }
    }
}
