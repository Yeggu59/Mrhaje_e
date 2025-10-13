// 목적
// Unit 엔티티와 내부 상태 정의
// UnitState 는 HP 와 위치를 관리
// AttackState 는 공격 쿨다운 등 전투 상태 관리
// Unit 은 MemoryRuntime 의 정책을 이용해 행동 의도를 결정
//
// 사용 규칙
// DecideNext 는 의도만 반환하며 상태 변경은 Application 계층에서만 수행한다
// 전투 중 MemoryRuntime 교체는 하지 않는다 정비 단계에서만 가능

using Game.Battle.Domain.Core;
using Game.Battle.Domain.Memory;
using Game.Battle.Domain.World;
using UnityEngine; // Mathf 사용


namespace Game.Battle.Domain.Unit
{
    public sealed class UnitState
    {
        public int HP { get; private set; }
        public Pos1 Position { get; private set; }

        public UnitState(int hp, Pos1 pos)
        {
            HP = hp;
            Position = pos;
        }

        public void ApplyDamage(int dmg)
        {
            var v = dmg < 0 ? 0 : dmg;
            HP = System.Math.Max(0, HP - v);
        }

        public bool IsAlive => HP > 0;

        public void MoveTo(Pos1 p) => Position = p;
    }

 // Assets/Scripts/Domain/Unit/Unit.cs
// 파일 상단에 추가

    public sealed class AttackState
    {
        // PATCH: 초 단위로 보유
        public float CooldownSec { get; private set; }

        public AttackState(float cdSec = 0f)
        {
            CooldownSec = Mathf.Max(0f, cdSec);
        }

        // PATCH: 초 단위 설정
        public void SetCooldownSeconds(float sec)
        {
            CooldownSec = Mathf.Max(0f, sec);
        }

        // PATCH: dt 만큼 감소
        public void ReduceCooldown(float deltaSec)
        {
            if (CooldownSec <= 0f) return;
            CooldownSec = Mathf.Max(0f, CooldownSec - Mathf.Max(0f, deltaSec));
        }
    }


    public sealed class Unit
    {
        public UnitId Id { get; }
        public Faction Faction { get; }
        public UnitState State { get; set; }
        public AttackState Attack { get; }
        public MemoryBuild MemoryRef { get; private set; }
        private readonly MemoryRuntime _runtime;
        public MemoryRuntime Runtime => _runtime;

        public Unit(UnitId id, Faction faction, UnitState state, AttackState attack, MemoryBuild memory)
        {
            Id = id;
            Faction = faction;
            State = state;
            Attack = attack;
            MemoryRef = memory;
            _runtime = memory.ToRuntime();
        }

        public IAction DecideNext(BattleView world)
        {
            var step = _runtime.Calculated.Speed;
            var range = _runtime.Calculated.Range;
            var bounds = world.Bounds1D;

            return _runtime.Decide.Decide(
                this,
                world,
                _runtime.Move,
                _runtime.Target,
                _runtime.Attack,
                step,
                range,
                bounds
            );
        }
    }
}
