using System;

namespace SnakeGame
{
    // Oyuna özgü hatalar için kendi exception sınıfımız.
    // Bu sayede genel Exception'lardan ayırt edip sadece oyun hatalarını yakalayabiliriz.
    public class GameException : Exception
    {
        public GameException(string message) : base(message) { }
    }
}
