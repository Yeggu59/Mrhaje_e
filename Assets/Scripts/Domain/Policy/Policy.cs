// 목적
// 전투 의사결정에 필요한 정책 인터페이스와 기본 구현 제공
// IMovePolicy ITargetPolicy IAttackPolicy IDecisionPolicy 네 가지 역할로 분리
// 기본 구현은 가장 가까운 적에게 접근하고 사거리 내에서는 공격한다
//
// 사용 규칙
// 정책은 읽기 전용 조회만 수행하며 상태 변경은 하지 않는다
// 확장 시 기존 코드 수정 없이 새 구현 클래스를 추가한다

using Game.Battle.Domain.Core;
using Game.Battle.Domain.Unit;
using Game.Battle.Domain.World;
using Game.Battle.Domain; // IAction, MoveAction 등

namespace Game.Battle.Domain.Policy
{
    public interface IMovePolicy
    {
        Pos1 Choose(Unit.Unit self, BattleView world, float step, Range1 bounds);
    }

    public interface ITargetPolicy
    {
        UnitId? PickTarget(Unit.Unit self, BattleView world, float maxRange);
    }

    public interface IAttackPolicy
    {
        bool CanAttack(Unit.Unit self, UnitId target, BattleView world, float maxRange);
    }

    public interface IDecisionPolicy
    {
        IAction Decide(
            Unit.Unit self,
            BattleView world,
            IMovePolicy move,
            ITargetPolicy target,
            IAttackPolicy attack,
            float step,
            float maxRange,
            Range1 bounds);
    }

    public sealed class MoveTowardNearest : IMovePolicy
    {
        public Pos1 Choose(Unit.Unit self, BattleView world, float step, Range1 bounds)
        {
            var me = self.State.Position;
            var enemyPos = world.NearestEnemyPos(self.Id);
            if (enemyPos is null) return me;

            var dir = (enemyPos.Value.x >= me.x) ? +1f : -1f;
            return bounds.Clamp(me + dir * step);
        }
    }

    public sealed class NearestTarget : ITargetPolicy
    {
        public UnitId? PickTarget(Unit.Unit self, BattleView world, float maxRange)
        {
            UnitId? best = null;
            var bestDist = float.MaxValue;
            var me = self.State.Position;

            foreach (var e in world.AliveEnemies(self.Id))
            {
                var d = Pos1.Distance(me, world.PositionOf(e));
                if (d <= maxRange && d < bestDist)
                {
                    best = e;
                    bestDist = d;
                }
            }
            return best;
        }
    }

    // Assets/Scripts/Domain/Policy/Policy.cs

    public sealed class InRangeAttack : IAttackPolicy
    {
        public bool CanAttack(Unit.Unit self, UnitId target, BattleView world, float maxRange)
        {
            if (!world.IsEnemy(self.Id, target)) return false;

            // PATCH: 초 단위 쿨다운 확인
            if (self.Attack.CooldownSec > 0f) return false;

            var me = self.State.Position;
            var tp = world.PositionOf(target);
            return Pos1.Distance(me, tp) <= maxRange;
        }
    }


    public sealed class SimpleDecision : IDecisionPolicy
    {
        public IAction Decide(
            Unit.Unit self, BattleView world,
            IMovePolicy move, ITargetPolicy target, IAttackPolicy attack,
            float step, float maxRange, Range1 bounds)
        {
            var tgt = target.PickTarget(self, world, maxRange);

            // 사거리 안에 실제 타겟이 있는지 거리로 재확인
            if (tgt.HasValue)
            {
                var me = self.State.Position;
                var tp = world.PositionOf(tgt.Value);
                var inRange = Pos1.Distance(me, tp) <= maxRange;

                // 사거리 안이고 쿨다운도 0이면 공격
                if (inRange && self.Attack.CooldownSec <= 0f)
                    return new AttackAction(self.Id, tgt.Value);

                // 사거리 안인데 쿨다운 때문에 못 때리면 그 자리에서 대기
                if (inRange && self.Attack.CooldownSec > 0f)
                    return new WaitAction(self.Id);
            }

            // 사거리 밖이면 접근
            var next = move.Choose(self, world, step, bounds);
            if (next.x != self.State.Position.x) return new MoveAction(self.Id, next);

            return new WaitAction(self.Id);
        }
    }

}
