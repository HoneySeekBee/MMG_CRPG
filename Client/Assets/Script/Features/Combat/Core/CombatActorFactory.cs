using Combat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task BuildFromSnapshot(CombatInitialSnapshotPb snapshot, Dictionary<long, GameObject> actorObjects, Dictionary<long, CombatTeam> actorTeams,
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
                var go = await CreateActorGameObject(actor.MasterId, actor.Team);
                if (go == null) continue;

                go.transform.SetParent(_parent, worldPositionStays: true);

                var view = go.GetComponent<CombatActorView>();
                if (view != null)
                    view.InitFromServer(actor.ActorId, actor.Team, actor.Hp);

                Vector3 worldPos = new Vector3(actor.X, 0f, actor.Z);
                go.transform.position = worldPos;

                actorObjects[actor.ActorId] = go;
                actorTeams[actor.ActorId] = (CombatTeam)actor.Team;
                actorWaveIndex[actor.ActorId] = actor.WaveIndex;
                actorMasterIds[actor.ActorId] = (int)actor.MasterId;

                if (actor.Team == (int)CombatTeam.Enemy)
                {
                    go.SetActive(false);
                    enemyActorIds.Add(actor.ActorId);
                }
                else
                {
                    playerSpawnPos[actor.ActorId] = worldPos;

                    if (view != null)
                        view.SetSpawnPosition(worldPos);

                    int characterId = (int)actor.MasterId;
                    int level = _getCharacterLevel?.Invoke(characterId) ?? 1;
                    onCreateSkillButton?.Invoke(characterId, actor.ActorId, level);
                }
            }
        }

        private async Task<GameObject> CreateActorGameObject(long masterId, int team)
        {
            if (masterId == 0) return null;

            if (team == (int)CombatTeam.Player)
            {
                GameObject character = PartySetManager.Instance.GetCharacterObject();
                var chaBase = character.GetComponent<CharacterBase>();
                if (chaBase != null)
                    await chaBase.Set(CharacterCache.Instance.CharacterModelById[(int)masterId], true);
                return character;
            }

            GameObject monster = UnityEngine.Object.Instantiate(_monsterBasePrefab, _parent);
            var monsterBase = monster.GetComponent<MonsterBase>();
            if (monsterBase != null)
                await monsterBase.Set(MonsterCache.Instance.MonstersById[(int)masterId]);

            monster.SetActive(false);
            return monster;
        }

    }
}