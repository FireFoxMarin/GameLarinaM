using MatchThreeLarina.GameStateManagement;
using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using SpriteBatchMod;
using System;

namespace MatchThreeLarina.Gui.Screens
{
    internal class LoadingScreen : GameScreen
    {
        private const string message = "Loading";
        private readonly GameScreen[] screensToLoad;
        private bool otherScreensAreGone;

        private LoadingScreen(GameScreen[] screensToLoad)
        {
            this.screensToLoad = screensToLoad;
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
        }

        public static void Load(ScreenManager screenManager, params GameScreen[] screensToLoad)
        {
            foreach (var screen in screenManager.GetScreens())
                screen.ExitScreen();
            var loadingScreen = new LoadingScreen(screensToLoad);
            screenManager.AddScreen(loadingScreen);
        }

        public override void Update(GameTime time, bool otherScreenHasFocus,
            bool coveredByOtherScreen)
        {
            base.Update(time, otherScreenHasFocus, coveredByOtherScreen);

            if (otherScreensAreGone)
            {
                ScreenManager.RemoveScreen(this);

                foreach (var screen in screensToLoad)
                    if (screen != null)
                        ScreenManager.AddScreen(screen);
                ScreenManager.Game.ResetElapsedTime();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (ScreenState == ScreenState.Active &&
                ScreenManager.GetScreens().Length == 1)
                otherScreensAreGone = true;
            var spriteBatch = ScreenManager.SpriteBatch;

            var textPosition = GetTextDrawingPosition();

            spriteBatch.WrappedDraw(() =>
            {
                spriteBatch.DrawString(Resources.Font, message, textPosition, Color.Black * TransitionAlpha);
            });
        }

        private Vector2 GetTextDrawingPosition()
        {
            var viewport = ScreenManager.GraphicsDevice.Viewport;
            var viewportSize = new Vector2(viewport.Width, viewport.Height);
            var textSize = Resources.Font.MeasureString(message);
            var textPosition = (viewportSize - textSize) / 2;

            return textPosition;
        }
    }
}
