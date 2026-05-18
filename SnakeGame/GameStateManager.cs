using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame
{
    // Oyunun olabileceği durumlar
    public enum GameState { Idle, Playing, Paused, GameOver }

    // Hangi state'ten hangi state'e geçilebileceğini kontrol eden sınıf.
    // Geçersiz bir geçiş denenirse InvalidOperationException fırlatır.
    public class GameStateManager
    {
        private GameState _state = GameState.Idle;

        // Her state için izin verilen hedef state'ler (state machine)
        private static readonly Dictionary<GameState, GameState[]> Transitions = new()
        {
            [GameState.Idle]     = new[] { GameState.Playing },
            [GameState.Playing]  = new[] { GameState.Paused, GameState.GameOver, GameState.Idle },
            [GameState.Paused]   = new[] { GameState.Playing, GameState.Idle },
            [GameState.GameOver] = new[] { GameState.Idle }
        };

        public GameState State => _state;

        // Şu anki state verilen state ile eşleşiyor mu?
        public bool Is(GameState s) => _state == s;

        // Geçiş geçerliyse uygular, değilse exception fırlatır
        public void Transition(GameState next)
        {
            if (!Transitions.TryGetValue(_state, out var allowed) || !allowed.Contains(next))
                throw new InvalidOperationException(
                    $"Geçersiz state geçişi: {_state} → {next}");

            _state = next;
        }
    }
}
