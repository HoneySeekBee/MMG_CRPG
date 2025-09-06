using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        long TotalCount
    )
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(PageSize, 1));
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
