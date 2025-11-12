using Application.Repositories.Contents;
using Domain.Entities.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;

        public ChapterService(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        public async Task<ChapterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _chapterRepository.GetByIdAsync(id, cancellationToken);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<IReadOnlyList<ChapterDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var list = await _chapterRepository.GetListAsync(cancellationToken);
            return list.Select(MapToDto).ToList();
        }

        public async Task<int> CreateAsync(CreateChapterRequest request, CancellationToken cancellationToken = default)
        {
            var entity = new Chapter(
                battleId: request.BattleId,
                chapterNum: request.ChapterNum,
                name: request.Name,
                description: request.Description,
                isActive: request.IsActive
            );

            await _chapterRepository.AddAsync(entity, cancellationToken);
            return entity.ChapterId;
        }

        public async Task<bool> UpdateAsync(UpdateChapterRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _chapterRepository.GetByIdAsync(request.ChapterId, cancellationToken);
            if (entity is null)
                return false;

            entity.Update(
                request.BattleId,
                request.ChapterNum,
                request.Name,
                request.Description,
                request.IsActive
            );

            await _chapterRepository.UpdateAsync(entity, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _chapterRepository.GetByIdAsync(id, cancellationToken);
            if (entity is null)
                return false;

            await _chapterRepository.DeleteAsync(entity, cancellationToken);
            return true;
        }

        private static ChapterDto MapToDto(Chapter c) => new()
        {
            ChapterId = c.ChapterId,
            BattleId = c.BattleId,
            ChapterNum = c.ChapterNum,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
} 