using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminTool.Services
{
    public interface ICharacterUiProvider
    {
        Task<IEnumerable<SelectListItem>> GetOptionsAsync(CancellationToken ct);
    }
}
