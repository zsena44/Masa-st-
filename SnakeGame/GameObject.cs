using System.Drawing;

namespace SnakeGame
{
    // Tüm oyun nesnelerinin türediği soyut temel sınıf (inheritance).
    // Update() ve Render() her alt sınıfta ayrı ayrı override edilmek zorunda.
    public abstract class GameObject
    {
        // Encapsulation: konum sadece bu sınıf ve alt sınıflar tarafından değiştirilebilir
        private Point2D _position;

        public Point2D Position
        {
            get => _position;
            protected set => _position = value;
        }

        protected GameObject(Point2D startPos)
        {
            _position = startPos;
        }

        // Her oyun adımında çağrılır — nesnenin mantığını günceller
        public abstract void Update();

        // Her frame çağrılır — nesneyi ekrana çizer
        public abstract void Render(Graphics g, int cellSize);
    }
}
