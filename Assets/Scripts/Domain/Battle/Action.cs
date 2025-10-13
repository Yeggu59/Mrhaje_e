// ����
// ���� �ǵ��� ǥ���ϴ� �׼� �� ����
// IAction �� Move Attack Wait �� ����
// ActionResult �� �α� ���޿� �ּ� ������ ����
//
// ��� ��Ģ
// �׼��� ���¸� ���� �������� �ʴ´�
// ���� ������ Application ���������� �����Ѵ�

using Game.Battle.Domain.Core;

namespace Game.Battle.Domain
{
    public interface IAction { UnitId Source { get; } }

    public readonly struct MoveAction : IAction
    {
        public UnitId Source { get; }
        public Pos1 Dest { get; }
        public MoveAction(UnitId src, Pos1 dest) { Source = src; Dest = dest; }
        public override string ToString() => $"Move to {Dest.x:0.##}";
    }

    public readonly struct AttackAction : IAction
    {
        public UnitId Source { get; }
        public UnitId Target { get; }
        public AttackAction(UnitId src, UnitId tgt) { Source = src; Target = tgt; }
        public override string ToString() => $"Attack {Target.Value}";
    }

    public readonly struct WaitAction : IAction
    {
        public UnitId Source { get; }
        public WaitAction(UnitId src) { Source = src; }
        public override string ToString() => "Wait";
    }

    public sealed class ActionResult
    {
        public string Log { get; }
        private ActionResult(string log) { Log = log; }
        public static ActionResult Move(string log) => new ActionResult(log);
        public static ActionResult Attack(string log) => new ActionResult(log);
        public static ActionResult Wait(string log) => new ActionResult(log);
    }
}
