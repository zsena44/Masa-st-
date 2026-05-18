using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SnakeGame
{
    // Ana oyun penceresi.
    // Tüm oyun nesnelerini oluşturur, döngüyü yönetir ve UI'ı günceller.
    public class SnakeGameForm : Form
    {
        // ── Sabitler ────────────────────────────────────────────────
        private const int Cols     = 22;
        private const int Rows     = 18;
        private const int CellSize = 28;

        // ── Oyun nesneleri ───────────────────────────────────────────
        private readonly GameRenderer     _renderer;
        private readonly ScoreManager     _scoreMgr;
        private readonly GameStateManager _stateMgr;
        private readonly ParticleSystem   _particles;

        private Snake     _snake;
        private Food      _food;
        private BonusFood _bonus;

        // Kaç frame'de bir oyun adımı atılacak (hız kontrolü)
        private int _frameCount;
        private int _stepEvery = 8;

        // ── UI kontrolleri ───────────────────────────────────────────
        private readonly DoubleBufferedPanel _gamePanel;
        private readonly Label _lblScore, _lblHigh, _lblLevel, _lblSpeed;
        private readonly Button _btnStart, _btnPause, _btnRestart, _btnQuit, _btnLeaderboard;
        private readonly Timer _timer;

        // Ekrana çizmeden önce bitmap'e yazmak için (double buffering)
        private Bitmap _buffer;

        // ─────────────────────────────────────────────────────────────
        public SnakeGameForm()
        {
            _renderer  = new GameRenderer(Cols, Rows, CellSize);
            _scoreMgr  = new ScoreManager();
            _stateMgr  = new GameStateManager();
            _particles = new ParticleSystem();

            // ── Form genel ayarları ──────────────────────────────────
            Text            = "SNAKE GAME";
            BackColor       = Color.FromArgb(10, 14, 26);
            ForeColor       = Color.FromArgb(224, 240, 255);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            KeyPreview      = true; // klavye olayları forma ulaşsın
            Font            = new Font("Segoe UI", 9);

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            int boardW = Cols * CellSize;
            int boardH = Rows * CellSize;

            // ── Oyun paneli (titreme önleyici) ────────────────────
            _gamePanel = new DoubleBufferedPanel
            {
                Location  = new Point(12, 36),
                Size      = new Size(boardW, boardH),
                BackColor = Color.FromArgb(13, 18, 32)
            };
            _gamePanel.Paint += GamePanel_Paint;
            Controls.Add(_gamePanel);

            _buffer = new Bitmap(boardW, boardH);

            // ── Form başlık etiketi ───────────────────────────────
            var titleBar = new Label
            {
                Text      = "SNAKE GAME",
                Font      = new Font("Consolas", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 245, 212),
                Location  = new Point(12, 10),
                AutoSize  = true
            };
            Controls.Add(titleBar);

            // ── Sağ panel — skor bilgileri ve butonlar ────────────
            int px = boardW + 24;
            int py = 36;

            _lblScore = MakeLabel("SCORE",      "0",   Color.FromArgb(0, 245, 212),   ref py, px);
            _lblHigh  = MakeLabel("HIGH SCORE", "0",   Color.FromArgb(247, 37, 133),  ref py, px);
            _lblLevel = MakeLabel("LEVEL",      "1",   Color.FromArgb(180, 120, 255), ref py, px);
            _lblSpeed = MakeLabel("SPEED",      "1×",  Color.FromArgb(80, 160, 255),  ref py, px);

            py += 10;
            _btnStart       = MakeButton("▶  BAŞLA",    ref py, px);
            _btnPause       = MakeButton("⏸  DURDUR",  ref py, px, enabled: false);
            _btnRestart     = MakeButton("↺  YENİDEN", ref py, px, enabled: false);
            _btnLeaderboard = MakeButton("🏆  LIDERLIK", ref py, px);
            _btnQuit        = MakeButton("✕  ÇIKIŞ",   ref py, px, danger: true);

            _btnStart.Click       += (_, __) => StartGame();
            _btnPause.Click       += (_, __) => TogglePause();
            _btnRestart.Click     += (_, __) => StartGame();
            _btnLeaderboard.Click += (_, __) => OpenLeaderboard();
            _btnQuit.Click        += (_, __) => Application.Exit();

            ClientSize = new Size(px + 180, boardH + 48);

            // ── Oyun döngüsü — ~60 FPS ───────────────────────────
            _timer          = new Timer { Interval = 16 };
            _timer.Tick    += GameLoop;
            _timer.Start();

            KeyDown += OnKeyDown;
        }

        // ── UI yardımcı metotları ────────────────────────────────────

        // Başlık + değer etiketi çifti oluşturur (skor/level/hız gösterimi için)
        private Label MakeLabel(string caption, string value,
                                Color accent, ref int py, int px)
        {
            var cap = new Label
            {
                Text      = caption,
                Font      = new Font("Consolas", 8, FontStyle.Bold),
                ForeColor = accent,
                Location  = new Point(px, py),
                AutoSize  = true
            };
            Controls.Add(cap);
            py += 18;

            var val = new Label
            {
                Text      = value,
                Font      = new Font("Consolas", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(224, 240, 255),
                Location  = new Point(px, py),
                AutoSize  = true,
                Tag       = caption
            };
            Controls.Add(val);
            py += 46;

            return val;
        }

        // Tek tip buton oluşturur — danger=true ise kırmızı renk
        private Button MakeButton(string text, ref int py, int px,
                                  bool enabled = true, bool danger = false)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Consolas", 9, FontStyle.Bold),
                ForeColor = danger ? Color.FromArgb(247, 80, 100) : Color.FromArgb(0, 245, 212),
                BackColor = Color.FromArgb(17, 24, 39),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(168, 38),
                Location  = new Point(px, py),
                Enabled   = enabled
            };
            btn.FlatAppearance.BorderColor = danger
                ? Color.FromArgb(80, 30, 40)
                : Color.FromArgb(30, 60, 95);
            Controls.Add(btn);
            py += 46;

            return btn;
        }

        // ── Oyun mantığı ─────────────────────────────────────────────

        // Yeni oyun başlatır — state makineyi sıfırlayıp nesneleri yeniden oluşturur
        private void StartGame()
        {
            try
            {
                // Önceki state ne olursa olsun Idle'a döndür
                if (!_stateMgr.Is(GameState.Idle))
                {
                    if (_stateMgr.Is(GameState.Paused))   _stateMgr.Transition(GameState.Idle);
                    else if (_stateMgr.Is(GameState.Playing)) _stateMgr.Transition(GameState.GameOver);

                    if (_stateMgr.Is(GameState.GameOver)) _stateMgr.Transition(GameState.Idle);
                }
                _stateMgr.Transition(GameState.Playing);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"State hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _scoreMgr.Reset();
            _snake     = new Snake(Cols / 2, Rows / 2, Cols, Rows);
            _food      = new NormalFood(Cols, Rows);
            _bonus     = null;
            _frameCount = 0;
            _stepEvery  = 8;

            _btnPause.Enabled   = true;
            _btnRestart.Enabled = true;
            _btnPause.Text      = "⏸  DURDUR";

            UpdateUI();
        }

        // Oyunu duraklatır veya devam ettirir
        private void TogglePause()
        {
            try
            {
                if (_stateMgr.Is(GameState.Playing))
                {
                    _stateMgr.Transition(GameState.Paused);
                    _btnPause.Text = "▶  DEVAM";
                }
                else if (_stateMgr.Is(GameState.Paused))
                {
                    _stateMgr.Transition(GameState.Playing);
                    _btnPause.Text = "⏸  DURDUR";
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Duraklat hatası: {ex.Message}");
            }
        }

        // Oyun bitti — kullanıcıya skor kaydetme diyaloğu gösterir
        private void GameOver()
        {
            try { _stateMgr.Transition(GameState.GameOver); }
            catch { /* zaten game over durumundaysa geç */ }

            _btnPause.Enabled = false;
            UpdateUI();

            // Skor kaydetme formu
            var dlg = new Form
            {
                Text            = "OYUN BİTTİ",
                Size            = new Size(340, 200),
                BackColor       = Color.FromArgb(17, 24, 39),
                ForeColor       = Color.White,
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false
            };

            var lbl = new Label
            {
                Text      = $"Skorun: {_scoreMgr.Score}   Level: {_scoreMgr.LevelDisplay}",
                Font      = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 245, 212),
                AutoSize  = true,
                Location  = new Point(30, 20)
            };

            var txtName = new TextBox
            {
                PlaceholderText = "Adını gir...",
                Location        = new Point(30, 60),
                Size            = new Size(200, 30),
                Font            = new Font("Consolas", 11),
                BackColor       = Color.FromArgb(13, 18, 32),
                ForeColor       = Color.White
            };

            var btnSave = new Button
            {
                Text      = "KAYDET",
                Location  = new Point(240, 58),
                Size      = new Size(70, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(0, 245, 212),
                BackColor = Color.FromArgb(17, 24, 39)
            };
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(30, 60, 95);
            btnSave.Click += (_, __) =>
            {
                try
                {
                    string name = string.IsNullOrWhiteSpace(txtName.Text) ? "Anonim" : txtName.Text;
                    _scoreMgr.SaveEntry(name, _scoreMgr.Score, _scoreMgr.Level);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Kaydetme Hatası");
                }
                dlg.Close();
            };

            var btnSkip = new Button
            {
                Text      = "Geç",
                Location  = new Point(30, 108),
                Size      = new Size(280, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(100, 120, 150),
                BackColor = Color.FromArgb(17, 24, 39)
            };
            btnSkip.FlatAppearance.BorderColor = Color.FromArgb(30, 40, 60);
            btnSkip.Click += (_, __) => dlg.Close();

            dlg.Controls.AddRange(new Control[] { lbl, txtName, btnSave, btnSkip });
            dlg.ShowDialog(this);
        }

        // ── Ana oyun döngüsü (~60 FPS) ───────────────────────────────
        private void GameLoop(object sender, EventArgs e)
        {
            if (_stateMgr.Is(GameState.Playing))
            {
                _frameCount++;
                _particles.Update(); // partiküller her frame ilerler

                // Oyun mantığı her _stepEvery frame'de bir çalışır (hız kontrolü)
                if (_frameCount % _stepEvery == 0)
                {
                    try
                    {
                        // Bonus yiyecek ömrünü azalt, süresi dolunca sil
                        if (_bonus != null)
                        {
                            _bonus.Update();
                            if (_bonus.IsExpired) _bonus = null;
                        }

                        // Rastgele bonus yiyecek çıkar (düşük olasılık)
                        if (_bonus == null && _scoreMgr.Score > 0 && new Random().Next(400) == 0)
                            _bonus = new BonusFood(Cols, Rows);

                        _food.Update();
                        _snake.Update();

                        // Kendine çarpma — oyun biter
                        if (_snake.HitsSelf())
                            throw new GameException("Kendine çarpıldı!");

                        // Normal yiyecek yeme
                        if (_snake.CollidesWithFood(_food.Position))
                        {
                            _snake.Grow();
                            // Level arttıkça her yiyecek biraz daha fazla puan verir
                            _scoreMgr.AddPoints(_food.Points + (_scoreMgr.Level - 1) * 2);
                            EmitParticles(_food.Position, Color.FromArgb(247, 37, 133));
                            _food.Respawn(_snake.Body);
                            UpdateUI();
                        }

                        // Bonus yiyecek yeme
                        if (_bonus != null && _snake.CollidesWithFood(_bonus.Position))
                        {
                            _snake.Grow();
                            _scoreMgr.AddPoints(_bonus.Points);
                            EmitParticles(_bonus.Position, Color.Gold);
                            _bonus = null;
                            UpdateUI();
                        }

                        // Hız: level arttıkça _stepEvery azalır (max hız 8x)
                        int speedLvl = Math.Min(_scoreMgr.Level, 8);
                        _stepEvery   = Math.Max(3, 9 - speedLvl);
                        _lblSpeed.Text = $"{speedLvl}×";
                    }
                    catch (GameException gex)
                    {
                        // Ölüm partikülleri — iki renk birden fırlat
                        EmitParticles(_snake.Head, Color.FromArgb(247, 37, 133));
                        EmitParticles(_snake.Head, Color.FromArgb(0, 245, 212));
                        GameOver();
                        Console.WriteLine($"[Oyun] {gex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Beklenmeyen hata] {ex}");
                    }
                }
            }

            // Oyun durumu ne olursa olsun her frame ekranı yenile
            _gamePanel.Invalidate();
        }

        // Hücre merkezinden partikül fırlatan yardımcı metot
        private void EmitParticles(Point2D cell, Color col)
        {
            float cx = cell.X * CellSize + CellSize / 2f;
            float cy = cell.Y * CellSize + CellSize / 2f;
            _particles.Emit(cx, cy, 20, col);
        }

        // ── Çizim ────────────────────────────────────────────────────
        // Önce bitmap'e çizer, sonra bitmap'i tek seferde ekrana kopyalar (titreme önleme)
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            using var g = Graphics.FromImage(_buffer);
            g.SmoothingMode = SmoothingMode.None;

            _renderer.DrawBackground(g);
            _renderer.DrawGrid(g);

            // Oyun nesneleri sadece Playing veya Paused durumunda çizilir
            if (_stateMgr.Is(GameState.Playing) || _stateMgr.Is(GameState.Paused))
            {
                _food?.Render(g, CellSize);
                _bonus?.Render(g, CellSize);
                _snake?.Render(g, CellSize);
                _particles.Render(g);
            }

            // Duruma göre overlay metin
            if (_stateMgr.Is(GameState.Idle))
                _renderer.DrawCenteredText(g,
                    "SNAKE GAME", "▶ BAŞLA tuşuna bas...",
                    Color.FromArgb(0, 245, 212), Color.FromArgb(150, 180, 220));

            if (_stateMgr.Is(GameState.Paused))
                _renderer.DrawCenteredText(g,
                    "DURAKLATILDI", "Devam etmek için P veya butona bas",
                    Color.FromArgb(180, 120, 255), Color.FromArgb(150, 180, 220));

            if (_stateMgr.Is(GameState.GameOver))
                _renderer.DrawCenteredText(g,
                    "OYUN BİTTİ", $"Skor: {_scoreMgr.Score}",
                    Color.FromArgb(247, 37, 133), Color.FromArgb(200, 220, 255));

            _renderer.DrawBorder(g);

            // Hazırlanan bitmap'i tek seferde ekrana kopyala
            e.Graphics.DrawImageUnscaled(_buffer, 0, 0);
        }

        // ── Klavye olayları ───────────────────────────────────────────
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up:    _snake?.SetDirection(Dir.Up);    break;
                case Keys.S: case Keys.Down:  _snake?.SetDirection(Dir.Down);  break;
                case Keys.A: case Keys.Left:  _snake?.SetDirection(Dir.Left);  break;
                case Keys.D: case Keys.Right: _snake?.SetDirection(Dir.Right); break;
                case Keys.P:
                case Keys.Escape:             TogglePause(); break;
                case Keys.R:                  StartGame();   break;
            }
            e.Handled = true; // tuşun başka kontrole iletilmesini engelle
        }

        // Sağ paneldeki etiketleri güncel değerlerle yazar
        private void UpdateUI()
        {
            _lblScore.Text = _scoreMgr.Score.ToString("N0");
            _lblHigh.Text  = _scoreMgr.HighScore.ToString("N0");
            _lblLevel.Text = _scoreMgr.LevelDisplay;
        }

        private void OpenLeaderboard()
        {
            using var lb = new LeaderboardForm(_scoreMgr, this);
            lb.ShowDialog(this);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer.Stop();
            base.OnFormClosing(e);
        }
    }
}
