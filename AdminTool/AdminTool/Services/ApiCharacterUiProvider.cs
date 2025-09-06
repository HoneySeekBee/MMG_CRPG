using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminTool.Services
{
    public sealed class ApiCharacterUiProvider : ICharacterUiProvider
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _cfg;
        public ApiCharacterUiProvider(IHttpClientFactory factory, IConfiguration cfg)
        {
            _factory = factory; _cfg = cfg;
        }

        public async Task<IEnumerable<SelectListItem>> GetOptionsAsync(CancellationToken ct)
        {
            var http = _factory.CreateClient("GameApi");

            // 캐릭터 요약 가져오기 (페이지 크게)
            var paged = await http.GetFromJsonAsync<Application.Character.PagedResult<Application.Character.CharacterSummaryDto>>(
                "/api/characters?page=1&pageSize=1000", ct)
                ?? new Application.Character.PagedResult<Application.Character.CharacterSummaryDto>(Array.Empty<Application.Character.CharacterSummaryDto>(), 0, 1, 1000);

            // 소속 라벨용 (있으면 라벨, 없으면 ID로 대체)
            var factions = await http.GetFromJsonAsync<List<Application.Factions.FactionDto>>("/api/factions", ct) ?? new();
            var fdict = factions.ToDictionary(f => f.FactionId, f => f.Label);

            // "123 | 메가나이트 | 왕국" 형식 라벨
            return paged.Items
                .OrderBy(c => c.Id)
                .Select(c =>
                {
                    fdict.TryGetValue(c.FactionId, out var fname);
                    var label = $"{c.Id} | {c.Name} | {(fname ?? $"Faction#{c.FactionId}")}";
                    return new SelectListItem(label, c.Id.ToString());
                })
                .ToList();
        }
    }
}
