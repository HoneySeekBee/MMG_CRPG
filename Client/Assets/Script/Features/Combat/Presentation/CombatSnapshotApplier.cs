using Combat;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ���� snapshot �޾Ƽ� ���ֿ� ������ 
namespace Game.Combat
{
    public class CombatSnapshotApplier
    {
        private class ActorLastState
        {
            public Vector3 RenderPos;
            public Vector3 TargetPos;
            public Vector3 Vel; 

            public int Hp;
            public bool Dead;
            public bool Inited;
        }

        private readonly Dictionary<long, GameObject> _actorObjects;
        private readonly Dictionary<long, CombatTeam> _actorTeams;
        private readonly Dictionary<long, ActorLastState> _states = new();
        private readonly Dictionary<long, CombatActorView> _viewCache = new();

        private readonly float _moveThreshold;
        private readonly float _smoothTime;
        private readonly float _teleportDistance;
        private bool _hardSnapThisFrame;
        public CombatSnapshotApplier(
           Dictionary<long, GameObject> actorObjects,
           Dictionary<long, CombatTeam> actorTeams,
           float moveThreshold = 0.01f,
           float smoothTime = 0.08f,
           float teleportDistance = 3.0f)
        {
            _actorObjects = actorObjects;
            _actorTeams = actorTeams;
            _moveThreshold = moveThreshold;
            _smoothTime = smoothTime;
            _teleportDistance = teleportDistance;
        }

        public void Clear()
        {
            _states.Clear();
            _viewCache.Clear();
        }

        public void Apply(CombatSnapshotPb snapshot, IList<CombatLogEventPb> eventsThisTick)
        {
            _hardSnapThisFrame =
    HasEventThisTick(eventsThisTick, "spawn")
 || HasEventThisTick(eventsThisTick, "wave_cleared");

            if (snapshot?.Actors == null) return;

            var seen = new HashSet<long>();

            foreach (var a in snapshot.Actors)
            {
                seen.Add(a.ActorId);

                if (!_actorObjects.TryGetValue(a.ActorId, out var go) || go == null)
                    continue;

                if (!_viewCache.TryGetValue(a.ActorId, out var view))
                {
                    view = go.GetComponent<CombatActorView>();
                    if (view == null) continue;
                    _viewCache[a.ActorId] = view;
                }

                if (!a.Dead && !go.activeSelf)
                    go.SetActive(true);

                if (!_states.TryGetValue(a.ActorId, out var st))
                {
                    st = new ActorLastState();
                    _states[a.ActorId] = st;
                }

                var newTarget = new Vector3(a.X, 0f, a.Z);

                if (!st.Inited)
                {
                    view.transform.position = newTarget;
                    st.RenderPos = newTarget;
                    st.TargetPos = newTarget;
                    st.Vel = Vector3.zero;
                    st.Hp = a.Hp;
                    st.Dead = a.Dead;
                    st.Inited = true;
                }

                st.TargetPos = newTarget;

                // HP/Dead
                view.SetHp(a.Hp);

                if (!st.Dead && a.Dead)
                {
                    view.OnDie();
                    SkillButton characterSkillBtn;
                    if (BattleMapPopup.Instance.SkillButtonDic.TryGetValue(view.ActorId, out characterSkillBtn))
                    {
                        characterSkillBtn.CharacterDead();
                    } 
                }

                st.Hp = a.Hp;
                st.Dead = a.Dead;
            }

            foreach (var kv in _actorObjects)
            {
                var actorId = kv.Key;
                var go = kv.Value;
                if (go == null) continue;

                if (_actorTeams.TryGetValue(actorId, out var team) && team != CombatTeam.Enemy)
                    continue;

                if (!seen.Contains(actorId))
                {
                    if (go.activeSelf) go.SetActive(false);
                    _states.Remove(actorId);
                }
            }
        }
        public void UpdateRender(float dt)
        {
            foreach (var kv in _states)
            {
                long actorId = kv.Key;
                var st = kv.Value;

                if (!_actorObjects.TryGetValue(actorId, out var go) || go == null || !go.activeSelf)
                    continue;

                if (!_viewCache.TryGetValue(actorId, out var view)) continue;

                var cur = view.transform.position;
                var target = st.TargetPos;

                float dist = Vector3.Distance(cur, target);
                if (_hardSnapThisFrame || dist > _teleportDistance * 3f)
                {
                    view.transform.position = target;
                    st.Vel = Vector3.zero;
                    view.ResetFacingCache();
                }
                else
                {
                    view.transform.position = Vector3.SmoothDamp(cur, target, ref st.Vel, _smoothTime);
                }
                view.UpdateFacingByMovement(dt);

                float moved = Vector3.Distance(st.RenderPos, view.transform.position);
                bool isMoving = moved > _moveThreshold && !st.Dead;

                if (view.State != CombatActorView.ActionState.Attack)
                {
                    if (isMoving) view.PlayMove();
                    else if (!st.Dead) view.PlayIdle();
                }

                st.RenderPos = view.transform.position;
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
