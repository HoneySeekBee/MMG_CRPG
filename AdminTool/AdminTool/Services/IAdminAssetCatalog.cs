using AdminTool.Models;

namespace AdminTool.Services
{
    public interface IAdminAssetCatalog
    {
        Task<IReadOnlyList<IconVm>> GetIconsAsync(CancellationToken ct);
        Task<IReadOnlyList<PortraitVm>> GetPortraitsAsync(CancellationToken ct);
        void InvalidateIcons();
        void InvalidatePortraits();
    }
}
