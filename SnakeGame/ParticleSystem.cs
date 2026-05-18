using System;
using System.Collections.Generic;
using System.Drawing;

namespace SnakeGame
{
    // Tek bir parçacık — konum, hız, renk ve kalan ömür
    public class Particle
    {
        public float X, Y;       // ekran koordinatı
        public float Vx, Vy;     // hız (piksel/frame)
        public float Life;       // kalan ömür (frame sayısı)
        public float MaxLife;    // başlangıç ömrü (saydamlık hesabı için)
        public Color Col;
    }

    // Yiyecek yeme ve ölüm anlarında görsel efekt sağlar.
    // Emit ile parçacık üretilir, Update ile hareket eder, Render ile çizilir.
    public class ParticleSystem
    {
        private readonly List<Particle> _particles = new();
        private static readonly Random Rng = new Random();

        // (cx, cy) merkezinden count kadar rastgele yönde parçacık fırlatır
        public void Emit(float cx, float cy, int count, Color col)
        {
            for (int i = 0; i < count; i++)
            {
                double angle = Rng.NextDouble() * Math.PI * 2;
                float  speed = (float)(Rng.NextDouble() * 3 + 1);

                _particles.Add(new Particle
                {
                    X       = cx,
                    Y       = cy,
                    Vx      = (float)Math.Cos(angle) * speed,
                    Vy      = (float)Math.Sin(angle) * speed,
                    Life    = 30 + Rng.Next(20),
                    MaxLife = 50,
                    Col     = col
                });
            }
        }

        // Tüm parçacıkları bir adım ilerletir; ömrü bitenleri siler
        public void Update()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.X  += p.Vx;
                p.Y  += p.Vy;
                p.Vx *= 0.92f; // sürtünme efekti
                p.Vy *= 0.92f;
                p.Life--;

                if (p.Life <= 0) _particles.RemoveAt(i);
            }
        }

        // Tüm parçacıkları ekrana çizer — ömrü azaldıkça küçülür ve solar
        public void Render(Graphics g)
        {
            foreach (var p in _particles)
            {
                float alpha = Math.Max(0, p.Life / p.MaxLife); // 0–1 arası saydamlık
                using var brush = new SolidBrush(
                    Color.FromArgb((int)(220 * alpha), p.Col));

                float size = Math.Max(1f, 4f * alpha);
                g.FillEllipse(brush, p.X - size / 2, p.Y - size / 2, size, size);
            }
        }
    }
}
