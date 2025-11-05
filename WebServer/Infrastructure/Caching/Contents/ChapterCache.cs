using Application.Contents.Chapters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching.Contents
{
    public sealed class ChapterCache : IChapterCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly object _gate = new();

        private List<ChapterDto> _all = new();
        private Dictionary<int, ChapterDto> _byId = new();

        public ChapterCache(IDbContextFactory<GameDBContext> factory)
        {
            _factory = factory;
        }

        public IReadOnlyList<ChapterDto> GetAll() => _all;

        public ChapterDto? GetById(int id)
            => _byId.TryGetValue(id, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            var list = await db.Chapters
                .AsNoTracking()
                .OrderBy(c => c.BattleId)
                .ThenBy(c => c.ChapterNum)
                .Select(c => new ChapterDto
                {
                    ChapterId = c.ChapterId,
                    BattleId = c.BattleId,
                    ChapterNum = c.ChapterNum,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(ct);

            var byId = list.ToDictionary(x => x.ChapterId);

            lock (_gate)
            {
                _all = list;
                _byId = byId;
            }

            Console.WriteLine($"[ChapterCache] loaded: {_all.Count}");
        }
    }
}
