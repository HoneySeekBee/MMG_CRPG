using AdminTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminTool.Controllers
{
    public class IconsController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public IconsController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        // 운영툴에서 Icons 이미지를 수정 및 관리한다. 

        // [1] Index
        // (1) 만들어진 Icon들 가지고 오기 
        // Get
        public async Task<IActionResult> Index()
        {
            var client = _httpFactory.CreateClient("GameApi");


            var icons = await client.GetFromJsonAsync<List<Icon>>("api/icons");

            return View(icons); 
        }


        // [2] Create
        // (2) 

        // [3] Edit
    }
}
