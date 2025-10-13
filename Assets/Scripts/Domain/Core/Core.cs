// 목적
// 1 Unity 환경 호환을 위해 Mathf 사용
// 2 1차원 좌표 Pos1 과 전장 경계 Range1 제공
// 3 도메인 공통 타입 UnitId 와 Faction 제공
//
// 사용 규칙
// Domain 전역에서 좌표와 경계 연산을 이 값 객체로 통일해 사용한다
// Mathf 의존이 싫다면 이후 전용 헬퍼로 치환 가능하다
//.

using UnityEngine;

namespace Game.Battle.Domain.Core
{
    public readonly struct Pos1
    {
        public readonly float x;
        public Pos1(float x) { this.x = x; }
        public static Pos1 operator +(Pos1 p, float dx) => new Pos1(p.x + dx);
        public static float Distance(Pos1 a, Pos1 b) => Mathf.Abs(a.x - b.x);
    }

    public readonly struct Range1
    {
        public readonly float min;
        public readonly float max;

        public Range1(float min, float max)
        {
            var lo = Mathf.Min(min, max);
            var hi = Mathf.Max(min, max);
            this.min = lo;
            this.max = hi;
        }

        public Pos1 Clamp(Pos1 p) => new Pos1(Mathf.Clamp(p.x, min, max));
        public bool Contains(Pos1 p) => p.x >= min && p.x <= max;
    }

    public readonly struct UnitId
    {
        public readonly int Value;
        public UnitId(int v) { Value = v; }
        public override string ToString() => Value.ToString();
    }

    public enum Faction : byte
    {
        Ally = 0,
        Enemy = 1
    }
}
