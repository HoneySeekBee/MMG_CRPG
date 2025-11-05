using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Chapters
{
    public interface IChapterService
    {
        Task<ChapterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChapterDto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateChapterRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(UpdateChapterRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
