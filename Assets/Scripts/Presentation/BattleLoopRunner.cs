using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Battle.Domain.Core;
using Game.Battle.Domain.Unit;
using Game.Battle.Domain.Memory;
using Game.Battle.Domain.Policy;
using Game.Battle.Domain.World;
using Game.Battle.Application;
using WorldBattle = Game.Battle.Domain.World.Battle;

namespace Game.Battle.Presentation
{
    public sealed class BattleLoopRunner : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Text log;

        [Header("Unit Prefabs")]
        [SerializeField] GameObject allyPrefab;
        [SerializeField] GameObject enemyPrefab;

        [Header("Lane Settings")]
        [SerializeField] float allyLaneY = 0f;
        [SerializeField] float enemyLaneY = 1.2f;
        [SerializeField] float worldOriginX = -10f; // 도메인 x=0일 때의 월드 X
        [SerializeField] float unitsToWorldScale = 1f; // 도메인 1 단위를 월드 몇 미터로 볼지

        WorldBattle _battle;
        ProcessBattleStep _step = new ProcessBattleStep();
        bool _settled;

        // 도메인 유닛 id에서 뷰를 찾는다
        readonly Dictionary<int, UnitView> _views = new Dictionary<int, UnitView>();

        // 스폰 헬퍼
        UnitView SpawnViewFor(Unit u)
        {
            var prefab = u.Faction == Faction.Ally ? allyPrefab : enemyPrefab;
            if (prefab == null)
            {
                // 프리팹이 없으면 기본 큐브로 대체
                prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prefab.name = "FallbackUnit";
                var col = prefab.GetComponent<Collider>();
                if (col) Destroy(col);
                var mr = prefab.GetComponent<MeshRenderer>();
                if (mr) mr.material.color = u.Faction == Faction.Ally ? Color.cyan : Color.red;
            }

            var go = Instantiate(prefab);
            go.name = $"Unit_{u.Id.Value}_{u.Faction}";
            var laneY = u.Faction == Faction.Ally ? allyLaneY : enemyLaneY;

            var startX = DomainXToWorldX(u.State.Position.x);
            go.transform.position = new Vector3(startX, laneY, 0f);

            var view = go.GetComponent<UnitView>();
            if (view == null) view = go.AddComponent<UnitView>();
            return view;
        }

        // 도메인 x를 월드 x로 변환
        float DomainXToWorldX(float domainX)
        {
            return worldOriginX + domainX * unitsToWorldScale;
        }

        // 현재 전장의 모든 뷰를 도메인에 맞춰 목표 좌표로 동기화
        void SyncViews()
        {
            var toRemove = new List<int>();

            foreach (var kv in _views)
            {
                var id = new UnitId(kv.Key);

                // 1) 존재 안 하면 뷰 제거
                if (!_battle.TryGetUnit(id, out var u))
                {
                    if (kv.Value) Destroy(kv.Value.gameObject);
                    toRemove.Add(kv.Key);
                    continue;
                }

                // 2) 존재하면 좌표 동기화
                var worldX = DomainXToWorldX(u.State.Position.x);
                kv.Value.SetTargetX(worldX);
            }

            // 3) 딕셔너리에서 제거
            foreach (var k in toRemove) _views.Remove(k);
        }


        void Start()
        {
            // 전장과 도메인 유닛 구성
            var bounds = new Range1(0f, 20f);

            var memA = new Memory(
                new StatTemplate
                {
                    MaxHP = 30,
                    Attack = 5,
                    Defense = 1,
                    Speed = 1.5f,
                    Range = 1.5f,
                    AttackCooldownSec = 0.8f   // 공속을 틱으로 해 둠. 
                },
                new MoveTowardNearest(),
                new NearestTarget(),
                new InRangeAttack(),
                new SimpleDecision()
            );

            var memB = new Memory(
                new StatTemplate
                {
                    MaxHP = 20,
                    Attack = 6,
                    Defense = 1,
                    Speed = 1.2f,
                    Range = 1.2f,
                    AttackCooldownSec = 0.4f   // 예: 연타형. 다음 틱 바로 가능
                },
                new MoveTowardNearest(),
                new NearestTarget(),
                new InRangeAttack(),
                new SimpleDecision()
            );


            var u1 = new Unit(new UnitId(1), Faction.Ally, new UnitState(30, new Pos1(2f)), new AttackState(), new MemoryBuild(memA));
            var u2 = new Unit(new UnitId(2), Faction.Enemy, new UnitState(20, new Pos1(18f)), new AttackState(), new MemoryBuild(memB));

            _battle = new WorldBattle(bounds, new[] { u1, u2 });

            // 프레젠테이션 오브젝트 생성 및 등록
            _views[u1.Id.Value] = SpawnViewFor(u1);
            _views[u2.Id.Value] = SpawnViewFor(u2);

            Append("Battle start");
            // 첫 프레임에 목표 위치 동기화
            SyncViews();
        }

        // Assets/Scripts/Presentation/BattleLoopRunner.cs

        void Update()
        {
            if (_battle.IsFinished)
            {
                if (!_settled)
                {
                    PostBattleSettlement.Settle(_battle);
                    Append("Battle finished. Allies recovered.");
                    _settled = true;
                    SyncViews();
                }
                return;
            }

            // PATCH: deltaTime 전달
            var r = _step.Run(_battle, Time.deltaTime);
            Append(r.Log);

            SyncViews();
        }


        void Append(string s)
        {
            if (log) log.text += s + "\n";
            else Debug.Log(s);
        }
    }
}
