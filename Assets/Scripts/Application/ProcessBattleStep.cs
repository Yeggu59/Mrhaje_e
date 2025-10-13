// ����
// ���� �� ���� ���� �������̽�
// ���� ���� ���� �ǵ� ���� ���� ��� ���� ��ٿ� ���� �� �̵� ����
//
// ��� ��Ģ
// �����Ӵ� �� �� ȣ���� �⺻���� �Ѵ�
// ���� ���� �ÿ��� �ƹ� �۾��� ���� �ʴ´�

using Game.Battle.Domain;
using Game.Battle.Domain.World;
using WorldBattle = Game.Battle.Domain.World.Battle;



namespace Game.Battle.Application
{
    public sealed class ProcessBattleStep
    {
        private readonly ExecuteAction _exec = new ExecuteAction();

        // ����: public ActionResult Run(WorldBattle battle)
        // PATCH:
        public ActionResult Run(WorldBattle battle, float deltaSeconds)
        {
            if (battle.IsFinished) return ActionResult.Wait("battle finished");

            var actor = battle.Current();
            var view = new BattleView(battle);
            var action = actor.DecideNext(view);

            var result = _exec.Apply(battle, action);

            battle.CleanupDeaths();
            battle.TickAllCooldowns(deltaSeconds); // PATCH: dt ����
            battle.AdvanceTurn();

            return result;
        }
    }
}
