// 목적
// 전투 상태 컨테이너와 읽기 전용 뷰 제공
// 유닛 목록 턴 순환 현재 행위자 선택 종료 판정 조회 헬퍼 포함
// 적 사망 즉시 제거 아군 사망은 전투 종료 후 정산을 위해 마킹한다
//
// 사용 규칙
// 상태 보관과 조회만 담당하고 정책이나 연출은 포함하지 않는다
// 쓰기 작업은 Application 계층 유스케이스에서 수행한다

using System.Collections.Generic;
using System.Linq;
using Game.Battle.Domain.Core;
using Game.Battle.Domain.Unit;

namespace Game.Battle.Domain.World
{
    public sealed class Battle
    {
        private readonly Dictionary<UnitId, Unit.Unit> _units = new Dictionary<UnitId, Unit.Unit>();
        private readonly List<UnitId> _order = new List<UnitId>();
        private int _cursor = 0;

        public Range1 Bounds1D { get; }
        private readonly HashSet<UnitId> _fallenAllies = new HashSet<UnitId>();

        public Battle(Range1 bounds, IEnumerable<Unit.Unit> units)
        {
            Bounds1D = bounds;
            foreach (var u in units)
            {
                _units[u.Id] = u;
                _order.Add(u.Id);
            }
        }

        public bool IsFinished =>
            !_units.Values.Any(u => u.Faction == Faction.Ally && u.State.IsAlive) ||
            !_units.Values.Any(u => u.Faction == Faction.Enemy && u.State.IsAlive);

        public Unit.Unit UnitOf(UnitId id) => _units[id];

        public Unit.Unit Current()
        {
            for (int i = 0; i < _order.Count; i++)
            {
                int idx = (_cursor + i) % _order.Count;
                var u = _units[_order[idx]];
                if (u.State.IsAlive) { _cursor = idx; return u; }
            }
            return _units[_order[_cursor]];
        }

        public void AdvanceTurn()
        {
            if (_order.Count == 0) return;
            _cursor = (_cursor + 1) % _order.Count;
        }

        public void TickAllCooldowns(float deltaSec)
        {
            foreach (var u in _units.Values)
                if (u.State.IsAlive)
                    u.Attack.ReduceCooldown(deltaSec);
        }

        // 데미지 적용 뒤 호출. 적은 즉시 제거, 아군은 마킹
        public void CleanupDeaths()
        {
            var toRemove = new List<UnitId>();
            foreach (var id in _order)
            {
                var u = _units[id];
                if (!u.State.IsAlive && u.Faction == Faction.Enemy)
                    toRemove.Add(id);
            }
            foreach (var id in toRemove)
            {
                _units.Remove(id);
                _order.Remove(id);
            }
            if (_order.Count > 0) _cursor %= _order.Count;

            foreach (var kv in _units)
            {
                var u = kv.Value;
                if (!u.State.IsAlive && u.Faction == Faction.Ally)
                    _fallenAllies.Add(u.Id);
            }
        }

        public IEnumerable<UnitId> FallenAllies => _fallenAllies;
        public void ClearFallenAllies() => _fallenAllies.Clear();

        // 조회 헬퍼
        public IEnumerable<UnitId> AliveEnemies(UnitId selfId)
        {
            var me = _units[selfId];
            return _units.Values.Where(u => u.Faction != me.Faction && u.State.IsAlive).Select(u => u.Id);
        }

        public IEnumerable<UnitId> AliveAllies(UnitId selfId)
        {
            var me = _units[selfId];
            return _units.Values.Where(u => u.Faction == me.Faction && u.State.IsAlive).Select(u => u.Id);
        }

        public bool IsEnemy(UnitId a, UnitId b) => _units[a].Faction != _units[b].Faction;
        public Pos1 PositionOf(UnitId id) => _units[id].State.Position;

        public Pos1? NearestEnemyPos(UnitId selfId)
        {
            var me = _units[selfId].State.Position;
            Pos1? best = null;
            float bestDist = float.MaxValue;

            foreach (var e in AliveEnemies(selfId))
            {
                var p = _units[e].State.Position;
                var d = Pos1.Distance(me, p);
                if (d < bestDist) { best = p; bestDist = d; }
            }
            return best;
        }

        public bool TryGetUnit(UnitId id, out Unit.Unit u) => _units.TryGetValue(id, out u);
        public System.Collections.Generic.IEnumerable<Unit.Unit> AllUnits() => _units.Values;

    }

    // 읽기 전용 뷰
    public sealed class BattleView
    {
        private readonly Battle _b;
        public BattleView(Battle b) { _b = b; }

        public Range1 Bounds1D => _b.Bounds1D;
        public IEnumerable<UnitId> AliveEnemies(UnitId self) => _b.AliveEnemies(self);
        public IEnumerable<UnitId> AliveAllies(UnitId self) => _b.AliveAllies(self);
        public bool IsEnemy(UnitId a, UnitId b) => _b.IsEnemy(a, b);
        public Pos1 PositionOf(UnitId id) => _b.PositionOf(id);
        public Pos1? NearestEnemyPos(UnitId self) => _b.NearestEnemyPos(self);
    }
}
