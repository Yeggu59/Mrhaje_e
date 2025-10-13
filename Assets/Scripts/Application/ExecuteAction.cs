// ����
// IAction �� ������ ������ ���� ���¸� ����
// �̵� ���� ��� ó���� �ܼ� ������ ���� ����
//
// ��� ��Ģ
// ���� ���� ��� ������ �ܺ� �������� CleanupDeaths �� ó���Ѵ�
// ������ ������ �ӽ��̸� ���� ��ü ����

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

                    // PATCH: ���ֺ� �� ���� ��ٿ� ����
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
