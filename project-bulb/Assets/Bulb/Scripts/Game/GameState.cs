using System;

namespace Bulb.Game
{
    public class GameState
    {
        public static event EventHandler<StateChangedEventArgs> StateChanged;

        private static GameStates _currentState;
        public static GameStates CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState == value)
                    return;

                _currentState = value;
                if (StateChanged != null)
                    StateChanged(null, new StateChangedEventArgs(value));
            }
        }
    }

    public enum GameStates
    {
        MainMenu,
        LevelSelection,
        Editor,
        Game
    }

    public class StateChangedEventArgs : EventArgs
    {
        public GameStates NewState { get; set; }

        public StateChangedEventArgs(GameStates newState)
        {
            NewState = newState;
        }
    }
}