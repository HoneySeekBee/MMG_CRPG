using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Chapters
{
    public interface IChapterCache
    {
        IReadOnlyList<ChapterDto> GetAll();
        ChapterDto? GetById(int id);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
