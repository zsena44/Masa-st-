using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ScoreGame
{
    // Düşen objeyi temsil eden sınıf
    class FallingItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Symbol { get; set; } // '*' veya 'O'
    }

    class Game
    {
        // --- Oyun alanı boyutları ---
        const int WIDTH = 40;
        const int HEIGHT = 20;

        // --- Oyuncu ---
        int playerX;
        int playerY;
        const char PLAYER_SYMBOL = '@';

        // --- Düşen objeler ---
        List<FallingItem> items = new List<FallingItem>();
        Random rng = new Random();

        // --- Skor & Zaman ---
        int score = 0;
        const int WIN_SCORE = 10;        // Bu skora ulaşınca oyun biter
        DateTime startTime;
        const int GAME_SECONDS = 30;     // Süre dolunca oyun biter

        // --- Spawn kontrolü ---
        int spawnCounter = 0;
        const int SPAWN_EVERY = 3;       // Her 3 güncellemede bir yeni obje

        // --- Log ---
        StreamWriter logWriter;
        const string LOG_FILE = "game_log.txt";

        // --- Oyun döngüsü bayrağı ---
        bool running = true;

        // -------------------------------------------------------
        public void Run()
        {
            // Log dosyasını aç
            logWriter = new StreamWriter(LOG_FILE, append: false);
            logWriter.AutoFlush = true;

            // Konsol ayarları
            Console.CursorVisible = false;
            Console.Clear();

            // Başlangıç konumu (ortada, altta)
            playerX = WIDTH / 2;
            playerY = HEIGHT - 1;

            startTime = DateTime.Now;

            Log($"GAME_START → width={WIDTH} height={HEIGHT} winScore={WIN_SCORE} timeLimit={GAME_SECONDS}s");

            // Ana döngü — her iterasyon ~50 ms sürer
            while (running)
            {
                // 1) Tuş girişini oku (bloke etmeden)
                HandleInput();

                // 2) Oyun mantığını güncelle
                Update();

                // 3) Ekranı çiz
                Draw();

                // 4) Bitiş kontrolü
                CheckEndConditions();

                Thread.Sleep(50);
            }

            // Oyun sona erdi
            ShowGameOver();
            logWriter.Close();
        }

        // -------------------------------------------------------
        void HandleInput()
        {
            if (!Console.KeyAvailable) return;

            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            Log($"INPUT → key={key.Key} playerX={playerX} playerY={playerY}");

            int prevX = playerX;

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    if (playerX > 0) playerX--;
                    break;
                case ConsoleKey.RightArrow:
                    if (playerX < WIDTH - 1) playerX++;
                    break;
                case ConsoleKey.Escape:
                    running = false;
                    Log("INPUT → key=Escape → game aborted by player");
                    return;
            }

            // Konum değiştiyse logla
            if (playerX != prevX)
                Log($"MOVE → playerX={playerX} playerY={playerY}");
        }

        // -------------------------------------------------------
        void Update()
        {
            spawnCounter++;

            // Yeni obje doğur
            if (spawnCounter >= SPAWN_EVERY)
            {
                spawnCounter = 0;
                SpawnItem();
            }

            // Mevcut objeleri aşağı taşı
            List<FallingItem> toRemove = new List<FallingItem>();

            foreach (var item in items)
            {
                item.Y++;
                Log($"OBJECT_MOVE → symbol={item.Symbol} x={item.X} y={item.Y}");

                // Ekran dışına çıktıysa sil
                if (item.Y >= HEIGHT)
                {
                    toRemove.Add(item);
                    Log($"OBJECT_REMOVE → symbol={item.Symbol} x={item.X} (missed)");
                    continue;
                }

                // Çarpışma kontrolü
                Log($"COLLISION_CHECK → itemX={item.X} itemY={item.Y} playerX={playerX} playerY={playerY}");

                if (item.X == playerX && item.Y == playerY)
                {
                    score++;
                    toRemove.Add(item);
                    Log($"COLLISION → symbol={item.Symbol} x={item.X} y={item.Y} score={score}");
                    Log($"SCORE_UPDATE → score={score} winScore={WIN_SCORE}");
                }
            }

            foreach (var r in toRemove)
                items.Remove(r);
        }

        // -------------------------------------------------------
        void SpawnItem()
        {
            char symbol = (rng.Next(2) == 0) ? '*' : 'O';
            int x = rng.Next(0, WIDTH);

            items.Add(new FallingItem { X = x, Y = 0, Symbol = symbol });
            Log($"UPDATE → itemSpawned symbol={symbol} x={x} y=0");
        }

        // -------------------------------------------------------
        void Draw()
        {
            // Tüm konsolu yeniden çizmek yerine, değişen hücreleri yazıyoruz
            Console.SetCursorPosition(0, 0);

            // Boş bir tampon oluştur
            char[,] buffer = new char[HEIGHT, WIDTH];
            for (int r = 0; r < HEIGHT; r++)
                for (int c = 0; c < WIDTH; c++)
                    buffer[r, c] = ' ';

            // Oyuncuyu yerleştir
            buffer[playerY, playerX] = PLAYER_SYMBOL;

            // Objeleri yerleştir
            foreach (var item in items)
                if (item.Y >= 0 && item.Y < HEIGHT && item.X >= 0 && item.X < WIDTH)
                    buffer[item.Y, item.X] = item.Symbol;

            // Üst çerçeve
            Console.WriteLine("+" + new string('-', WIDTH) + "+");

            // Her satırı yaz
            for (int r = 0; r < HEIGHT; r++)
            {
                Console.Write("|");
                for (int c = 0; c < WIDTH; c++)
                    Console.Write(buffer[r, c]);
                Console.WriteLine("|");
            }

            // Alt çerçeve
            Console.WriteLine("+" + new string('-', WIDTH) + "+");

            // Durum çubuğu
            int elapsed = (int)(DateTime.Now - startTime).TotalSeconds;
            int remaining = Math.Max(0, GAME_SECONDS - elapsed);
            Console.WriteLine($"  SKOR: {score}/{WIN_SCORE}   SÜRE: {remaining}s   [← →] Hareket  [ESC] Çıkış  ");
        }

        // -------------------------------------------------------
        void CheckEndConditions()
        {
            int elapsed = (int)(DateTime.Now - startTime).TotalSeconds;

            if (score >= WIN_SCORE)
            {
                running = false;
                Log($"GAME_END → reason=WIN score={score} elapsed={elapsed}s");
            }
            else if (elapsed >= GAME_SECONDS)
            {
                running = false;
                Log($"GAME_END → reason=TIMEOUT score={score} elapsed={elapsed}s");
            }
        }

        // -------------------------------------------------------
        void ShowGameOver()
        {
            Console.Clear();
            Console.CursorVisible = true;

            int elapsed = (int)(DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════╗");
            Console.WriteLine("  ║         OYUN BİTTİ           ║");
            Console.WriteLine("  ╠══════════════════════════════╣");
            Console.WriteLine($"  ║  Toplam Skor : {score,3}            ║");
            Console.WriteLine($"  ║  Geçen Süre  : {elapsed,3}s           ║");

            if (score >= WIN_SCORE)
                Console.WriteLine("  ║  Sonuç       : KAZANDIN!    ║");
            else
                Console.WriteLine("  ║  Sonuç       : Süre doldu.   ║");

            Console.WriteLine("  ╚══════════════════════════════╝");
            Console.WriteLine($"\n  Log dosyası: {Path.GetFullPath(LOG_FILE)}");
            Console.WriteLine("\n  Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }

        // -------------------------------------------------------
        void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            logWriter.WriteLine($"[{timestamp}] {message}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new Game().Run();
        }
    }
}