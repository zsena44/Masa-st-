using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SnakeGame
{
    // Yılan nesnesi — GameObject'ten türetildi.
    // Gövde LinkedList olarak tutulur: baş eklenir, kuyruk silinir.
    public class Snake : GameObject
    {
        // Gövde dışarıdan değiştirilemesin diye private readonly
        private readonly LinkedList<Point2D> _body;

        private Dir _currentDir; // şu an gidilen yön
        private Dir _nextDir;    // bir sonraki adımda uygulanacak yön
        private bool _grow;      // true ise bu adımda kuyruk silinmeyecek (büyüme)
        private int _cols;
        private int _rows;

        // Dışarıdan okunabilir ama değiştirilemez erişimciler
        public Point2D Head => _body.First.Value;
        public IReadOnlyCollection<Point2D> Body => _body;
        public int Length => _body.Count;

        // Neon renk gradyanı için renk sabitleri
        private static readonly Color HeadColor  = Color.FromArgb(0, 245, 212);
        private static readonly Color BodyColor1 = Color.FromArgb(8, 145, 178);
        private static readonly Color BodyColor2 = Color.FromArgb(3, 78, 115);

        public Snake(int startX, int startY, int cols, int rows)
            : base(new Point2D(startX, startY))
        {
            _cols = cols;
            _rows = rows;

            _body = new LinkedList<Point2D>();
            // Başlangıçta 3 hücreli yılan — baş sağa bakıyor
            _body.AddFirst(new Point2D(startX, startY));
            _body.AddLast(new Point2D(startX - 1, startY));
            _body.AddLast(new Point2D(startX - 2, startY));

            _currentDir = Dir.Right;
            _nextDir    = Dir.Right;
            _grow       = false;
        }

        // Yön değiştirme isteği — U-dönüşü (tam ters yön) reddedilir
        public void SetDirection(Dir d)
        {
            if (d != DirectionHelper.Opposite(_currentDir))
                _nextDir = d;
        }

        // Bir sonraki Update'te yılanı büyütmek için işaretler
        public void Grow() => _grow = true;

        // Yılanı bir adım ilerletir.
        // Kenara çarparsa karşı taraftan çıkar (wrap-around — duvar çarpışması yok).
        public override void Update()
        {
            _currentDir = _nextDir;

            var vec    = DirectionHelper.ToVector(_currentDir);
            var rawHead = Head.Add(vec);

            // Modüler aritmetik ile kenar sarımı (negatif değerler için +cols/rows ekliyoruz)
            int nx = ((rawHead.X % _cols) + _cols) % _cols;
            int ny = ((rawHead.Y % _rows) + _rows) % _rows;
            var newHead = new Point2D(nx, ny);

            _body.AddFirst(newHead);
            Position = newHead;

            if (_grow)
                _grow = false; // kuyruk silinmeyince gövde bir hücre uzamış olur
            else
                _body.RemoveLast();
        }

        // Wrap-around modunda duvar çarpışması olmadığı için her zaman false döner
        public bool HitsWall(int cols, int rows) => false;

        // Baş, gövdenin geri kalanına değdi mi?
        public bool HitsSelf()
        {
            var h = Head;
            return _body.Skip(1).Any(p => p == h);
        }

        // Yılanın başı yiyeceğin üzerinde mi?
        public bool CollidesWithFood(Point2D foodPos) => Head == foodPos;

        // ── Çizim ───────────────────────────────────────────────────
        public override void Render(Graphics g, int cellSize)
        {
            int margin   = 2;
            var bodyList = _body.ToList();

            // Kuyruktan başa doğru çiz (baş üstte kalsın)
            for (int i = bodyList.Count - 1; i >= 0; i--)
            {
                var cell = bodyList[i];

                // t: 0 = baş, 1 = kuyruk — renk gradyanı için
                float t = bodyList.Count > 1 ? (float)i / (bodyList.Count - 1) : 0f;

                Color c = i == 0
                    ? HeadColor
                    : LerpColor(BodyColor1, BodyColor2, t);

                int rx = cell.X * cellSize + margin;
                int ry = cell.Y * cellSize + margin;
                int rw = cellSize - margin * 2;
                int rh = cellSize - margin * 2;

                // İç dolgu (yarı saydam)
                using var brush = new SolidBrush(Color.FromArgb(180, c));
                g.FillRectangle(brush, rx, ry, rw, rh);

                // Neon kenar çizgisi
                using var pen = new Pen(Color.FromArgb(220, c), 1.5f);
                g.DrawRectangle(pen, rx, ry, rw, rh);

                // 3D görünüm için sol ve üst kenara parlak çizgi
                using var highlight = new Pen(Color.FromArgb(120, 255, 255, 255), 1f);
                g.DrawLine(highlight, rx, ry, rx + rw, ry);
                g.DrawLine(highlight, rx, ry, rx, ry + rh);

                // Sadece başa göz çiz
                if (i == 0) DrawEyes(g, cell, cellSize, _currentDir);
            }
        }

        // Yılanın başına bakış yönüne göre iki göz çizer
        private void DrawEyes(Graphics g, Point2D head, int cs, Dir dir)
        {
            int ex1, ey1, ex2, ey2;
            int s = Math.Max(2, cs / 7); // göz boyutu
            int q = cs / 4;

            // Yöne göre göz konumlarını hesapla
            switch (dir)
            {
                case Dir.Right:
                    ex1 = head.X * cs + cs - q; ey1 = head.Y * cs + q;
                    ex2 = head.X * cs + cs - q; ey2 = head.Y * cs + cs - q - s;
                    break;
                case Dir.Left:
                    ex1 = head.X * cs + q - s; ey1 = head.Y * cs + q;
                    ex2 = head.X * cs + q - s; ey2 = head.Y * cs + cs - q - s;
                    break;
                case Dir.Up:
                    ex1 = head.X * cs + q;         ey1 = head.Y * cs + q - s;
                    ex2 = head.X * cs + cs - q - s; ey2 = head.Y * cs + q - s;
                    break;
                default: // Down
                    ex1 = head.X * cs + q;         ey1 = head.Y * cs + cs - q;
                    ex2 = head.X * cs + cs - q - s; ey2 = head.Y * cs + cs - q;
                    break;
            }

            using var eyeBrush = new SolidBrush(Color.FromArgb(230, 220, 30, 30));
            g.FillEllipse(eyeBrush, ex1, ey1, s, s);
            g.FillEllipse(eyeBrush, ex2, ey2, s, s);
        }

        // İki renk arasında t oranında doğrusal interpolasyon
        private static Color LerpColor(Color a, Color b, float t)
            => Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
    }
}
