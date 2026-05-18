using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame
{
    // Panel.DoubleBuffered property'si protected olduğundan,
    // kalıtım yoluyla çift tampon desteği etkinleştirilen özel panel.
    // Bu sayede yeniden çizimde titreme olmaz.
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint            |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            UpdateStyles();
        }
    }

    // Oyun tahtasının arka planını, gridin çizgilerini ve metin katmanlarını çizer.
    // Snake, Food ve Particle nesneleri kendi Render metodlarını kullanır.
    public class GameRenderer
    {
        // Renk sabitleri — tüm çizim buradan koordineli yönetilir
        private readonly Color BgColor     = Color.FromArgb(13, 18, 32);
        private readonly Color GridColor   = Color.FromArgb(26, 37, 64);
        private readonly Color AccentColor = Color.FromArgb(0, 245, 212);

        public int CellSize { get; }
        public int Cols     { get; }
        public int Rows     { get; }

        public GameRenderer(int cols, int rows, int cellSize = 28)
        {
            Cols     = cols;
            Rows     = rows;
            CellSize = cellSize;
        }

        // Oyun alanının tamamını arka plan rengiyle doldurur
        public void DrawBackground(Graphics g)
        {
            using var brush = new SolidBrush(BgColor);
            g.FillRectangle(brush, 0, 0, Cols * CellSize, Rows * CellSize);
        }

        // Izgara çizgileri — her hücreyi görsel olarak ayırır
        public void DrawGrid(Graphics g)
        {
            using var pen = new Pen(GridColor, 1f);
            for (int x = 0; x <= Cols; x++)
                g.DrawLine(pen, x * CellSize, 0, x * CellSize, Rows * CellSize);
            for (int y = 0; y <= Rows; y++)
                g.DrawLine(pen, 0, y * CellSize, Cols * CellSize, y * CellSize);
        }

        // Oyun alanının etrafına neon çerçeve çizer
        public void DrawBorder(Graphics g)
        {
            int w = Cols * CellSize, h = Rows * CellSize;
            using var pen = new Pen(Color.FromArgb(180, AccentColor), 2f);
            g.DrawRectangle(pen, 1, 1, w - 2, h - 2);
        }

        // Ekranın ortasına büyük başlık + alt yazı çizer (idle/pause/gameover için)
        public void DrawCenteredText(Graphics g, string line1, string line2,
                                     Color c1, Color c2)
        {
            int w = Cols * CellSize, h = Rows * CellSize;

            using var f1 = new Font("Segoe UI", 28, FontStyle.Bold);
            using var f2 = new Font("Segoe UI", 14, FontStyle.Regular);
            using var b1 = new SolidBrush(c1);
            using var b2 = new SolidBrush(c2);

            SizeF s1 = g.MeasureString(line1, f1);
            SizeF s2 = g.MeasureString(line2, f2);

            g.DrawString(line1, f1, b1, (w - s1.Width) / 2f, (h / 2f) - 40);
            g.DrawString(line2, f2, b2, (w - s2.Width) / 2f, (h / 2f) + 10);
        }
    }
}
