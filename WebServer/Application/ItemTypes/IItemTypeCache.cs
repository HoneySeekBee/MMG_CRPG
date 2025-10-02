using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ItemTypes
{
    public interface IItemTypeCache
    {
        IReadOnlyList<ItemTypeDto> GetAll();
        ItemTypeDto? GetById(short id);
        Task ReloadAsync(CancellationToken ct = default); 
    }
}
