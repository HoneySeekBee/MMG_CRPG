using Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebServer.Protos.Monsters;
namespace Game.Combat
{
    public class CombatActorFactory
    {
        private readonly Transform _parent;
        private readonly GameObject _monsterBasePrefab;
        private readonly Func<int, int> _getCharacterLevel;

        public CombatActorFactory(Transform parent, GameObject monsterBasePrefab, Func<int, int> getCharacterLevel)
        {
            _parent = parent;
            _monsterBasePrefab = monsterBasePrefab;
            _getCharacterLevel = getCharacterLevel;
        }

        public void BuildFromSnapshot(CombatInitialSnapshotPb snapshot, Dictionary<long, GameObject> actorObjects, Dictionary<long, CombatTeam> actorTeams,
            Dictionary<long, int> actorWaveIndex, Dictionary<long, Vector3> playerSpawnPos, Dictionary<long, int> actorMasterIds, List<long> enemyActorIds, Action<int, long, int> onCreateSkillButton = null)
        {
            if (snapshot == null) return;

            foreach (var kv in actorObjects)
            {
                if (kv.Value != null)
                    UnityEngine.Object.Destroy(kv.Value);
            }

            actorObjects.Clear();
            actorTeams.Clear();
            actorWaveIndex.Clear();
            playerSpawnPos.Clear();
            actorMasterIds.Clear();
            enemyActorIds.Clear();

            foreach (var actor in snapshot.Actors)
            {
                var go = CreateActorGameObject(actor.MasterId, actor.Team);
                if (go == null) continue;

                go.transform.SetParent(_parent, worldPositionStays: true);

                var view = go.GetComponent<CombatActorView>();
                if (view != null)
                {
                    view.InitFromServer(actor.ActorId, actor.Team, actor.Hp);
                }

                Vector3 worldPos = new Vector3(actor.X, 0f, actor.Z);
                go.transform.position = worldPos;

                actorObjects[actor.ActorId] = go;
                actorTeams[actor.ActorId] = (CombatTeam)actor.Team;
                actorWaveIndex[actor.ActorId] = actor.WaveIndex;
                actorMasterIds[actor.ActorId] = (int)actor.MasterId;

                if (actor.Team == (int)CombatTeam.Enemy)
                {
                    // 적은 spawn 이벤트로 켜줄 거라면 기본 비활성
                    go.SetActive(false);
                    enemyActorIds.Add(actor.ActorId);
                }
                else // Player
                {
                    playerSpawnPos[actor.ActorId] = worldPos;

                    if (view != null)
                        view.SetSpawnPosition(worldPos);

                    // 스킬 버튼 만들기 (원하면 콜백으로 뺌)
                    int characterId = (int)actor.MasterId;
                    int level = _getCharacterLevel?.Invoke(characterId) ?? 1;
                    onCreateSkillButton?.Invoke(characterId, actor.ActorId, level);
                }
            }
        }

        private GameObject CreateActorGameObject(long masterId, int team)
        {
            if (masterId == 0) return null;

            // Player
            if (team == (int)CombatTeam.Player)
            {
                GameObject character = PartySetManager.Instance.GetCharacterObject();

                var chaBase = character.GetComponent<CharacterBase>();
                if (chaBase != null)
                {
                    int characterId = (int)masterId;
                    chaBase.Set(CharacterCache.Instance.CharacterModelById[characterId], true);
                }

                return character;
            }

            // Enemy
            GameObject monster = UnityEngine.Object.Instantiate(_monsterBasePrefab, _parent);
            var monsterBase = monster.GetComponent<MonsterBase>();
            if (monsterBase != null)
            {
                int monsterId = (int)masterId;
                MonsterPb monsterPb = MonsterCache.Instance.MonstersById[monsterId];
                monsterBase.Set(monsterPb);
            }

            monster.SetActive(false);
            return monster;
        }

    }
}