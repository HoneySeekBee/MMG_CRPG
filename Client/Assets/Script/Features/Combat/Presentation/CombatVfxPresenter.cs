using Combat;
using Game.Data;
using PixPlays.ElementalVFX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class CombatVfxPresenter
    {
        private readonly SkillFxDataList _skillFxDb;
        private readonly Dictionary<long, GameObject> _actorObjects;
        private readonly Dictionary<long, int> _actorMasterIds;
        private readonly Transform parent;

        public CombatVfxPresenter(SkillFxDataList skillFxDb, Dictionary<long, GameObject> actorObjects, Dictionary<long, int> actorMasterIds, Transform particleParent)
        {
            _skillFxDb = skillFxDb;
            _actorObjects = actorObjects;
            _actorMasterIds = actorMasterIds;
            parent = particleParent;
        }
        public void HandleEvent(CombatLogEventPb ev)
        {
            switch (ev.Type)
            {
                case CombatEventTypes.SkillCast:
                    OnSkillCast(ev);
                    break;

                case CombatEventTypes.NormalAttack:
                    OnNormalAttack(ev);
                    break;

                case CombatEventTypes.Hit:
                    OnHit(ev);
                    break;

                case CombatEventTypes.SkillHit:
                case CombatEventTypes.SkillHitAoe:
                    OnSkillHit(ev);
                    break;
            }
        }

        private void OnSkillCast(CombatLogEventPb ev)
        {
            if (!long.TryParse(ev.Actor, out var casterActorId)) return;
            if (!_actorObjects.TryGetValue(casterActorId, out var casterGo)) return;
            if (!_actorMasterIds.TryGetValue(casterActorId, out var characterId)) return;

            int breakthrough = GetBreakthrough(characterId);
            var sd = _skillFxDb.GetByCharacterId(characterId);
            if (sd == null) return;

            var fxSet = sd.GetFxSet(breakthrough);
            if (fxSet == null) return;

            // 애니메이션 (시전 시작)
            casterGo.GetComponent<CombatActorView>()?.PlayAttack(false);

            // 사운드
            if (fxSet.castSound != null)
                AudioManager.Instance.PlaySFX(fxSet.castSound);

            // FX
            if (fxSet.skillFx != null)
            {
                Vector3 source = casterGo.transform.position;

                Vector3 target = source + casterGo.transform.forward * 2f;
                if (!string.IsNullOrEmpty(ev.Target) &&
                    long.TryParse(ev.Target, out var targetActorId) &&
                    _actorObjects.TryGetValue(targetActorId, out var targetGo))
                {
                    target = targetGo.transform.position;
                }

                var fx = Object.Instantiate(fxSet.skillFx, parent);
                fx.transform.localScale = Vector3.one * fxSet.fxScale;

                float duration = 2f;
                float radius = 1f;

                var data = new VfxData(source, target, duration, radius);
                data.SetGround(new Vector3(target.x, 0f, target.z));
                fx.Play(data);
            }
        }

        private void OnHit(CombatLogEventPb ev)
        {
            if (!long.TryParse(ev.Actor, out var casterActorId)) return;
            if (!_actorMasterIds.TryGetValue(casterActorId, out var casterCharacterId)) return;

            if (!long.TryParse(ev.Target, out var targetActorId)) return;
            if (!_actorObjects.TryGetValue(targetActorId, out var targetGo)) return;

            int breakthrough = GetBreakthrough(casterCharacterId);
            var sd = _skillFxDb.GetByCharacterId(casterCharacterId);
            if (sd == null) return;

            var fxSet = sd.GetFxSet(breakthrough);
            if (fxSet == null) return;

            if (fxSet.hitSound != null)
                AudioSource.PlayClipAtPoint(fxSet.hitSound, targetGo.transform.position);

            // (선택적으로 여기서 hitFx를 따로 처리)
            // 단, 이 SkillFxSet에 hitFx가 없는 경우 skillFx를 비활성화하여 처리해야 함.
        }

        private int GetBreakthrough(int characterId)
        {
            var user = GameState.Instance.CurrentUser;
            if (user.UserCharactersDict.TryGetValue(characterId, out var c))
                return c.BreakThrough;
            return 0;
        }
        private void OnNormalAttack(CombatLogEventPb ev)
        {
            if (!long.TryParse(ev.Actor, out var attackerActorId)) return;
            if (!_actorObjects.TryGetValue(attackerActorId, out var attackerGo)) return;
            if (!_actorMasterIds.TryGetValue(attackerActorId, out var characterId)) return;

            // 타겟 (hitSound는 타겟 기준으로 나중에 처리)
            GameObject targetGo = null;
            if (!string.IsNullOrEmpty(ev.Target) &&
                long.TryParse(ev.Target, out var targetActorId))
            {
                _actorObjects.TryGetValue(targetActorId, out targetGo);
            }

            bool isCrit = ev.Crit ?? false;

            // 공격/피격 애니메이션은 SkillFxDb 유무와 관계없이 항상 재생
            attackerGo.GetComponent<CombatActorView>()?.PlayAttack(isCrit);
            targetGo?.GetComponent<CombatActorView>()?.PlayHitFx(isCrit);

            // FX/사운드는 SkillFxDb가 있을 때만
            var sd = _skillFxDb.GetByCharacterId(characterId);
            if (sd == null) return;

            // 노말/크리티컬 FX Set 선택
            var fxSet = isCrit ? sd.criticalAttackFx : sd.normalAttackFx;
            if (fxSet == null) return;

            // 사운드 (castSound는 공격 시작음, hitSound는 피격음)
            if (fxSet.castSound != null)
                AudioManager.Instance.PlaySFX(fxSet.castSound);

            if (fxSet.hitSound != null)
            {
                var pos = targetGo != null ? targetGo.transform.position : attackerGo.transform.position;
                AudioManager.Instance.PlaySFX(fxSet.hitSound);
            }

            // FX (투사체 등)
            if (fxSet.skillFx != null)
            {
                Vector3 source = attackerGo.transform.position;
                Vector3 target = (targetGo != null)
                    ? targetGo.transform.position
                    : source + attackerGo.transform.forward * 2f;

                var fx = Object.Instantiate(fxSet.skillFx, parent);
                fx.transform.localScale = Vector3.one * fxSet.fxScale;

                float duration = 1.0f;
                float radius = 0.5f;

                var data = new VfxData(source, target, duration, radius);
                data.SetGround(new Vector3(target.x, 0f, target.z));
                fx.Play(data);
            }
        }
        private void OnSkillHit(CombatLogEventPb ev)
        {
            // 1. 캐스터
            if (!long.TryParse(ev.Actor, out var casterActorId)) return;
            if (!_actorMasterIds.TryGetValue(casterActorId, out var casterCharacterId)) return;

            // 2. 타겟
            if (!long.TryParse(ev.Target, out var targetActorId)) return;
            if (!_actorObjects.TryGetValue(targetActorId, out var targetGo)) return;

            // 3. 스킬 ID (필수!)
            if (ev.Extra == null ||
                !ev.Extra.Fields.TryGetValue("skillId", out var skillIdValue))
                return;

            int skillId =
                skillIdValue.KindCase == Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue
                    ? (int)skillIdValue.NumberValue
                    : int.Parse(skillIdValue.StringValue);

            // 4. SkillData 가져오기
            var sd = _skillFxDb.GetByCharacterId(casterCharacterId);
            if (sd == null) return;

            int breakthrough = GetBreakthrough(casterCharacterId);
            var fxSet = sd.GetFxSet(breakthrough);
            if (fxSet == null) return;

            // 5. 히트 사운드
            if (fxSet.hitSound != null)
                AudioManager.Instance.PlaySFX(fxSet.hitSound);

            // 6. 히트 FX (선택)
            if (fxSet.skillFx != null)
            {
                Vector3 pos = targetGo.transform.position;

                var fx = Object.Instantiate(fxSet.skillFx, parent);
                fx.transform.localScale = Vector3.one * fxSet.fxScale;

                var data = new VfxData(
                    source: pos,
                    target: pos,
                    duration: 0.6f,
                    radius: 0.8f
                );
                data.SetGround(new Vector3(pos.x, 0f, pos.z));
                fx.Play(data);
            }
        }
    }
}
