using Application.Repositories;
using Application.Storage;
using Domain.Entities;

namespace Application.Portraits
{
    public class PortraitService
    {
        private readonly IPortraitStorage _storage;
        private readonly IPortraitRepository _repo;

        public PortraitService(IPortraitStorage storage, IPortraitRepository repo)
        {
            _storage = storage;
            _repo = repo;
        }

        public async Task<List<PortraitDto>> GetAllAsync(CancellationToken ct)
            => (await _repo.GetAllAsync(ct))
                .Select(x => new PortraitDto
                {
                    PortraitId = x.PortraitId,
                    Key = x.Key,
                    Version = x.Version,
                    Url = _storage.GetPublicUrl(x.Key, x.Version)
                })
                .ToList();

        public async Task<PortraitDto> GetByIdAsync(int id, CancellationToken ct)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            return x == null ? null : new PortraitDto
            {
                PortraitId = x.PortraitId,
                Key = x.Key,
                Version = x.Version,
                Url = _storage.GetPublicUrl(x.Key, x.Version)
            };
        }

        public async Task<PortraitDto> CreateAsync(CreatePortraitCommand cmd, CancellationToken ct)
        {
            var entity = new Portrait { Key = cmd.Key };
            await _repo.AddAsync(entity, ct);

            return new PortraitDto
            {
                PortraitId = entity.PortraitId,
                Key = entity.Key,
                Version = entity.Version,
                Url = _storage.GetPublicUrl(entity.Key, entity.Version)
            };
        }

        public async Task<PortraitDto?> UpdateAsync(UpdatePortraitCommand cmd, CancellationToken ct)
        {
            var p = await _repo.GetByIdAsync(cmd.Id, ct);
            if (p is null) return null;

            if (cmd.Key is not null) p.Key = cmd.Key;
            if (cmd.Atlas is not null) p.Atlas = cmd.Atlas;
            if (cmd.X.HasValue) p.X = cmd.X.Value;
            if (cmd.Y.HasValue) p.Y = cmd.Y.Value;
            if (cmd.W.HasValue) p.W = cmd.W.Value;
            if (cmd.H.HasValue) p.H = cmd.H.Value;
            if (cmd.Version.HasValue) p.Version = cmd.Version.Value;

            await _repo.UpdateAsync(p, ct);

            return new PortraitDto
            {
                PortraitId = p.PortraitId,
                Key = p.Key,
                Version = p.Version,
                Url = _storage.GetPublicUrl(p.Key, p.Version)
            };
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var p = await _repo.GetByIdAsync(id, ct);
            if (p is null) return false;

            await _repo.DeleteAsync(p, ct);
            return true;
        }

        public async Task UploadAsync(UploadPortraitCommand cmd, CancellationToken ct)
        {
            var p = await _repo.GetByKeyAsync(cmd.Key, ct);
            if (p is null)
            {
                p = new Portrait { Key = cmd.Key, Version = 0 };
                await _repo.AddAsync(p, ct);
            }

            await _storage.SaveAsync(cmd.Key, cmd.Content, cmd.ContentType, ct);

            p.Version++;
            await _repo.UpdateAsync(p, ct);
        }
    }
}
