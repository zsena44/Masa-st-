using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;  

namespace SnakeGame
{
    // Tüm yiyeceklerin türediği soyut temel sınıf.
    // Her yiyecek tipi Points ve TypeName'i kendisi belirlemek zorunda (polymorphism).
    public abstract class Food : GameObject
    {
        protected static readonly Random Rng = new Random();
        protected int Cols, Rows;

        // Alt sınıflar bu iki özelliği doldurmak zorunda
        public abstract int Points { get; }
        public abstract string TypeName { get; }

        protected Food(int cols, int rows)
            : base(new Point2D(Rng.Next(0, cols), Rng.Next(0, rows)))
        {
            Cols = cols;
            Rows = rows;
        }

        // Yiyecek yenildiğinde yılanın olmadığı rastgele bir hücreye taşır
        public void Respawn(IReadOnlyCollection<Point2D> snakeBody)
        {
            Point2D newPos;
            do
            {
                newPos = new Point2D(Rng.Next(0, Cols), Rng.Next(0, Rows));
            } while (snakeBody.Contains(newPos));

            Position = newPos;
        }

        // Yiyecek kendi kendine hareket etmez, Update boş bırakıldı
        public override void Update() { }
    }

    // ─────────────────────────────────────────────────────────────────
    // Normal yiyecek — 10 puan, pembe neon görünüm, nabız animasyonu
    // ─────────────────────────────────────────────────────────────────
    public class NormalFood : Food
    {
        public override int Points => 10;
        public override string TypeName => "Normal";

        // Nabız animasyonu için sinüs açısı
        private float _pulse;

        public NormalFood(int cols, int rows) : base(cols, rows) { }

        public override void Render(Graphics g, int cellSize)
        {
            // Her render çağrısında açıyı ilerlet (nabız efekti)
            _pulse = (_pulse + 0.12f) % (MathF.PI * 2);
            float glow = 0.6f + 0.4f * MathF.Sin(_pulse);

            int margin = 4;
            int rx = Position.X * cellSize + margin;
            int ry = Position.Y * cellSize + margin;
            int rw = cellSize - margin * 2;
            int rh = cellSize - margin * 2;

            // Dış parıltı (gradient path)
            int glowR = (int)(rw * 1.6f);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rx - (glowR - rw) / 2, ry - (glowR - rh) / 2, glowR, glowR);
                using var pgb = new PathGradientBrush(path);
                pgb.CenterColor    = Color.FromArgb((int)(100 * glow), 247, 37, 133);
                pgb.SurroundColors = new[] { Color.Transparent };
                g.FillPath(pgb, path);
            }

            // Yiyeceğin ana gövdesi (pembe neon)
            using var border = new Pen(Color.FromArgb(200, 247, 37, 133), 2f);
            using var fill   = new SolidBrush(Color.FromArgb(60, 247, 37, 133));
            g.FillRectangle(fill, rx, ry, rw, rh);
            g.DrawRectangle(border, rx, ry, rw, rh);

            // Küçük yaprak detayı
            using var leafPen = new Pen(Color.FromArgb(200, 100, 220, 80), 1.5f);
            int lx = rx + rw / 2, ly = ry;
            g.DrawBezier(leafPen,
                new PointF(lx, ly),
                new PointF(lx + 4, ly - 5),
                new PointF(lx + 8, ly - 8),
                new PointF(lx + 5, ly - 3));
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Bonus yiyecek — 50 puan, dönen yıldız, süresi dolunca yok olur
    // ─────────────────────────────────────────────────────────────────
    public class BonusFood : Food
    {
        public override int Points => 50;
        public override string TypeName => "Bonus";

        private float _angle;    // dönen yıldız açısı
        private int   _lifetime; // kalan adım sayısı

        // _lifetime sıfıra düşünce GameLoop bunu siler
        public bool IsExpired => _lifetime <= 0;

        public BonusFood(int cols, int rows) : base(cols, rows)
        {
            _lifetime = 80; // yaklaşık 8 saniye (her 6 frame'de bir tick)
        }

        // Her oyun adımında açıyı döndür ve ömrü azalt
        public override void Update()
        {
            _lifetime--;
            _angle += 0.15f;
        }

        public override void Render(Graphics g, int cellSize)
        {
            int cx = Position.X * cellSize + cellSize / 2;
            int cy = Position.Y * cellSize + cellSize / 2;
            int r  = cellSize / 2 - 3;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 5 köşeli dönen yıldız — 10 nokta (dış ve iç yarıçap dönüşümlü)
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = _angle + i * MathF.PI / 5f;
                float rad   = (i % 2 == 0) ? r : r * 0.45f;
                pts[i] = new PointF(cx + MathF.Cos(angle) * rad,
                                    cy + MathF.Sin(angle) * rad);
            }

            using var fill = new SolidBrush(Color.FromArgb(200, 255, 200, 0));
            using var pen  = new Pen(Color.FromArgb(255, 255, 160, 0), 1.5f);
            g.FillPolygon(fill, pts);
            g.DrawPolygon(pen, pts);

            g.SmoothingMode = SmoothingMode.None;
        }
    }
}
