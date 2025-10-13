// 목적
// IAction 를 실제로 적용해 전투 상태를 변경
// 이동 공격 대기 처리와 단순 데미지 공식 포함
//
// 사용 규칙
// 적용 이후 사망 정리는 외부 루프에서 CleanupDeaths 로 처리한다
// 데미지 공식은 임시이며 이후 교체 가능

using Game.Battle.Domain;
using Game.Battle.Domain.World;
using WorldBattle = Game.Battle.Domain.World.Battle;


namespace Game.Battle.Application
{
    public sealed class ExecuteAction
    {
        private static int CalcDamage(Domain.Unit.Unit atk, Domain.Unit.Unit def)
        {
            var a = atk.MemoryRef.ToRuntime().Calculated.Attack;
            var d = def.MemoryRef.ToRuntime().Calculated.Defense;
            return System.Math.Max(1, a - d);
        }

        public ActionResult Apply(WorldBattle battle, IAction a)
        {
            switch (a)
            {
                case MoveAction m:
                    battle.UnitOf(m.Source).State.MoveTo(battle.Bounds1D.Clamp(m.Dest));
                    return ActionResult.Move($"{m.Source.Value} moves to {m.Dest.x:0.##}");

                case AttackAction at:
                    var src = battle.UnitOf(at.Source);
                    var tgt = battle.UnitOf(at.Target);
                    var dmg = CalcDamage(src, tgt);
                    tgt.State.ApplyDamage(dmg);

                    // PATCH: 유닛별 초 단위 쿨다운 적용
                    var cdSec = System.Math.Max(0.0, src.Runtime.Calculated.AttackCooldownSec);
                    src.Attack.SetCooldownSeconds((float)cdSec);

                    return ActionResult.Attack($"{at.Source.Value} hits {at.Target.Value} for {dmg}");


                case WaitAction w:
                    return ActionResult.Wait($"{w.Source.Value} waits");

                default:
                    return ActionResult.Wait("unknown action");
            }
        }
    }
}
