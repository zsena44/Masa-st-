using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame
{
    // Liderlik tablosunu gösteren ayrı pencere.
    // ScoreManager üzerinden listeyi okur ve silme işlemi yapabilir (CRUD UI).
    public class LeaderboardForm : Form
    {
        private readonly ScoreManager _scoreMgr;
        private readonly ListView _listView;

        public LeaderboardForm(ScoreManager scoreMgr, Form owner)
        {
            _scoreMgr = scoreMgr;

            Text            = "🏆 LİDERLİK TABLOSU";
            Size            = new Size(540, 460);
            BackColor       = Color.FromArgb(17, 24, 39);
            ForeColor       = Color.FromArgb(224, 240, 255);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // Liste görünümü — sütunlar: sıra, isim, skor, level, tarih
            _listView = new ListView
            {
                FullRowSelect = true,
                GridLines     = false,
                View          = View.Details,
                Location      = new Point(12, 12),
                Size          = new Size(508, 340),
                BackColor     = Color.FromArgb(13, 18, 32),
                ForeColor     = Color.FromArgb(200, 230, 255),
                Font          = new Font("Consolas", 10),
                BorderStyle   = BorderStyle.FixedSingle
            };
            _listView.Columns.Add("#",     36);
            _listView.Columns.Add("İSİM", 140);
            _listView.Columns.Add("SKOR", 100);
            _listView.Columns.Add("LEVEL", 70);
            _listView.Columns.Add("TARİH", 148);
            Controls.Add(_listView);

            // Seçili kaydı silen buton
            var btnDelete = new Button
            {
                Text      = "Seçileni Sil",
                Location  = new Point(12, 364),
                Size      = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(247, 80, 100),
                BackColor = Color.FromArgb(17, 24, 39)
            };
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(80, 30, 40);
            btnDelete.Click += (_, __) =>
            {
                if (_listView.SelectedIndices.Count == 0) return;

                int idx = _listView.SelectedIndices[0];
                try
                {
                    _scoreMgr.DeleteEntry(idx);
                    Populate(); // listeyi yenile
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Silme Hatası");
                }
            };
            Controls.Add(btnDelete);

            var btnClose = new Button
            {
                Text      = "Kapat",
                Location  = new Point(400, 364),
                Size      = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(0, 245, 212),
                BackColor = Color.FromArgb(17, 24, 39)
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(30, 60, 95);
            btnClose.Click += (_, __) => Close();
            Controls.Add(btnClose);

            Populate();
        }

        // ScoreManager'dan listeyi çekip ListView'e doldurur
        private void Populate()
        {
            _listView.Items.Clear();
            var lb = _scoreMgr.GetLeaderboard();

            string[] medals = { "🥇", "🥈", "🥉" };

            for (int i = 0; i < lb.Count; i++)
            {
                var e = lb[i];

                // İlk 3'e madalya, diğerlerine numara
                string rank = i < 3 ? medals[i] : $"{i + 1}.";

                var item = new ListViewItem(rank);
                item.SubItems.Add(e.Name);
                item.SubItems.Add(e.Score.ToString("N0"));
                item.SubItems.Add($"Lvl {e.Level}");
                item.SubItems.Add(e.Date);

                // İlk 3'e altın/gümüş/bronz rengi, diğerlerine standart renk
                item.ForeColor = i == 0 ? Color.Gold
                               : i == 1 ? Color.Silver
                               : i == 2 ? Color.FromArgb(210, 140, 70)
                               :          Color.FromArgb(180, 200, 230);

                _listView.Items.Add(item);
            }

            // Liste boşsa bilgilendirici satır ekle
            if (lb.Count == 0)
            {
                var empty = new ListViewItem("–");
                empty.SubItems.Add("Henüz skor yok");
                empty.ForeColor = Color.FromArgb(80, 100, 130);
                _listView.Items.Add(empty);
            }
        }
    }
}
