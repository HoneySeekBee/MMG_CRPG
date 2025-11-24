using Application.Common.Interface;

namespace AdminTool.Models
{
    public class StreamListVm
    {
        public string StreamName { get; set; } = "";
        public List<StreamEntryDto> Entries { get; set; } = new();
    }
}
