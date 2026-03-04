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

            // �ִϸ��̼�(�ִ� ���)
            casterGo.GetComponent<CombatActorView>()?.PlayAttack(false);

            // ����
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

            // (���ϸ� ���⼭ hitFx�� ��� ����)
            // ��, �� SkillFxSet�� hitFx�� ���� ������, skillFx�� ��Ȱ������ �����ؾ� ��.
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

            // Ÿ�� (��� �Ǳ� �ϴµ� hitSound�� Ÿ�� ������ �� �ڿ�������)
            GameObject targetGo = null;
            if (!string.IsNullOrEmpty(ev.Target) &&
                long.TryParse(ev.Target, out var targetActorId))
            {
                _actorObjects.TryGetValue(targetActorId, out targetGo);
            }

            var sd = _skillFxDb.GetByCharacterId(characterId);
            if (sd == null) return;

            bool isCrit = ev.Crit?? false; // proto���� bool�̸� �ٷ� ��� ����

            //  ���� �ִϸ��̼�
            attackerGo.GetComponent<CombatActorView>()?.PlayAttack(isCrit);

            //  ��Ÿ FX Set ����
            var fxSet = isCrit ? sd.criticalAttackFx : sd.normalAttackFx;
            if (fxSet == null) return;

            // ���� (castSound�� ������������ ����, hitSound�� ���ǰݡ����� ���� ����)
            if (fxSet.castSound != null)
                AudioManager.Instance.PlaySFX(fxSet.castSound);

            if (fxSet.hitSound != null)
            {
                var pos = targetGo != null ? targetGo.transform.position : attackerGo.transform.position; 
                AudioManager.Instance.PlaySFX(fxSet.hitSound); 
            }

            //  FX (������ ���)
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
            // 1. ĳ����
            if (!long.TryParse(ev.Actor, out var casterActorId)) return;
            if (!_actorMasterIds.TryGetValue(casterActorId, out var casterCharacterId)) return;

            // 2. Ÿ��
            if (!long.TryParse(ev.Target, out var targetActorId)) return;
            if (!_actorObjects.TryGetValue(targetActorId, out var targetGo)) return;

            // 3. ��ų ID (�ʼ�!)
            if (ev.Extra == null ||
                !ev.Extra.Fields.TryGetValue("skillId", out var skillIdValue))
                return;

            int skillId =
                skillIdValue.KindCase == Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue
                    ? (int)skillIdValue.NumberValue
                    : int.Parse(skillIdValue.StringValue);

            // 4. SkillData ��������
            var sd = _skillFxDb.GetByCharacterId(casterCharacterId);
            if (sd == null) return;
             
            int breakthrough = GetBreakthrough(casterCharacterId);
            var fxSet = sd.GetFxSet(breakthrough);
            if (fxSet == null) return;

            // 5. ��Ʈ ����
            if (fxSet.hitSound != null)
                AudioManager.Instance.PlaySFX(fxSet.hitSound); 

            // 6. ��Ʈ FX (����)
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