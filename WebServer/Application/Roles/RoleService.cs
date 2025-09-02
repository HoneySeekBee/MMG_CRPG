using Application.Repositories;
using Application.Validation;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Roles
{
    public sealed class RoleService : IRoleService
    {
        private readonly IRoleRepository _repo;
        public RoleService(IRoleRepository repo) => _repo = repo;

        public async Task<RoleDto?> GetAsync(int id, CancellationToken ct)
            => (await _repo.GetByIdAsync(id, ct)) is { } e ? RoleDto.From(e) : null;

        public async Task<IReadOnlyList<RoleDto>> ListAsync(bool? isActive, int page, int pageSize, CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 50;
            var list = await _repo.ListAsync(isActive, (page - 1) * pageSize, pageSize, ct);
            return list.Select(RoleDto.From).ToList();
        }

        public async Task<RoleDto> CreateAsync(CreateRoleRequest req, CancellationToken ct)
        {
            Guard.NotEmpty(req.Key, nameof(req.Key));
            Guard.NotEmpty(req.Label, nameof(req.Label));
            Guard.Color(req.ColorHex, nameof(req.ColorHex));

            if (await _repo.GetByKeyAsync(req.Key, ct) is not null)
                throw new InvalidOperationException("이미 존재하는 Key 입니다.");

            var e = new Role
            {
                Key = req.Key.Trim(),
                Label = req.Label.Trim(),
                IconId = req.IconId,
                ColorHex = req.ColorHex,
                SortOrder = req.SortOrder,
                IsActive = req.IsActive,
                Meta = req.Meta
            };

            await _repo.AddAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
            return RoleDto.From(e);
        }

        public async Task UpdateAsync(int id, UpdateRoleRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            Guard.NotEmpty(req.Label, nameof(req.Label));
            Guard.Color(req.ColorHex, nameof(req.ColorHex));

            e.Label = req.Label.Trim();
            e.IconId = req.IconId;
            e.ColorHex = req.ColorHex;
            e.SortOrder = req.SortOrder;
            e.IsActive = req.IsActive;
            e.Meta = req.Meta;

            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");
            await _repo.RemoveAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }

}
