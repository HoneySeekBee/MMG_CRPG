using Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class CombatDirector
    {
        private CombatNetwork _network;

        private long _combatId;
        private int _tick = 0;

        public bool BattleEnded { get; private set; } = false;

        public event Action<CombatSnapshotPb, IList<CombatLogEventPb>> OnTickApplied;

        public event Action<CombatLogEventPb> OnCombatEvent;

        public event Action OnBattleEnd;
        public CombatDirector(CombatNetwork network)
        {
            _network = network;
        }

        public void Init(long combatId)
        {
            _combatId = combatId;
            _tick = 0;
            BattleEnded = false;
        }

        public IEnumerator Tick()
        {
            if (BattleEnded) yield break;

            CombatTickResponsePb tickRes = null;

            yield return _network.TickAsync(_combatId, _tick, res =>
            {
                if (!res.Ok)
                {
                    Debug.LogError("[CombatDirector] Tick failed: " + res.Message);
                    return;
                }

                tickRes = res.Data;
            });

            if (tickRes == null) yield break;

            // Snapshot + Events ����
            OnTickApplied?.Invoke(tickRes.Snapshot, tickRes.Events);

            // ���� �̺�Ʈ ó�� (skill_hit ��)
            foreach (var ev in tickRes.Events)
            {
                OnCombatEvent?.Invoke(ev);

                if (ev.Type == "stage_cleared")
                {
                    BattleEnded = true;
                    OnBattleEnd?.Invoke();
                }
            }

            _tick++;
        }
    }

}