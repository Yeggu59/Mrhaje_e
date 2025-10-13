// 목적
// 전투 수치 템플릿과 정책 참조를 담는 Memory 계열 정의
// MemoryBuild 는 확장 훅만 남기고 ToRuntime 으로 전투용 캐시 생성
// MemoryRuntime 은 전투 중 변경되지 않는 조회 전용 데이터
//
// 사용 규칙
// 프로토타입에서는 IMemoryModifier 는 비활성 상태로 둔다
// 전투 입장 시 ToRuntime 을 통해 캐시를 한 번만 만든다

using System.Collections.Generic;
using Game.Battle.Domain.Policy;

namespace Game.Battle.Domain.Memory
{
    public sealed class StatTemplate
    {
        public int MaxHP;
        public int Attack;
        public int Defense;
        public float Speed; // 틱당 이동량
        public float Range; // 공격 사거리
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
        private readonly List<IMemoryModifier> _mods = new List<IMemoryModifier>(); // 자리만

        public MemoryBuild(Memory baseMem) { Base = baseMem; }

        public MemoryRuntime ToRuntime()
        {
            var ctx = new ComposeContext(Base);
            foreach (var m in _mods) m.Apply(ctx); // 현재 비어 있음
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
