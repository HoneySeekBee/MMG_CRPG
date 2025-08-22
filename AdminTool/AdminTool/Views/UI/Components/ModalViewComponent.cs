using AdminTool.Models.UI.Components.Modal;
using Microsoft.AspNetCore.Mvc;

namespace AdminTool.Views.UI.Components
{
    public class ModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ModalVm vm) => View(vm);
    }
}
