// 목적
// 전투 종료 시 쓰러진 아군 정산
// 프로토타입에서는 HP 를 최대치로 완전 회복
//
// 사용 규칙
// 전투 종료가 확인된 뒤 한 번만 호출한다
// 정산 후 FallenAllies 마킹을 해제한다

using WorldBattle = Game.Battle.Domain.World.Battle;
using System.Linq;
using Game.Battle.Domain.World;

namespace Game.Battle.Application
{
    public static class PostBattleSettlement
    {
        public static void Settle(WorldBattle battle)
        {
            foreach (var id in battle.FallenAllies.ToList())
            {
                var u = battle.UnitOf(id);
                var maxHp = u.MemoryRef.ToRuntime().Calculated.MaxHP;
                u.State = new Game.Battle.Domain.Unit.UnitState(maxHp, u.State.Position);
            }
            battle.ClearFallenAllies();
        }
    }
}
