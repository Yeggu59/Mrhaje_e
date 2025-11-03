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
        [SerializeField] float worldOriginX = -10f; // ������ x=0�� ���� ���� X
        [SerializeField] float unitsToWorldScale = 1f; // ������ 1 ������ ���� �� ���ͷ� ����

        WorldBattle _battle;
        ProcessBattleStep _step = new ProcessBattleStep();
        bool _settled;

        // ������ ���� id���� �並 ã�´�
        readonly Dictionary<int, UnitView> _views = new Dictionary<int, UnitView>();

        // ���� ����
        UnitView SpawnViewFor(Unit u)
        {
            var prefab = u.Faction == Faction.Ally ? allyPrefab : enemyPrefab;
            if (prefab == null)
            {
                // �������� ������ �⺻ ť��� ��ü
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
            go.transform.position = new Vector3(startX, laneY, -1f);

            var view = go.GetComponent<UnitView>();
            if (view == null) view = go.AddComponent<UnitView>();
            return view;
        }

        // ������ x�� ���� x�� ��ȯ
        float DomainXToWorldX(float domainX)
        {
            return worldOriginX + domainX * unitsToWorldScale;
        }

        // ���� ������ ��� �並 �����ο� ���� ��ǥ ��ǥ�� ����ȭ
        void SyncViews()
        {
            var toRemove = new List<int>();

            foreach (var kv in _views)
            {
                var id = new UnitId(kv.Key);

                // 1) ���� �� �ϸ� �� ����
                if (!_battle.TryGetUnit(id, out var u))
                {
                    if (kv.Value) Destroy(kv.Value.gameObject);
                    toRemove.Add(kv.Key);
                    continue;
                }

                // 2) �����ϸ� ��ǥ ����ȭ
                var worldX = DomainXToWorldX(u.State.Position.x);
                kv.Value.SetTargetX(worldX);
            }

            // 3) ��ųʸ����� ����
            foreach (var k in toRemove) _views.Remove(k);
        }


        void Start()
        {
            // ����� ������ ���� ����
            var bounds = new Range1(0f, 20f);

            var memA = new Memory(
                new StatTemplate
                {
                    MaxHP = 30,
                    Attack = 5,
                    Defense = 1,
                    Speed = 1.5f,
                    Range = 1.5f,
                    AttackCooldownSec = 0.8f   // ������ ƽ���� �� ��. 
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
                    AttackCooldownSec = 0.4f   // ��: ��Ÿ��. ���� ƽ �ٷ� ����
                },
                new MoveTowardNearest(),
                new NearestTarget(),
                new InRangeAttack(),
                new SimpleDecision()
            );


            var u1 = new Unit(new UnitId(1), Faction.Ally, new UnitState(30, new Pos1(2f)), new AttackState(), new MemoryBuild(memA));
            var u2 = new Unit(new UnitId(2), Faction.Enemy, new UnitState(20, new Pos1(18f)), new AttackState(), new MemoryBuild(memB));

            _battle = new WorldBattle(bounds, new[] { u1, u2 });

            // ���������̼� ������Ʈ ���� �� ���
            _views[u1.Id.Value] = SpawnViewFor(u1);
            _views[u2.Id.Value] = SpawnViewFor(u2);

            Append("Battle start");
            // ù �����ӿ� ��ǥ ��ġ ����ȭ
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

            // PATCH: deltaTime ����
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
