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

        public CombatVfxPresenter(SkillFxDataList skillFxDb, Dictionary<long, GameObject> actorObjects, Dictionary<long, int> actorMasterIds)
        {
            _skillFxDb = skillFxDb;
            _actorObjects = actorObjects;
            _actorMasterIds = actorMasterIds;
        }
        public void HandleEvent(CombatLogEventPb ev)
        {
            Debug.Log($"[HandleEvnet] {ev.Type}");
            switch (ev.Type)
            {
                case "skill_cast":
                    OnSkillCast(ev);
                    break;

                case "normal_attack":
                    OnNormalAttack(ev);
                    break;

                case "hit":
                    OnHit(ev); // (기존) 데미지 히트 사운드용, 필요하면 유지
                    break;

                // 스킬 타격도 따로 오면 여기 추가
                case "skill_hit":
                case "skill_hit_aoe":
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

            // 애니메이션(있는 경우)
            casterGo.GetComponent<CombatActorView>()?.PlayAttack(false);

            // 사운드
            if (fxSet.castSound != null)
                AudioSource.PlayClipAtPoint(fxSet.castSound, casterGo.transform.position);

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

                var fx = Object.Instantiate(fxSet.skillFx);

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

            // (원하면 여기서 hitFx도 재생 가능)
            // 단, 너 SkillFxSet에 hitFx를 따로 넣을지, skillFx를 재활용할지 결정해야 함.
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

            // 타겟 (없어도 되긴 하는데 hitSound는 타겟 기준이 더 자연스러움)
            GameObject targetGo = null;
            if (!string.IsNullOrEmpty(ev.Target) &&
                long.TryParse(ev.Target, out var targetActorId))
            {
                _actorObjects.TryGetValue(targetActorId, out targetGo);
            }

            var sd = _skillFxDb.GetByCharacterId(characterId);
            if (sd == null) return;

            bool isCrit = ev.Crit?? false; // proto에서 bool이면 바로 사용 가능

            //  공격 애니메이션
            attackerGo.GetComponent<CombatActorView>()?.PlayAttack(isCrit);

            //  평타 FX Set 선택
            var fxSet = isCrit ? sd.criticalAttackFx : sd.normalAttackFx;
            if (fxSet == null) return;

            // 사운드 (castSound를 “스윙”으로 쓸지, hitSound를 “피격”으로 쓸지 선택)
            if (fxSet.castSound != null)
                AudioSource.PlayClipAtPoint(fxSet.castSound, attackerGo.transform.position);

            if (fxSet.hitSound != null)
            {
                var pos = targetGo != null ? targetGo.transform.position : attackerGo.transform.position;
                AudioSource.PlayClipAtPoint(fxSet.hitSound, pos);
            }

            //  FX (있으면 재생)
            if (fxSet.skillFx != null)
            {
                Vector3 source = attackerGo.transform.position;
                Vector3 target = (targetGo != null)
                    ? targetGo.transform.position
                    : source + attackerGo.transform.forward * 2f;

                var fx = Object.Instantiate(fxSet.skillFx);

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
                AudioSource.PlayClipAtPoint(fxSet.hitSound, targetGo.transform.position);

            // 6. 히트 FX (선택)
            if (fxSet.skillFx != null)
            {
                Vector3 pos = targetGo.transform.position;

                var fx = Object.Instantiate(fxSet.skillFx);

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