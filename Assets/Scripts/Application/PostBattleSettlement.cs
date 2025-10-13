// ����
// ���� ���� �� ������ �Ʊ� ����
// ������Ÿ�Կ����� HP �� �ִ�ġ�� ���� ȸ��
//
// ��� ��Ģ
// ���� ���ᰡ Ȯ�ε� �� �� ���� ȣ���Ѵ�
// ���� �� FallenAllies ��ŷ�� �����Ѵ�

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
