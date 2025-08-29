using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Elements
{
    public sealed record CreateElementRequest(string Key, string Label, int? IconId, string ColorHex, short SortOrder, string MetaJson);
    public sealed record UpdateElementRequest(string Label, int? IconId, string ColorHex, short SortOrder, string MetaJson);
}
