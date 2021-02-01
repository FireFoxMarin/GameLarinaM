using Microsoft.Xna.Framework;
using System;

namespace MatchThreeLarina.GameStateManagement
{
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden
    }

    public abstract class GameScreen
    {
        private bool otherScreenHasFocus;

        public bool IsPopup { get; protected set; } = false;

        public TimeSpan TransitionOnTime { get; protected set; } = TimeSpan.Zero;

        public TimeSpan TransitionOffTime { get; protected set; } = TimeSpan.Zero;

        public float TransitionPosition { get; protected set; } = 1;

        public float TransitionAlpha => 1f - TransitionPosition;

        public ScreenState ScreenState { get; protected set; } = ScreenState.TransitionOn;

        public bool IsExiting { get; protected internal set; }

        public bool IsActive =>
            !otherScreenHasFocus &&
            (ScreenState == ScreenState.TransitionOn ||
             ScreenState == ScreenState.Active);

        public ScreenManager ScreenManager { get; internal set; }

        public virtual void Activate(bool instancePreserved)
        {
        }

        public virtual void Unload()
        {
        }

        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            this.otherScreenHasFocus = otherScreenHasFocus;

            if (IsExiting)
            {
                ScreenState = ScreenState.TransitionOff;

                if (!UpdateTransition(gameTime, TransitionOffTime, 1))
                    ScreenManager.RemoveScreen(this);
            }
            else if (coveredByOtherScreen)
            {
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                    ScreenState = ScreenState.TransitionOff;
                else
                    ScreenState = ScreenState.Hidden;
            }
            else
            {
                if (UpdateTransition(gameTime, TransitionOnTime, -1))
                    ScreenState = ScreenState.TransitionOn;
                else
                    ScreenState = ScreenState.Active;
            }
        }

        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

            TransitionPosition += transitionDelta * direction;

            if (direction < 0 && TransitionPosition <= 0 ||
                direction > 0 && TransitionPosition >= 1)
            {
                TransitionPosition = MathHelper.Clamp(TransitionPosition, 0, 1);
                return false;
            }

            return true;
        }

        public virtual void HandleInput(GameTime gameTime, Input input)
        {
        }

        public virtual void Draw(GameTime gameTime)
        {
        }

        public void ExitScreen()
        {
            if (TransitionOffTime == TimeSpan.Zero)
                ScreenManager.RemoveScreen(this);
            else
                IsExiting = true;
        }
    }
}
