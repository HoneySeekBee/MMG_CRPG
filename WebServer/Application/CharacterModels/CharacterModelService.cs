using Application.Repositories;
using Domain.Entities.Characters;
using Domain.Enum.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CharacterModels
{
    public sealed class CharacterModelService : ICharacterModelService
    {
        private readonly ICharacterModelRepository _repo;

        public CharacterModelService(ICharacterModelRepository repo)
        {
            _repo = repo;
        }

        public async Task<CharacterModelDto?> GetByCharacterIdAsync(int characterId, CancellationToken ct)
        {
            var model = await _repo.GetByCharacterIdAsync(characterId, ct);
            return model == null ? null : ToDto(model);
        }

        public async Task<List<CharacterModelDto>> GetAllAsync(CancellationToken ct)
        {
            var models = await _repo.GetAllAsync(ct);
            return models.Select(ToDto).ToList();
        }
        public async Task<int> CreateAsync(CreateCharacterModelRequest req, CancellationToken ct)
        {
            var body = Enum.Parse<BodySize>(req.BodyType);
            var anim = Enum.Parse<CharacterAnimationType>(req.AnimationType);

            // 캐릭터ID는 DB에서 자동생성 (Sequence)
            var entity = CharacterModel.Create(0, body, anim);

            // 무기/파츠 장착
            entity.EquipWeapons(req.WeaponLId, req.WeaponRId);
            entity.SetPart(PartType.Head, req.PartHeadId);
            entity.SetPart(PartType.Hair, req.PartHairId);
            entity.SetPart(PartType.Mouth, req.PartMouthId);
            entity.SetPart(PartType.Eye, req.PartEyeId);
            entity.SetPart(PartType.Acc, req.PartAccId);

            var created = await _repo.AddAsync(entity, ct);
            return created.CharacterId;
        }

        public async Task UpdateAsync(int characterId, CreateCharacterModelRequest req, CancellationToken ct)
        {
            var entity = await _repo.GetByCharacterIdAsync(characterId, ct)
                ?? throw new Exception("Character model not found");

            entity.Update(
                Enum.Parse<BodySize>(req.BodyType),
                Enum.Parse<CharacterAnimationType>(req.AnimationType),
                req.WeaponLId,
                req.WeaponRId,
                req.PartHeadId,
                req.PartHairId,
                req.PartMouthId,
                req.PartEyeId,
                req.PartAccId
            );

            await _repo.UpdateAsync(entity, ct);
        }

        public async Task DeleteAsync(int characterId, CancellationToken ct)
        {
            await _repo.DeleteAsync(characterId, ct);
        }

        private static CharacterModelDto ToDto(CharacterModel entity) => new()
        {
            CharacterId = entity.CharacterId,
            BodyType = entity.BodyType.ToString(),
            AnimationType = entity.AnimationType.ToString(),
            WeaponLId = entity.WeaponLId,
            WeaponRId = entity.WeaponRId,
            PartHeadId = entity.PartHeadId,
            PartHairId = entity.PartHairId,
            PartMouthId = entity.PartMouthId,
            PartEyeId = entity.PartEyeId,
            PartAccId = entity.PartAccId,
            HairColorCode = entity.HairColorCode,
            SkinColorCode = entity.SkinColorCode,
        };
    }
} 