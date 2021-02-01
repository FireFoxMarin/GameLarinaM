using Microsoft.Xna.Framework.Input;

namespace MatchThreeLarina.GameStateManagement
{
    public class Input
    {
        public const int MaxInputs = 1;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly KeyboardState[] LastKeyboardStates;


        public Input()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            LastKeyboardStates = new KeyboardState[MaxInputs];
        }

        public void Update()
        {
            for (var i = 0; i < MaxInputs; i++)
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
        }
    }
}
