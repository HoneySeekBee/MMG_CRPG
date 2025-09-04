using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminTool.Controllers
{
    [ApiController]
    [Route("api")]
    public sealed class LookupsController : ControllerBase
    {
        private readonly GameDBContext _db;
        public LookupsController(GameDBContext db) => _db = db;

        [HttpGet("elements")]
        public async Task<IEnumerable<object>> GetElements(CancellationToken ct) =>
            await _db.Elements.AsNoTracking()
                .Where(e => e.IsActive)                         // 필요시
                .OrderBy(e => e.SortOrder).ThenBy(e => e.ElementId)
                .Select(e => new { e.ElementId, e.Label, e.SortOrder })
                .ToListAsync(ct);

        [HttpGet("rarities")]
        public async Task<IEnumerable<object>> GetRarities(CancellationToken ct) =>
            await _db.Rarities.AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.SortOrder).ThenBy(r => r.RarityId)
                .Select(r => new { r.RarityId, r.Stars, r.Label, r.SortOrder })
                .ToListAsync(ct);

        [HttpGet("roles")]
        public async Task<IEnumerable<object>> GetRoles(CancellationToken ct) =>
            await _db.Roles.AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.SortOrder).ThenBy(r => r.RoleId)
                .Select(r => new { r.RoleId, r.Label, r.SortOrder })
                .ToListAsync(ct);

        [HttpGet("factions")]
        public async Task<IEnumerable<object>> GetFactions(CancellationToken ct) =>
            await _db.Factions.AsNoTracking()
                .Where(f => f.IsActive)
                .OrderBy(f => f.SortOrder).ThenBy(f => f.FactionId)
                .Select(f => new { f.FactionId, f.Label, f.SortOrder })
                .ToListAsync(ct);
    }
}
