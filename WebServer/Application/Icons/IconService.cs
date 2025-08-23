using Application.Repositories;
using Application.Storage;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Application.Icons
{
    public class IconService
    {
        private readonly IIconStorage _storage;
        private readonly IIconRepository _repo; // DB 저장소 인터페이스 

        public IconService(IIconStorage storage, IIconRepository repo)
        {
            _storage = storage;
            _repo = repo;
        }

        public async Task<List<IconDto>> GetAllAsync(CancellationToken ct)
        => (await _repo.GetAllAsync(ct))
            .Select(x => new IconDto
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = _storage.GetPublicUrl(x.Key, x.Version)
            }).ToList();

        public async Task<IconDto> GetByIdAsync(int id, CancellationToken ct)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            return x == null ? null :
                new IconDto
                {
                    IconId = x.IconId,
                    Key = x.Key,
                    Version = x.Version,
                    Url = _storage.GetPublicUrl(x.Key, x.Version)
                };
        }

        public async Task<IconDto> CreateAsync(CreateIconCommand cmd, CancellationToken ct)
        {
            var entity = new Icon { Key = cmd.Key };
            await _repo.AddAsync(entity, ct);
            return new IconDto
            {
                IconId = entity.IconId,
                Key = entity.Key,
                Version = entity.Version,
                Url = _storage.GetPublicUrl(entity.Key, entity.Version)
            };
        }

        public async Task<IconDto?> UpdateAsync(UpdateIconCommand cmd, CancellationToken ct)
        {
            var icon = await _repo.GetByIdAsync(cmd.Id, ct);

            if (icon is null) return null;

            icon.Key = cmd.Key;
            icon.Atlas = cmd.Atlas;
            icon.X = cmd.X;
            icon.Y = cmd.Y;
            icon.W = cmd.W;
            icon.H = cmd.H;
            icon.Version = cmd.Version;

            await _repo.UpdateAsync(icon, ct);
            return new IconDto
            {
                IconId = icon.IconId,
                Key = icon.Key,
                Version = icon.Version,
                Url = _storage.GetPublicUrl(icon.Key, icon.Version)
            };
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var icon = await _repo.GetByIdAsync(id, ct);
            if (icon is null) return false;

            await _repo.DeleteAsync(icon, ct);
            return true;
        }

        public async Task UploadAsync(UploadIconCommand cmd, CancellationToken ct)
        {
            var icon = await _repo.GetByKeyAsync(cmd.Key, ct);
            if (icon is null)
            {
                icon = new Icon { Key = cmd.Key, Version = 0 };
                await _repo.AddAsync(icon, ct);
            }
            await _storage.SaveAsync(cmd.Key, cmd.Content, cmd.ContentType, ct);
            icon.Version++;
            await _repo.UpdateAsync(icon, ct);
        }
    }
}
