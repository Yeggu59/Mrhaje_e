// 목적
// 전투 의도를 표현하는 액션 모델 정의
// IAction 과 Move Attack Wait 을 포함
// ActionResult 는 로그 전달용 최소 정보만 포함
//
// 사용 규칙
// 액션은 상태를 직접 변경하지 않는다
// 상태 변경은 Application 계층에서만 수행한다

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
