using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : CombatActorView
{
    public override void PlayHitFx(bool isCrit)
    {
        base.PlayHitFx(isCrit);
        // 플레이어 캐릭터만의 피격 연출
    }

    protected override void OnDie()
    {
        base.OnDie();
        // 파티원 사망 UI, 부활 가능 등
    }
}
