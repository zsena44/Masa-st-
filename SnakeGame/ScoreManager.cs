using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SnakeGame
{
    // Liderlik tablosundaki tek bir kaydı temsil eder
    public class ScoreEntry
    {
        public string Name  { get; set; }
        public int    Score { get; set; }
        public int    Level { get; set; }
        public string Date  { get; set; }
    }

    // Anlık skor, high score ve liderlik tablosunu yönetir.
    // Liderlik tablosu JSON dosyasına kaydedilip okunur (CRUD).
    public class ScoreManager
    {
        // Encapsulation: dışarıdan doğrudan değiştirilemesin
        private int _score;
        private int _highScore;
        private readonly List<ScoreEntry> _leaderboard;

        // Kayıt dosyasının adı — çalışma dizinine yazılır
        private const string SaveFile = "snakegame_scores.json";

        public int Score     => _score;
        public int HighScore => _highScore;

        // Level: her 100 puanda bir artar (1'den başlar)
        public int    Level        => _score / 100 + 1;
        public string LevelDisplay => Level.ToString();

        public ScoreManager()
        {
            _leaderboard = new List<ScoreEntry>();
            LoadLeaderboard(); // program açılınca dosyadan oku
        }

        // Skoru sıfırlar — yeni oyun başlarken çağrılır
        public void Reset()
        {
            _score = 0;
        }

        // Puan ekler ve gerekirse high score'u günceller
        public void AddPoints(int pts)
        {
            if (pts < 0) throw new ArgumentException("Puan negatif olamaz.");
            _score += pts;
            if (_score > _highScore) _highScore = _score;
        }

        // ── CRUD: Create ────────────────────────────────────────────
        // Yeni skor kaydı ekler, sıralar, fazla kayıtları atar, dosyaya yazar
        public void SaveEntry(string name, int score, int level)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("İsim boş olamaz.");

            _leaderboard.Add(new ScoreEntry
            {
                Name  = name.Trim(),
                Score = score,
                Level = level,
                Date  = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            });

            // Yüksek skordan düşüğe sırala
            _leaderboard.Sort((a, b) => b.Score.CompareTo(a.Score));

            // En fazla 20 kayıt tut
            if (_leaderboard.Count > 20)
                _leaderboard.RemoveRange(20, _leaderboard.Count - 20);

            WriteLeaderboard();
        }

        // ── CRUD: Read ──────────────────────────────────────────────
        // Listeyi salt okunur olarak dışarıya verir
        public IReadOnlyList<ScoreEntry> GetLeaderboard() => _leaderboard.AsReadOnly();

        // ── CRUD: Delete ─────────────────────────────────────────────
        // Belirtilen indeksteki kaydı siler
        public void DeleteEntry(int index)
        {
            if (index < 0 || index >= _leaderboard.Count)
                throw new IndexOutOfRangeException("Geçersiz skor indeksi.");

            _leaderboard.RemoveAt(index);
            WriteLeaderboard();
        }

        // ── Dosya I/O ────────────────────────────────────────────────
        // Dosya varsa içini okur ve listeye ekler
        private void LoadLeaderboard()
        {
            try
            {
                if (!File.Exists(SaveFile)) return;

                string json   = File.ReadAllText(SaveFile);
                var   entries = JsonSerializer.Deserialize<List<ScoreEntry>>(json);

                if (entries != null) _leaderboard.AddRange(entries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScoreManager] Yükleme hatası: {ex.Message}");
            }
        }

        // Tüm listeyi JSON formatında dosyaya yazar
        private void WriteLeaderboard()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(SaveFile, JsonSerializer.Serialize(_leaderboard, options));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScoreManager] Kaydetme hatası: {ex.Message}");
            }
        }
    }
}
