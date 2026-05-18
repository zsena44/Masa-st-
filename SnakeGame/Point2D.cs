using System;

namespace SnakeGame
{
    // Oyun tahtasındaki bir hücrenin x ve y koordinatını tutan yapı.
    // struct kullandık çünkü koordinat değişmez ve küçük bir veri.
    // IEquatable sayesinde == operatörünü kendimiz tanımlayabildik.
    public readonly struct Point2D : IEquatable<Point2D>
    {
        public int X { get; }
        public int Y { get; }

        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        // İki koordinatı toplayıp yeni bir Point2D döner (hareket hesaplamak için)
        public Point2D Add(Point2D other) => new Point2D(X + other.X, Y + other.Y);

        // Eşitlik kontrolü — iki hücre aynı mı?
        public bool Equals(Point2D other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Point2D p && Equals(p);
        public override int GetHashCode() => HashCode.Combine(X, Y);

        // == ve != operatörlerini de kullanabilmek için tanımladık
        public static bool operator ==(Point2D a, Point2D b) => a.Equals(b);
        public static bool operator !=(Point2D a, Point2D b) => !a.Equals(b);

        public override string ToString() => $"({X},{Y})";
    }
}
