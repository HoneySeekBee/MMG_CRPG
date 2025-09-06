using Application.Combat;
using Microsoft.AspNetCore.Mvc;

namespace AdminTool.Services
{
    public sealed class CombatApiClient : ICombatApiClient
    {
        private readonly IHttpClientFactory _factory;
        public CombatApiClient(IHttpClientFactory factory) => _factory = factory;

        private HttpClient Http => _factory.CreateClient("GameApi"); // 너 이미 등록해둔 named client 사용

        public async Task<SimulateCombatResponse> SimulateAsync(SimulateCombatRequest req, CancellationToken ct)
        {
            var http = _factory.CreateClient("GameApi");
            var res = await http.PostAsJsonAsync("/combat/simulate", req, ct);

            if (res.IsSuccessStatusCode)
                return (await res.Content.ReadFromJsonAsync<SimulateCombatResponse>(cancellationToken: ct))!;

            // 실패 시 문제상세 or 원문 보여주기
            var body = await res.Content.ReadAsStringAsync(ct);
            try
            {
                var vpd = System.Text.Json.JsonSerializer.Deserialize<ValidationProblemDetails>(body);
                if (vpd?.Errors?.Count > 0)
                    throw new InvalidOperationException(string.Join("; ",
                        vpd.Errors.SelectMany(kv => kv.Value.Select(msg => $"{kv.Key}: {msg}"))));
                var pd = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(body);
                throw new InvalidOperationException(pd?.Detail ?? body);
            }
            catch
            {
                throw new InvalidOperationException(body);
            }
        }
        public async Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct)
        {
            var url = $"/combat/{combatId}/log?size={size}" + (string.IsNullOrEmpty(cursor) ? "" : $"&cursor={Uri.EscapeDataString(cursor)}");
            return (await Http.GetFromJsonAsync<CombatLogPageDto>(url, ct))!;
        }

        public Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct)
            => Http.GetFromJsonAsync<CombatLogSummaryDto>($"/combat/{combatId}/summary", ct)!;
    }
}
