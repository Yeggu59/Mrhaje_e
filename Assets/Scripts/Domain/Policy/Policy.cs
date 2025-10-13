// ����
// ���� �ǻ������ �ʿ��� ��å �������̽��� �⺻ ���� ����
// IMovePolicy ITargetPolicy IAttackPolicy IDecisionPolicy �� ���� ���ҷ� �и�
// �⺻ ������ ���� ����� ������ �����ϰ� ��Ÿ� �������� �����Ѵ�
//
// ��� ��Ģ
// ��å�� �б� ���� ��ȸ�� �����ϸ� ���� ������ ���� �ʴ´�
// Ȯ�� �� ���� �ڵ� ���� ���� �� ���� Ŭ������ �߰��Ѵ�

using Game.Battle.Domain.Core;
using Game.Battle.Domain.Unit;
using Game.Battle.Domain.World;
using Game.Battle.Domain; // IAction, MoveAction ��

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

            // PATCH: �� ���� ��ٿ� Ȯ��
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

            // ��Ÿ� �ȿ� ���� Ÿ���� �ִ��� �Ÿ��� ��Ȯ��
            if (tgt.HasValue)
            {
                var me = self.State.Position;
                var tp = world.PositionOf(tgt.Value);
                var inRange = Pos1.Distance(me, tp) <= maxRange;

                // ��Ÿ� ���̰� ��ٿ 0�̸� ����
                if (inRange && self.Attack.CooldownSec <= 0f)
                    return new AttackAction(self.Id, tgt.Value);

                // ��Ÿ� ���ε� ��ٿ� ������ �� ������ �� �ڸ����� ���
                if (inRange && self.Attack.CooldownSec > 0f)
                    return new WaitAction(self.Id);
            }

            // ��Ÿ� ���̸� ����
            var next = move.Choose(self, world, step, bounds);
            if (next.x != self.State.Position.x) return new MoveAction(self.Id, next);

            return new WaitAction(self.Id);
        }
    }

}
