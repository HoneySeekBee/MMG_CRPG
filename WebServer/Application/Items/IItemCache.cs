using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Items
{
    public interface IItemCache
    {
        IReadOnlyList<ItemDto> GetAll();
        ItemDto? GetById(long id);
        ItemDto? GetByCode(string code);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
