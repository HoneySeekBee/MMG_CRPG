using AdminTool.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminTool.Controllers
{
    public sealed class StaticStageUiProvider : IStageUiProvider
    {
        public Task<IEnumerable<SelectListItem>> GetOptionsAsync(CancellationToken ct)
        {
            // TODO: 필요하면 실제 스테이지 ID/이름으로 교체
            var items = new List<SelectListItem>
            {
                new("Stage 100", "100"),
                new("Stage 101", "101"),
                new("Stage 102", "102")
            };
            return Task.FromResult<IEnumerable<SelectListItem>>(items);
        }
    }
}
