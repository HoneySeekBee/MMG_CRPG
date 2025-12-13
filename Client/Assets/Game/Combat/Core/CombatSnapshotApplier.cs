using Combat;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 서버 snapshot 받아서 유닛에 저굥ㅇ 
namespace Game.Combat
{
    public class CombatSnapshotApplier
    {
        private class ActorLastState
        {
            public Vector3 Pos;
            public int Hp;
            public bool Dead;
        }

        private readonly Dictionary<long, GameObject> _actorObjects;
        private readonly Dictionary<long, ActorLastState> _lastStates = new();

        // 튜닝값
        private readonly float _moveThreshold;

        public CombatSnapshotApplier(Dictionary<long, GameObject> actorObjects, float moveThreshold = 0.01f)
        {
            _actorObjects = actorObjects;
            _moveThreshold = moveThreshold;
        }

        public void Clear()
        {
            _lastStates.Clear();
        }

        public void Apply(CombatSnapshotPb snapshot, IList<CombatLogEventPb> eventsThisTick)
        {
            if (snapshot == null || snapshot.Actors == null)
                return;

            foreach (var a in snapshot.Actors)
            {
                if (!_actorObjects.TryGetValue(a.ActorId, out var go) || go == null)
                    continue;

                var view = go.GetComponent<CombatActorView>();
                if (view == null)
                    continue;

                // 스냅샷 기준으로 살아있으면 꺼져있던 오브젝트 켜기 (spawn 이벤트 누락 대비)
                if (!a.Dead && !go.activeSelf)
                    go.SetActive(true);

                if (!_lastStates.TryGetValue(a.ActorId, out var prev))
                {
                    prev = new ActorLastState
                    {
                        Pos = view.transform.position,
                        Hp = view.Hp,
                        Dead = false
                    };
                    _lastStates[a.ActorId] = prev;
                }

                // 1) 위치 적용
                var newPos = new Vector3(a.X, 0f, a.Z);
                float moveDist = Vector3.Distance(prev.Pos, newPos);

                view.transform.position = newPos;

                // 2) 이동/Idle 애니
                bool isMoving = moveDist > _moveThreshold && !a.Dead;
                if (isMoving) view.PlayMove();
                else if (!a.Dead) view.PlayIdle();

                // 3) HP 적용
                int prevHp = prev.Hp;
                view.SetHp(a.Hp);

                // 4) 죽음 처리 (상태 변화시에만)
                if (!prev.Dead && a.Dead)
                    view.OnDie();

                // 상태 저장
                prev.Pos = newPos;
                prev.Hp = a.Hp;
                prev.Dead = a.Dead;
                _lastStates[a.ActorId] = prev;
            }
        }
        public static bool HasEventThisTick(IList<CombatLogEventPb> eventsThisTick, string type, string actorId = null, string targetId = null)
        {
            if (eventsThisTick == null) return false;

            return eventsThisTick.Any(ev =>
                ev.Type == type &&
                (actorId == null || ev.Actor == actorId) &&
                (targetId == null || ev.Target == targetId)
            );
        }
    }
}
