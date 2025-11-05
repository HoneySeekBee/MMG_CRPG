using Application.Repositories.Contents;
using Domain.Entities.Contents;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly GameDBContext _context;

        public ChapterRepository(GameDBContext context)
        {
            _context = context;
        }

        public async Task<Chapter?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Chapters
                .FirstOrDefaultAsync(c => c.ChapterId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Chapter>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Chapters
                .AsNoTracking()
                .OrderBy(c => c.BattleId)
                .ThenBy(c => c.ChapterNum)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default)
        {
            await _context.Chapters.AddAsync(chapter, cancellationToken);
            // 여기서 SaveChangesAsync를 할지 말지는 네가 전체 아키텍처에서 정한 곳에서 해
        }

        public Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken = default)
        {
            _context.Chapters.Update(chapter);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Chapter chapter, CancellationToken cancellationToken = default)
        {
            _context.Chapters.Remove(chapter);
            return Task.CompletedTask;
        }
    }
}
