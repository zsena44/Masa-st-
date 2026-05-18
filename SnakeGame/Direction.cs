using System;

namespace SnakeGame
{
    // Yön sabitleri — yılanın gidebileceği 4 yön
    public enum Dir { Up, Down, Left, Right }

    // Yönle ilgili yardımcı metotları barındıran static sınıf
    public static class DirectionHelper
    {
        // Verilen yönü birim vektöre çevirir (grid üzerinde hareket için)
        public static Point2D ToVector(Dir d) => d switch
        {
            Dir.Up    => new Point2D(0, -1),
            Dir.Down  => new Point2D(0, 1),
            Dir.Left  => new Point2D(-1, 0),
            Dir.Right => new Point2D(1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(d))
        };

        // Verilen yönün tam tersini döner — U-dönüşünü engellemek için kullanıyoruz
        public static Dir Opposite(Dir d) => d switch
        {
            Dir.Up    => Dir.Down,
            Dir.Down  => Dir.Up,
            Dir.Left  => Dir.Right,
            Dir.Right => Dir.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(d))
        };
    }
}
