// ����
// Unit ��ƼƼ�� ���� ���� ����
// UnitState �� HP �� ��ġ�� ����
// AttackState �� ���� ��ٿ� �� ���� ���� ����
// Unit �� MemoryRuntime �� ��å�� �̿��� �ൿ �ǵ��� ����
//
// ��� ��Ģ
// DecideNext �� �ǵ��� ��ȯ�ϸ� ���� ������ Application ���������� �����Ѵ�
// ���� �� MemoryRuntime ��ü�� ���� �ʴ´� ���� �ܰ迡���� ����

using Game.Battle.Domain.Core;
using Game.Battle.Domain.Memory;
using Game.Battle.Domain.World;
using UnityEngine; // Mathf ���


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
// ���� ��ܿ� �߰�

    public sealed class AttackState
    {
        // PATCH: �� ������ ����
        public float CooldownSec { get; private set; }

        public AttackState(float cdSec = 0f)
        {
            CooldownSec = Mathf.Max(0f, cdSec);
        }

        // PATCH: �� ���� ����
        public void SetCooldownSeconds(float sec)
        {
            CooldownSec = Mathf.Max(0f, sec);
        }

        // PATCH: dt ��ŭ ����
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
