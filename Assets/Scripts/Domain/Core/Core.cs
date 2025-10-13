// ����
// 1 Unity ȯ�� ȣȯ�� ���� Mathf ���
// 2 1���� ��ǥ Pos1 �� ���� ��� Range1 ����
// 3 ������ ���� Ÿ�� UnitId �� Faction ����
//
// ��� ��Ģ
// Domain �������� ��ǥ�� ��� ������ �� �� ��ü�� ������ ����Ѵ�
// Mathf ������ �ȴٸ� ���� ���� ���۷� ġȯ �����ϴ�
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
