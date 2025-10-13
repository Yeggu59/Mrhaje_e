// 목적
// 전투 한 스텝 실행 유스케이스
// 현재 유닛 선택 의도 결정 실행 사망 정리 쿨다운 감소 턴 이동 순서
//
// 사용 규칙
// 프레임당 한 번 호출을 기본으로 한다
// 전투 종료 시에는 아무 작업도 하지 않는다

using Game.Battle.Domain;
using Game.Battle.Domain.World;
using WorldBattle = Game.Battle.Domain.World.Battle;



namespace Game.Battle.Application
{
    public sealed class ProcessBattleStep
    {
        private readonly ExecuteAction _exec = new ExecuteAction();

        // 기존: public ActionResult Run(WorldBattle battle)
        // PATCH:
        public ActionResult Run(WorldBattle battle, float deltaSeconds)
        {
            if (battle.IsFinished) return ActionResult.Wait("battle finished");

            var actor = battle.Current();
            var view = new BattleView(battle);
            var action = actor.DecideNext(view);

            var result = _exec.Apply(battle, action);

            battle.CleanupDeaths();
            battle.TickAllCooldowns(deltaSeconds); // PATCH: dt 적용
            battle.AdvanceTurn();

            return result;
        }
    }
}
