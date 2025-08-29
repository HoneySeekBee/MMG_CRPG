using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Elements
{
    public class ElementService : IElementService
    {
        private readonly IElementRepository _repo;
        private static ElementDto ToDto(Element e) =>
            new(e.ElementId, e.Key, e.Label, e.IconId, e.ColorHex, e.SortOrder, e.IsActive, e.Meta, e.CreatedAt, e.UpdatedAt);

        public ElementService(IElementRepository repo) => _repo = repo;

        public async Task<int> CreateAsync(CreateElementRequest req, CancellationToken ct)
        {
            if (await _repo.KeyExistsAsync(req.Key, ct)) throw new InvalidOperationException("Key already exists.");
            var e = new Element(req.Key, req.Label, req.ColorHex, req.SortOrder, req.IconId, req.MetaJson);
            await _repo.AddAsync(e, ct);
            // Repository가 DbContext.SaveChanges까지 맡는 구조라면 여기서 끝.
            return e.ElementId;
        }

        public async Task UpdateAsync(int id, UpdateElementRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Element not found.");
            e.Update(req.Label, req.ColorHex, req.SortOrder, req.IconId, req.MetaJson);
            // 변경 추적 → SaveChanges는 Repository 쪽에서 처리
        }

        public async Task SetActiveAsync(int id, bool isActive, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Element not found.");
            if (isActive) e.Activate(); else e.Deactivate();
        }

        public async Task<ElementDto> GetByIdAsync(int id, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Element not found.");
            return ToDto(e);
        }

        public async Task<IReadOnlyList<ElementDto>> ListAsync(bool? isActive, string? search, int page, int pageSize, CancellationToken ct)
        {
            var skip = Math.Max(0, (page - 1) * pageSize);
            var list = await _repo.ListAsync(isActive, search, skip, pageSize, ct);
            return list.Select(ToDto).ToList();
        }
    }
}
