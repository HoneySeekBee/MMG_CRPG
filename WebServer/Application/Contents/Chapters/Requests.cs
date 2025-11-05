using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Chapters
{
    public class CreateChapterRequest
    {
        public int BattleId { get; set; }
        public int ChapterNum { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
    public class UpdateChapterRequest
    {
        public int ChapterId { get; set; }
        public int BattleId { get; set; }
        public int ChapterNum { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
