using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CharacterModels
{
    public interface ICharacterModelCache
    {
        Task ReloadAsync(CancellationToken ct = default);
        // 기본 조회
        CharacterModelDto? GetModel(int characterId);

        // 파츠/무기 메타 조회
        CharacterModelPartDto? GetPart(int partId);
        CharacterModelWeaponDto? GetWeapon(int weaponId); 
        // Addressables 키까지 풀어준 “레시피” 조회 (전투/프리뷰용)
        CharacterVisualRecipe? BuildRecipe(int characterId);
        IEnumerable<CharacterModelDto> GetAllModels();
        IEnumerable<CharacterModelPartDto> GetAllParts();
        IEnumerable<CharacterModelWeaponDto> GetAllWeapons();

    }
}
