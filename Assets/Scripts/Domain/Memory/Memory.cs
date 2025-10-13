// ����
// ���� ��ġ ���ø��� ��å ������ ��� Memory �迭 ����
// MemoryBuild �� Ȯ�� �Ÿ� ����� ToRuntime ���� ������ ĳ�� ����
// MemoryRuntime �� ���� �� ������� �ʴ� ��ȸ ���� ������
//
// ��� ��Ģ
// ������Ÿ�Կ����� IMemoryModifier �� ��Ȱ�� ���·� �д�
// ���� ���� �� ToRuntime �� ���� ĳ�ø� �� ���� �����

using System.Collections.Generic;
using Game.Battle.Domain.Policy;

namespace Game.Battle.Domain.Memory
{
    public sealed class StatTemplate
    {
        public int MaxHP;
        public int Attack;
        public int Defense;
        public float Speed; // ƽ�� �̵���
        public float Range; // ���� ��Ÿ�
        public float AttackCooldownSec;
        public StatTemplate Clone() => (StatTemplate)MemberwiseClone();
    }

    public sealed class Memory
    {
        public StatTemplate BaseStats { get; }
        public IMovePolicy MovePolicy { get; }
        public ITargetPolicy TargetPolicy { get; }
        public IAttackPolicy AttackPolicy { get; }
        public IDecisionPolicy DecisionPolicy { get; }

        public Memory(
            StatTemplate stats,
            IMovePolicy move,
            ITargetPolicy target,
            IAttackPolicy attack,
            IDecisionPolicy decide)
        {
            BaseStats = stats;
            MovePolicy = move;
            TargetPolicy = target;
            AttackPolicy = attack;
            DecisionPolicy = decide;
        }
    }

    public interface IMemoryModifier
    {
        void Apply(ComposeContext ctx);
    }

    public sealed class ComposeContext
    {
        public StatTemplate Stats;
        public IMovePolicy Move;
        public ITargetPolicy Target;
        public IAttackPolicy Attack;
        public IDecisionPolicy Decide;

        public ComposeContext(Memory src)
        {
            Stats = src.BaseStats.Clone();
            Move = src.MovePolicy;
            Target = src.TargetPolicy;
            Attack = src.AttackPolicy;
            Decide = src.DecisionPolicy;
        }

        public MemoryRuntime Build() => new MemoryRuntime(Stats, Move, Target, Attack, Decide);
    }

    public sealed class MemoryBuild
    {
        public Memory Base { get; }
        private readonly List<IMemoryModifier> _mods = new List<IMemoryModifier>(); // �ڸ���

        public MemoryBuild(Memory baseMem) { Base = baseMem; }

        public MemoryRuntime ToRuntime()
        {
            var ctx = new ComposeContext(Base);
            foreach (var m in _mods) m.Apply(ctx); // ���� ��� ����
            return ctx.Build();
        }
    }

    public sealed class MemoryRuntime
    {
        public StatTemplate Calculated { get; }
        public IMovePolicy Move { get; }
        public ITargetPolicy Target { get; }
        public IAttackPolicy Attack { get; }
        public IDecisionPolicy Decide { get; }

        public MemoryRuntime(
            StatTemplate s,
            IMovePolicy m,
            ITargetPolicy t,
            IAttackPolicy a,
            IDecisionPolicy d)
        {
            Calculated = s;
            Move = m;
            Target = t;
            Attack = a;
            Decide = d;
        }
    }
}
